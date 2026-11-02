using Engine.Core.Math;
using Engine.Core.Time;
using Engine.Graphics.Commands;
using Engine.UI.Actions;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Combat;
using GameProject.Combat.Battlefield;
using GameProject.Combat.Cards;
using GameProject.Combat.UI;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;
using GameProject.Game.Operations;
using GameProject.Game.Scenes.Modals;

namespace GameProject.Game.Scenes;

internal sealed class BattleScene : IGameScene
{
    private const int CardsPerTurn = 5;
    private const int ActionPointsPerTurn = 3;
    private const int BetweenBattleRecoveryPercent = 50;
    private readonly CampaignState _campaign;
    private readonly ArmyDeploymentState _deployment;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;

    private UiCanvas? _canvas;
    private UiCanvas? _handCanvas;

    private UiPanel? _backgroundPanel;
    private UiPanel? _handBackground;

    private UiLabel? _battleInfoLabel;
    private UiLabel? _actionPointsLabel;
    private UiLabel? _deckInfoLabel;

    private UiButton? _endTurnButton;

    private BattleDirectionDropTarget? _leftTarget;
    private BattleDirectionDropTarget? _centerTarget;
    private BattleDirectionDropTarget? _rightTarget;

    private BattlefieldState? _battlefield;
    private CardDeck? _deck;
    private CombatTurnState? _turn;

    private BattleVictoryModal? _victoryModal;
    private BattleDefeatModal? _defeatModal;

    private bool _resultModalShown;

    public BattleScene(
        CampaignState campaign,
        ArmyDeploymentState deployment)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        ArgumentNullException.ThrowIfNull(deployment);

        _campaign = campaign;
        _deployment = deployment;
    }

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;

        var operation = _campaign.CurrentOperation
            ?? throw new InvalidOperationException(
                "Battle cannot start without an active operation.");

        _ui = context.CreateUiSystem();

        _uiActions = new UiActionRuntime(
            context.Runtime.Events,
            context.UiResources);

        _victoryModal = new BattleVictoryModal(
            context,
            _ui,
            _uiActions,
            ReturnToHeadquarters);

        _defeatModal = new BattleDefeatModal(
            context,
            _ui,
            _uiActions,
            ReturnToHeadquarters);

        _battlefield = BattlefieldFactory.CreatePrototype(
            _deployment,
            operation.CurrentBattle.EnemyForces);

        _deck = CreateDeck();

        _turn = new CombatTurnState(
            ActionPointsPerTurn);

        _turn.BeginTurn();

        _deck.DrawCards(CardsPerTurn);

        BuildBattleUi();

        RefreshBattleUi();
        RefreshHandUi(context.Host.WindowWidth);

        context.Host.Resized += OnHostResized;

        Console.WriteLine("Battle scene initialized.");
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

        _victoryModal?.Dispose();
        _victoryModal = null;

        _defeatModal?.Dispose();
        _defeatModal = null;

        if (_ui is not null && _canvas is not null)
        {
            _ui.Root.RemoveChild(_canvas);
        }

        _uiActions?.Dispose();
        _uiActions = null;

        _leftTarget = null;
        _centerTarget = null;
        _rightTarget = null;

        _battlefield = null;
        _deck = null;
        _turn = null;

        _handCanvas = null;
        _canvas = null;

        _backgroundPanel = null;
        _handBackground = null;

        _battleInfoLabel = null;
        _actionPointsLabel = null;
        _deckInfoLabel = null;

        _endTurnButton = null;

        _ui = null;
        _context = null;
        _requestScene = null;

        Console.WriteLine("Battle scene disposed.");
    }

    private void BuildBattleUi()
    {
        var context = _context
            ?? throw new InvalidOperationException();

        _canvas = new UiCanvas();

        _ui!.Root.AddChild(_canvas);

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

        var operation = _campaign.CurrentOperation
            ?? throw new InvalidOperationException();

        var operationTitle =
            _campaign.Stage == CampaignStage.FinalOperation
                ? "ФИНАЛЬНАЯ ОПЕРАЦИЯ"
                : $"ОПЕРАЦИЯ {_campaign.CurrentOperationNumber} / " +
                  $"{_campaign.RegularOperationCount}";

        AddAnchoredLabel(
            $"{operationTitle} — {operation.CurrentBattle.Name}",
            UiAnchor.TopCenter,
            new Vector2(0, 18),
            1000,
            36,
            22);

        _battleInfoLabel = AddAnchoredLabel(
            "",
            UiAnchor.TopCenter,
            new Vector2(0, 60),
            900,
            30,
            16);

        _actionPointsLabel = AddAnchoredLabel(
            "",
            UiAnchor.TopLeft,
            new Vector2(24, 65),
            260,
            30,
            16);

        _deckInfoLabel = AddAnchoredLabel(
            "",
            UiAnchor.TopRight,
            new Vector2(-24, 65),
            260,
            30,
            16);

        _leftTarget = CreateDirectionTarget(
            BattleDirection.LeftFlank,
            "ЛЕВЫЙ ФЛАНГ",
            -370);

        _centerTarget = CreateDirectionTarget(
            BattleDirection.Center,
            "ЦЕНТР",
            0);

        _rightTarget = CreateDirectionTarget(
            BattleDirection.RightFlank,
            "ПРАВЫЙ ФЛАНГ",
            370);

        _handBackground = new UiPanel
        {
            Width = context.Host.WindowWidth,
            Height = 195,
            Background = new UiColor(27, 32, 42),
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.BottomCenter,
            ZIndex = -10
        };

        _canvas.AddChild(_handBackground);

        _handCanvas = new UiCanvas
        {
            Height = 190,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.BottomCenter
        };

        _canvas.AddChild(_handCanvas);

        _endTurnButton = new UiButton("ЗАВЕРШИТЬ ХОД")
        {
            Width = 190,
            Height = 46,
            FontSize = 16,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.BottomRight,
            Offset = new Vector2(-24, -210)
        };

        _endTurnButton.Clicked += EndTurn;

        _canvas.AddChild(_endTurnButton);
    }

    private BattleDirectionDropTarget CreateDirectionTarget(
        BattleDirection direction,
        string title,
        float offsetX)
    {
        var target = new BattleDirectionDropTarget(
            direction,
            title,
            TryPlayCard)
        {
            Width = 350,
            Height = 320,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = new Vector2(offsetX, 120)
        };

        _canvas!.AddChild(target);

        return target;
    }

    private UiLabel AddAnchoredLabel(
        string text,
        UiAnchor anchor,
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
            Anchor = anchor,
            Offset = offset
        };

        _canvas!.AddChild(label);

        return label;
    }

    private static CardDeck CreateDeck()
    {
        var cards = new List<CardDefinition>();

        // Пять копий каждой тактической карты.
        for (var i = 0; i < 5; i++)
        {
            foreach (var card in TacticalCardCatalog.All)
            {
                cards.Add(card);
            }
        }

        var deck = new CardDeck(cards);

        deck.ShuffleDrawPile();

        return deck;
    }

    private void RefreshHandUi(int? viewportWidth = null)
    {
        if (_handCanvas is null || _deck is null)
            return;

        _handCanvas.ClearChildren();

        var hand = _deck.Hand;
        var width = viewportWidth ??
            _context?.Host.WindowWidth ?? 1280;

        if (hand.Count == 0)
        {
            var emptyLabel = new UiLabel("НЕТ КАРТ В РУКЕ")
            {
                Width = 300,
                Height = 40,
                FontSize = 16,
                LayoutMode = UiLayoutMode.Anchor,
                Anchor = UiAnchor.Center
            };

            _handCanvas.AddChild(emptyLabel);

            return;
        }

        const float cardWidth = 142;
        const float cardSpacing = 12;

        var totalWidth =
            hand.Count * cardWidth +
            (hand.Count - 1) * cardSpacing;

        var startX = Math.Max(
            12,
            (width - totalWidth) / 2);

        for (var i = 0; i < hand.Count; i++)
        {
            var cardView = new CardView(hand[i])
            {
                ZIndex = i
            };

            _handCanvas.AddChild(cardView);

            _handCanvas.SetPosition(
                cardView,
                new Vector2(
                    startX + i * (cardWidth + cardSpacing),
                    5));
        }
    }

    private bool TryPlayCard(
        BattleDirection direction,
        CardDefinition card)
    {
        if (_battlefield is null ||
            _deck is null ||
            _turn is null ||
            _battlefield.IsFinished)
        {
            return false;
        }

        var directionState = GetDirectionState(direction);

        if (directionState.IsFinished)
            return false;

        var handIndex = -1;

        for (var i = 0; i < _deck.Hand.Count; i++)
        {
            if (_deck.Hand[i].Id == card.Id)
            {
                handIndex = i;
                break;
            }
        }

        if (handIndex < 0)
            return false;

        if (_turn.AvailableActionPoints < card.ActionCost)
            return false;

        // Сначала проверяем и ставим эффект.
        // Если направление недоступно, карта не оплачивается.
        switch (card.Kind)
        {
            case CardKind.ArtilleryStrike:
                _battlefield.QueueArtilleryStrike(
                    direction,
                    card.Value);
                break;

            case CardKind.SmokeScreen:
                _battlefield.QueueSmokeScreen(
                    direction,
                    card.Value);
                break;

            case CardKind.Supply:
                _battlefield.QueueSupply(
                    direction,
                    card.Value);
                break;

            default:
                return false;
        }

        _turn.TrySpendActionPoints(card.ActionCost);

        _deck.DiscardFromHand(handIndex);

        RefreshHandUi();
        RefreshBattleUi();

        return true;
    }

    private void EndTurn()
    {
        if (_battlefield is null ||
            _deck is null ||
            _turn is null ||
            _battlefield.IsFinished)
        {
            return;
        }

        // Неиспользованные карты уходят в сброс.
        _deck.DiscardHand();

        // Разрешаем один боевой раунд на всех направлениях.
        _battlefield.ResolveStep();

        // Новый ход начинается только если бой продолжается.
        if (!_battlefield.IsFinished)
        {
            _turn.BeginTurn();
            _deck.DrawCards(CardsPerTurn);
        }

        RefreshHandUi();
        RefreshBattleUi();
    }

    private BattleDirectionState GetDirectionState(
        BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.LeftFlank =>
                _battlefield!.LeftFlank,

            BattleDirection.Center =>
                _battlefield!.Center,

            BattleDirection.RightFlank =>
                _battlefield!.RightFlank,

            _ => throw new ArgumentOutOfRangeException(
                nameof(direction))
        };
    }

    private void RefreshBattleUi()
    {
        if (_battlefield is null ||
            _turn is null)
        {
            return;
        }

        var operation = _campaign.CurrentOperation
            ?? throw new InvalidOperationException(
                "There is no active operation.");

        var operationTitle =
    _campaign.Stage == CampaignStage.FinalOperation
        ? "ФИНАЛЬНАЯ ОПЕРАЦИЯ"
        : $"ОПЕРАЦИЯ {_campaign.CurrentOperationNumber}";

        _battleInfoLabel.Text =
            $"{operationTitle} | " +
            $"СРАЖЕНИЕ {operation.CurrentBattleNumber}/" +
            $"{OperationState.TotalBattleCount} | " +
            $"ХОД {_turn!.TurnNumber} | " +
            $"КОНТРОЛЬ: ИГРОК {_battlefield.PlayerControlledDirections}/2 | " +
            $"ПРОТИВНИК {_battlefield.EnemyControlledDirections}/2";

        _actionPointsLabel!.Text =
            $"ОЧКИ ДЕЙСТВИЙ: " +
            $"{_turn.AvailableActionPoints}/{_turn.MaxActionPoints}";

        _deckInfoLabel!.Text =
            $"КОЛОДА: {_deck!.DrawPileCount} | " +
            $"СБРОС: {_deck.DiscardPileCount}";

        RefreshDirectionTarget(
            _leftTarget!,
            _battlefield.LeftFlank);

        RefreshDirectionTarget(
            _centerTarget!,
            _battlefield.Center);

        RefreshDirectionTarget(
            _rightTarget!,
            _battlefield.RightFlank);

        _endTurnButton!.Enabled = !_battlefield.IsFinished;

        if (_resultModalShown)
            return;

        switch (_battlefield.Result)
        {
            case BattlefieldResult.PlayerVictory:
                _resultModalShown = true;
                _victoryModal?.Show();
                break;

            case BattlefieldResult.PlayerDefeat:
                _resultModalShown = true;
                _defeatModal?.Show();
                break;
        }
    }

    private static void RefreshDirectionTarget(
    BattleDirectionDropTarget target,
    BattleDirectionState state)
    {
        var player = state.PlayerDetachment;
        var enemy = state.EnemyDetachment;

        var effects = string.Empty;

        if (state.PendingArtilleryDamage > 0)
        {
            effects +=
                $"АРТИЛЛЕРИЯ: +{state.PendingArtilleryDamage} урона\n";
        }

        if (state.PendingSmokeReductionPercent > 0)
        {
            effects +=
                $"ДЫМ: -{state.PendingSmokeReductionPercent}% урона противника\n";
        }

        if (state.PendingSupplyHealing > 0)
        {
            effects +=
                $"СНАБЖЕНИЕ: до +{state.PendingSupplyHealing} HP\n";
        }

        if (string.IsNullOrEmpty(effects))
        {
            effects = "ЭФФЕКТЫ: НЕТ";
        }
        else
        {
            effects = "ПОДГОТОВЛЕНО:\n" + effects.TrimEnd();
        }

        target.SetState(
            $"ИГРОК: {player.Definition.Name}\n" +
            $"ЗДОРОВЬЕ: {player.Health}/{player.MaxHealth}\n" +
            $"АТАКА: {player.Attack}",

            $"ПРОТИВНИК: {enemy.Definition.Name}\n" +
            $"ЗДОРОВЬЕ: {enemy.Health}/{enemy.MaxHealth}\n" +
            $"АТАКА: {enemy.Attack}",

            $"ПРИКАЗ: {player.Order?.Name ?? "—"}",

            effects,

            state.Control,
            state.IsFinished);
    }

    private void OnHostResized(
        int width,
        int height)
    {
        _ui?.Resize(width, height);

        if (_backgroundPanel is not null)
        {
            _backgroundPanel.Width = width;
            _backgroundPanel.Height = height;
        }

        if (_handBackground is not null)
        {
            _handBackground.Width = width;
        }

        RefreshHandUi(width);
    }

    private void ReturnToHeadquarters(CombatResult result)
    {
        var operation = _campaign.CurrentOperation
            ?? throw new InvalidOperationException(
                "There is no active operation.");

        if (_deployment.GetDetachmentState("infantry").Health == 0)
        {
            operation.FailAdditionalObjective();
        }

        if (result == CombatResult.PlayerVictory)
        {
            var operationCompleted =
                operation.RegisterBattleVictory();

            if (operationCompleted)
            {
                _campaign.CompleteCurrentOperation(
                    OperationOutcome.Victory);

                _requestScene?.Invoke(
                    new OperationReportScene(_campaign, operation));

                return;
            }

            _deployment.RecoverBetweenBattles(
                 BetweenBattleRecoveryPercent);
            // Следующее сражение той же операции.
            _requestScene?.Invoke(
                new BattleScene(_campaign, _deployment));

            return;
        }

        if (result == CombatResult.PlayerDefeat)
        {
            operation.RegisterBattleDefeat();

            _campaign.CompleteCurrentOperation(
                OperationOutcome.Defeat);

            _requestScene?.Invoke(
                new OperationReportScene(_campaign, operation));

            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(result),
            result,
            "Cannot leave an unfinished battle.");
    }
}