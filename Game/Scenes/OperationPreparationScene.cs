
using Engine.Core.Math;
using Engine.Core.Time;
using Engine.Graphics.Commands;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Combat.Battlefield;
using GameProject.Combat.Cards;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;
using GameProject.Game.Operations;

namespace GameProject.Game.Scenes;

internal sealed class OperationPreparationScene : IGameScene
{
    private static readonly BattleDirection[] Directions =
    [
        BattleDirection.LeftFlank,
        BattleDirection.Center,
        BattleDirection.RightFlank
    ];

    private readonly IReadOnlyList<CardDefinition> _availableCards;

    private UiButton? _deckEditorButton;
    private UiButton? _continueButton;

    private UiPanel? _deckEditorOverlay;
    private UiPanel? _deckEditorPanel;

    private UiLabel? _deckEditorCountLabel;
    private UiLabel? _deckEditorStatusLabel;

    private UiButton? _deckApplyButton;

    private readonly Dictionary<string, int> _deckCounts =
        new(StringComparer.Ordinal);

    private readonly Dictionary<string, UiLabel> _deckCountLabels =
        new(StringComparer.Ordinal);

    private readonly CampaignState _campaign;
    private readonly OperationState _operation;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiCanvas? _canvas;
    private UiPanel? _backgroundPanel;

    private UiLabel? _resourcesLabel;
    private UiLabel? _informationLabel;

    private UiDropdown? _directionDropdown;
    private UiDropdown? _terrainDropdown;

    private readonly Dictionary<string, UiButton> _purchaseButtons =
        new(StringComparer.Ordinal);

    private string _statusText =
        "Можно приобрести несколько преимуществ, пока хватает ресурсов.";

    public OperationPreparationScene(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        _campaign = campaign;
        _availableCards = campaign.Progression.GetAvailableCards();
        _operation = campaign.CurrentOperation
            ?? throw new ArgumentException(
                "An active operation is required.",
                nameof(campaign));

        if (_operation.IsCompleted ||
            _operation.CompletedBattleCount > 0)
        {
            throw new ArgumentException(
                "Preparation is only available before the first battle.",
                nameof(campaign));
        }
    }

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;

        _ui = context.CreateUiSystem();
        _canvas = new UiCanvas();

        _ui.Root.AddChild(_canvas);

        _backgroundPanel = new UiPanel
        {
            Width = context.Host.WindowWidth,
            Height = context.Host.WindowHeight,
            Background = new UiColor(18, 22, 30),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopLeft,
            ZIndex = -100
        };

        _canvas.AddChild(_backgroundPanel);

        BuildUi();

        RefreshUi();

        CampaignSaveStore.Save(_campaign);

        context.Host.Resized += OnHostResized;
    }

    private void BuildUi()
    {
        AddLabel(
            "ПОДГОТОВКА ОПЕРАЦИИ",
            new Vector2(0, 30),
            850,
            45,
            30);

        AddLabel(
            $"{_operation.Definition.Name}\n" +
            $"{_operation.Definition.EnemyBriefing}",
            new Vector2(0, 85),
            1050,
            55,
            15);

        _resourcesLabel = AddLabel(
            "",
            new Vector2(0, 145),
            500,
            30,
            19);

        // Фон карточек подготовки.
        AddCardPanel(-300, 250, 560);
        AddCardPanel(300, 250, 560);
        AddCardPanel(-300, 440, 560);
        AddCardPanel(300, 440, 560);

        // Разведка.
        AddLabel(
            "РАЗВЕДКА\n" +
            "Раскрывает состав противника во всех четырёх боях операции.\n" +
            "Стоимость: 2 ресурса.",
            new Vector2(-300, 190),
            510,
            85,
            16);

        AddPurchaseButton(
            OperationPreparationCatalog.ReconnaissanceId,
            -300,
            285);

        // Инженерная подготовка.
        AddLabel(
            "ИНЖЕНЕРНАЯ ПОДГОТОВКА\n" +
            "Выбрать местность одного направления в первом бою.\n" +
            "Стоимость: 3 ресурса.",
            new Vector2(300, 170),
            510,
            55,
            14);

        _directionDropdown = CreateDropdown(
            ["Левый фланг", "Центр", "Правый фланг"],
            300,
            230);

        _terrainDropdown = CreateDropdown(
            BattleTerrainCatalog.All
                .Select(terrain => terrain.Name)
                .ToArray(),
            300,
            275);

        AddTerrainPurchaseButton();

        // Медицинская группа.
        AddLabel(
            "МЕДИЦИНСКАЯ ГРУППА\n" +
            "Восстановление между боями увеличивается с 50% до 75%.\n" +
            "Стоимость: 2 ресурса.",
            new Vector2(-300, 390),
            510,
            70,
            15);

        AddPurchaseButton(
            OperationPreparationCatalog.MedicalSupportId,
            -300,
            475);

        // Резерв снабжения.
        AddLabel(
            "РЕЗЕРВ СНАБЖЕНИЯ\n" +
            "Добавляет одну дополнительную карту снабжения в каждый бой.\n" +
            "Стоимость: 2 ресурса.",
            new Vector2(300, 390),
            510,
            70,
            15);

        AddPurchaseButton(
            OperationPreparationCatalog.SupplyReserveId,
            300,
            475);

        _informationLabel = AddLabel(
            "",
            new Vector2(0, 535),
            1200,
            100,
            12);

        AddButton(
            "В ШТАБ",
            UiAnchor.BottomCenter,
            new Vector2(-150, -28),
            ReturnToHeadquarters);

        _deckEditorButton = AddButton(
            "СОСТАВ КОЛОДЫ",
            UiAnchor.BottomCenter,
            new Vector2(0, -82),
            OpenDeckEditor);

        _continueButton = AddButton(
            "К РАССТАНОВКЕ ОТРЯДОВ",
            UiAnchor.BottomCenter,
            new Vector2(150, -28),
            ContinueToDeployment);
    }

    private void OpenDeckEditor()
    {
        if (_deckEditorOverlay is not null ||
            _context is null ||
            _canvas is null)
        {
            return;
        }

        _deckCounts.Clear();
        _deckCountLabels.Clear();

        foreach (var card in _availableCards)
        {
            _deckCounts[card.Id] =
                _operation.SelectedCardIds.Count(
                    id => id == card.Id);
        }

        _deckEditorOverlay = new UiPanel
        {
            Width = _context.Host.WindowWidth,
            Height = _context.Host.WindowHeight,
            Background = new UiColor(12, 16, 22),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopLeft,
            ZIndex = 1000
        };

        _canvas.AddChild(_deckEditorOverlay);

        _deckEditorPanel = new UiPanel
        {
            Width = 1100,
            Height = 620,
            Background = new UiColor(31, 38, 49),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.Center
        };

        _deckEditorOverlay.AddChild(_deckEditorPanel);

        AddDeckEditorLabel(
            "СОСТАВ КОЛОДЫ",
            new Vector2(0, 25),
            800,
            40,
            26);

        _deckEditorCountLabel = AddDeckEditorLabel(
            "",
            new Vector2(0, 75),
            500,
            30,
            20);

        for (var i = 0; i < _availableCards.Count; i++)
        {
            var card = _availableCards[i];
            var y = 135 + i * 70;

            AddDeckEditorLabel(
                $"{card.Title}\n{card.Description}  " +
                $"(максимум: {card.CopiesInDeck})",
                new Vector2(-150, y),
                620,
                58,
                14,
                UiHorizontalAlignment.Left);

            AddDeckEditorButton(
                "−",
                new Vector2(300, y + 5),
                44,
                () => ChangeDeckCount(card.Id, -1));

            var countLabel = AddDeckEditorLabel(
                "",
                new Vector2(370, y + 5),
                65,
                38,
                18);

            _deckCountLabels.Add(card.Id, countLabel);

            AddDeckEditorButton(
                "+",
                new Vector2(440, y + 5),
                44,
                () => ChangeDeckCount(card.Id, 1));
        }

        _deckEditorStatusLabel = AddDeckEditorLabel(
            "",
            new Vector2(0, 490),
            900,
            30,
            14);

        AddDeckEditorButton(
            "ОТМЕНА",
            new Vector2(-135, 550),
            210,
            CloseDeckEditor);

        _deckApplyButton = AddDeckEditorButton(
            "ПРИМЕНИТЬ",
            new Vector2(135, 550),
            240,
            ApplyDeckSelection);

        RefreshDeckEditorUi();
    }

    private UiLabel AddDeckEditorLabel(
    string text,
    Vector2 offset,
    float width,
    float height,
    float fontSize,
    UiHorizontalAlignment alignment =
        UiHorizontalAlignment.Center)
    {
        var label = new UiLabel(text)
        {
            Width = width,
            Height = height,
            FontSize = fontSize,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = offset,
            HorizontalAlignment = alignment,
            VerticalAlignment = UiVerticalAlignment.Center
        };

        _deckEditorPanel!.AddChild(label);

        return label;
    }

    private UiButton AddDeckEditorButton(
        string text,
        Vector2 offset,
        float width,
        Action onClick)
    {
        var button = new UiButton(text)
        {
            Width = width,
            Height = 44,
            FontSize = 18,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = offset
        };

        button.Clicked += onClick;

        _deckEditorPanel!.AddChild(button);

        return button;
    }

    private void ChangeDeckCount(
    string cardId,
    int delta)
    {
        if (!_deckCounts.TryGetValue(cardId, out var current))
            return;

        var card = _availableCards.First(
            item => item.Id == cardId);

        var total = _deckCounts.Values.Sum();

        if (delta > 0)
        {
            if (total >= OperationState.DeckSize)
            {
                _deckEditorStatusLabel!.Text =
                    "Колода уже содержит 10 карт.";

                return;
            }

            if (current >= card.CopiesInDeck)
            {
                _deckEditorStatusLabel!.Text =
                    "Достигнут лимит копий этой карты.";

                return;
            }
        }
        else if (current <= 0)
        {
            return;
        }

        _deckCounts[cardId] = current + delta;

        RefreshDeckEditorUi();
    }

    private void RefreshDeckEditorUi()
    {
        if (_deckEditorCountLabel is null ||
            _deckEditorStatusLabel is null ||
            _deckApplyButton is null)
        {
            return;
        }

        var total = _deckCounts.Values.Sum();

        _deckEditorCountLabel.Text =
            $"КАРТ В КОЛОДЕ: {total}/{OperationState.DeckSize}";

        foreach (var card in _availableCards)
        {
            _deckCountLabels[card.Id].Text =
                $"{_deckCounts[card.Id]} / {card.CopiesInDeck}";
        }

        _deckApplyButton.Enabled =
            total == OperationState.DeckSize;

        _deckEditorStatusLabel.Text =
            total == OperationState.DeckSize
                ? "Колода готова к операции."
                : $"Добавьте ещё {OperationState.DeckSize - total} карт.";
    }

    private void ApplyDeckSelection()
    {
        if (_deckCounts.Values.Sum() != OperationState.DeckSize)
            return;

        var selectedIds = new List<string>(
            OperationState.DeckSize);

        foreach (var card in _availableCards)
        {
            for (var i = 0; i < _deckCounts[card.Id]; i++)
            {
                selectedIds.Add(card.Id);
            }
        }

        if (!_campaign.TrySetOperationDeck(selectedIds))
        {
            _deckEditorStatusLabel!.Text =
                "Не удалось сохранить состав колоды.";

            return;
        }

        CampaignSaveStore.Save(_campaign);

        CloseDeckEditor();

        _statusText = "Состав колоды сохранён.";

        RefreshUi();
    }

    private void CloseDeckEditor()
    {
        if (_deckEditorOverlay is not null &&
            _canvas is not null)
        {
            _canvas.RemoveChild(_deckEditorOverlay);
        }

        _deckEditorOverlay = null;
        _deckEditorPanel = null;

        _deckEditorCountLabel = null;
        _deckEditorStatusLabel = null;
        _deckApplyButton = null;

        _deckCounts.Clear();
        _deckCountLabels.Clear();
    }

    private void AddTerrainPurchaseButton()
    {
        var preparation =
            OperationPreparationCatalog.FindById(
                OperationPreparationCatalog.TerrainSelectionId)
            ?? throw new InvalidOperationException(
                "Terrain preparation is missing.");

        var button = AddButton(
            $"КУПИТЬ — {preparation.Cost}",
            UiAnchor.TopCenter,
            new Vector2(300, 320),
            PurchaseTerrainSelection);

        _purchaseButtons.Add(preparation.Id, button);
    }
    private void AddCardPanel(
        float x,
        float y,
        float height)
    {
        var panel = new UiPanel
        {
            Width = 560,
            Height = height,
            Background = new UiColor(31, 38, 49),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.Center,
            Offset = new Vector2(x, y),
            ZIndex = -5
        };

        _canvas!.AddChild(panel);
    }

    private UiLabel AddLabel(
        string text,
        Vector2 offset,
        float width,
        float height,
        float fontSize)
    {
        var label = new UiLabel(text)
        {
            Width = width,
            Height = height,
            FontSize = fontSize,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = offset,
            HorizontalAlignment = UiHorizontalAlignment.Center,
            VerticalAlignment = UiVerticalAlignment.Center
        };

        _canvas!.AddChild(label);

        return label;
    }

    private UiButton AddButton(
        string text,
        UiAnchor anchor,
        Vector2 offset,
        Action onClick)
    {
        var button = new UiButton(text)
        {
            Width = 240,
            Height = 45,
            FontSize = 15,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = anchor,
            Offset = offset
        };

        button.Clicked += onClick;

        _canvas!.AddChild(button);

        return button;
    }

    private void AddPurchaseButton(
        string preparationId,
        float x,
        float y)
    {
        var preparation =
            OperationPreparationCatalog.FindById(preparationId)
            ?? throw new InvalidOperationException(
                $"Unknown preparation: {preparationId}");

        var button = AddButton(
            $"КУПИТЬ — {preparation.Cost}",
            UiAnchor.TopCenter,
            new Vector2(x, y),
            () => Purchase(preparationId));

        _purchaseButtons.Add(preparationId, button);
    }

    private UiDropdown CreateDropdown(
        IReadOnlyList<string> options,
        float x,
        float y)
    {
        var dropdown = new UiDropdown
        {
            Width = 360,
            Height = 38,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = new Vector2(x, y)
        };

        foreach (var option in options)
        {
            dropdown.AddOption(option);
        }

        dropdown.SetSelectedIndex(0);

        _canvas!.AddChild(dropdown);

        return dropdown;
    }

    private void Purchase(string preparationId)
    {
        if (!_campaign.TryPurchasePreparation(preparationId))
        {
            _statusText =
                "Не удалось приобрести преимущество. " +
                "Проверьте количество ресурсов.";

            RefreshUi();
            return;
        }

        CampaignSaveStore.Save(_campaign);

        _statusText = "Преимущество приобретено.";

        RefreshUi();
    }

    private void PurchaseTerrainSelection()
    {
        if (_directionDropdown is null ||
            _terrainDropdown is null)
        {
            return;
        }

        var directionIndex = _directionDropdown.SelectedIndex;
        var terrainIndex = _terrainDropdown.SelectedIndex;

        if ((uint)directionIndex >= (uint)Directions.Length ||
            (uint)terrainIndex >= (uint)BattleTerrainCatalog.All.Count)
        {
            _statusText = "Выберите направление и местность.";
            RefreshUi();
            return;
        }

        var direction = Directions[directionIndex];
        var terrain = BattleTerrainCatalog.All[terrainIndex];

        if (!_campaign.TryPurchasePreparation(
                OperationPreparationCatalog.TerrainSelectionId,
                direction,
                terrain.Id))
        {
            _statusText =
                "Не удалось приобрести инженерную подготовку.";

            RefreshUi();
            return;
        }

        CampaignSaveStore.Save(_campaign);

        _statusText =
            $"Инженерная подготовка: {terrain.Name}, " +
            $"{Directions[directionIndex]}.";

        RefreshUi();
    }

    private void RefreshUi()
    {
        if (_resourcesLabel is null ||
            _informationLabel is null)
        {
            return;
        }

        _resourcesLabel.Text =
            $"РЕСУРСЫ ШТАБА: {_campaign.Progression.Resources}";

        foreach (var preparation in OperationPreparationCatalog.All)
        {
            var button = _purchaseButtons[preparation.Id];

            var purchased =
                _operation.Preparation.HasPurchased(preparation.Id);

            button.Text = purchased
                ? "ПРИОБРЕТЕНО"
                : $"КУПИТЬ — {preparation.Cost}";

            button.Enabled =
                !purchased &&
                _campaign.Progression.CanAfford(preparation.Cost);
        }

        var terrainPurchased =
            _operation.Preparation.HasTerrainSelection;

        if (_directionDropdown is not null)
            _directionDropdown.Enabled = !terrainPurchased;

        if (_terrainDropdown is not null)
            _terrainDropdown.Enabled = !terrainPurchased;

        _informationLabel.Text =
            _operation.Preparation.HasReconnaissance
                ? BuildReconnaissanceReport()
                : _statusText;

        if (_deckEditorButton is not null)
        {
            _deckEditorButton.Text =
                $"СОСТАВ КОЛОДЫ: " +
                $"{_operation.SelectedCardIds.Count}/" +
                $"{OperationState.DeckSize}";
        }

        if (_continueButton is not null)
        {
            _continueButton.Enabled =
                _operation.HasCompleteDeck;
        }
    }

    private string BuildReconnaissanceReport()
    {
        var lines = new List<string>
        {
            "РАЗВЕДДАННЫЕ ПО ОПЕРАЦИИ:"
        };

        for (var i = 0; i < _operation.Definition.Battles.Count; i++)
        {
            var battle = _operation.Definition.Battles[i];
            var enemy = battle.EnemyForces;

            lines.Add(
                $"{i + 1}. {battle.Name}: " +
                $"{enemy.LeftFlank.Name} / " +
                $"{enemy.Center.Name} / " +
                $"{enemy.RightFlank.Name}. " +
                $"Цель: {battle.Objective.Name}.");
        }

        return string.Join("\n", lines);
    }

    private void ContinueToDeployment()
    {
        if (!ReferenceEquals(
                _campaign.CurrentOperation,
                _operation))
        {
            return;
        }

        if (!_operation.HasCompleteDeck)
        {
            OpenDeckEditor();
            return;
        }

        CampaignSaveStore.Save(_campaign);

        _requestScene?.Invoke(
            new ArmyDeploymentScene(
                _campaign,
                _operation.Definition));
    }

    private void ReturnToHeadquarters()
    {
        CampaignSaveStore.Save(_campaign);

        _requestScene?.Invoke(
            new HeadquartersScene(_campaign));
    }

    public void Update(TimeSnapshot time)
    {
        _ui?.Update(time.Delta.TotalSeconds);
    }

    public void Render(double interpolationAlpha)
    {
        _ui?.Render();
    }

    public void Dispose()
    {
        if (_context is not null)
        {
            _context.Host.Resized -= OnHostResized;
        }

        if (_ui is not null && _canvas is not null)
        {
            _ui.Root.RemoveChild(_canvas);
        }

        _purchaseButtons.Clear();

        _backgroundPanel = null;
        _resourcesLabel = null;
        _informationLabel = null;

        _directionDropdown = null;
        _terrainDropdown = null;

        _canvas = null;
        _ui = null;
        _context = null;
        _requestScene = null;

        _deckEditorOverlay = null;
        _deckEditorPanel = null;

        _deckEditorCountLabel = null;
        _deckEditorStatusLabel = null;
        _deckApplyButton = null;
        _deckEditorButton = null;
        _continueButton = null;

        _deckCounts.Clear();
        _deckCountLabels.Clear();
    }

    private void OnHostResized(int width, int height)
    {
        _ui?.Resize(width, height);

        if (_backgroundPanel is not null)
        {
            _backgroundPanel.Width = width;
            _backgroundPanel.Height = height;
        }

        if (_deckEditorOverlay is not null)
        {
            _deckEditorOverlay.Width = width;
            _deckEditorOverlay.Height = height;
        }
    }
}
