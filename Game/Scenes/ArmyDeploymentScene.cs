
using Engine.Core.Math;
using Engine.Core.Time;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class ArmyDeploymentScene : IGameScene
{
    private readonly CampaignState _campaign;
    private readonly ArmyDeploymentState _deployment;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;
    private UiSystem? _ui;
    private UiCanvas? _canvas;

    private UiDropdown? _leftDropdown;
    private UiDropdown? _centerDropdown;
    private UiDropdown? _rightDropdown;

    private UiLabel? _summaryLabel;
    private UiLabel? _messageLabel;

    private UiButton? _startButton;

    private bool _restoringSelection;

    private readonly Dictionary<BattleDirection, DetachmentOrderDefinition> _orders =
    new()
    {
        [BattleDirection.LeftFlank] = DetachmentOrders.Attack,
        [BattleDirection.Center] = DetachmentOrders.Attack,
        [BattleDirection.RightFlank] = DetachmentOrders.Attack
    };

    public ArmyDeploymentScene(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        _campaign = campaign;
        _deployment = new ArmyDeploymentState(
            DetachmentCatalog.CreateInitialRoster());
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

        AddLabel(
            "FORM ARMY",
            UiAnchor.TopCenter,
            new Vector2(0, 65),
            500,
            55,
            32);

        AddLabel(
            "Choose one detachment for each direction",
            UiAnchor.TopCenter,
            new Vector2(0, 125),
            700,
            35,
            18);

        _leftDropdown = CreateDropdown(
            BattleDirection.LeftFlank,
            "LEFT FLANK",
            210);

        _centerDropdown = CreateDropdown(
            BattleDirection.Center,
            "CENTER",
            305);

        _rightDropdown = CreateDropdown(
            BattleDirection.RightFlank,
            "RIGHT FLANK",
            400);

        _summaryLabel = AddLabel(
            "",
            UiAnchor.TopCenter,
            new Vector2(0, 495),
            850,
            100,
            17);

        _messageLabel = AddLabel(
            "",
            UiAnchor.TopCenter,
            new Vector2(0, 610),
            850,
            35,
            16);

        var backButton = new UiButton("BACK")
        {
            Width = 220,
            Height = 48,
            FontSize = 18
        };

        ConfigureAnchor(
            backButton,
            UiAnchor.BottomCenter,
            new Vector2(-160, -65));

        backButton.Clicked +=
            () => _requestScene?.Invoke(
                new HeadquartersScene(_campaign));

        _canvas.AddChild(backButton);

        _startButton = new UiButton("START BATTLE")
        {
            Width = 220,
            Height = 48,
            FontSize = 18,
            Enabled = false
        };

        ConfigureAnchor(
            _startButton,
            UiAnchor.BottomCenter,
            new Vector2(160, -65));

        _startButton.Clicked += StartBattle;

        _canvas.AddChild(_startButton);

        RefreshUi();

        context.Host.Resized += _ui.Resize;
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
        if (_context is not null && _ui is not null)
        {
            _context.Host.Resized -= _ui.Resize;

            if (_canvas is not null)
            {
                _ui.Root.RemoveChild(_canvas);
            }
        }

        _leftDropdown = null;
        _centerDropdown = null;
        _rightDropdown = null;
        _summaryLabel = null;
        _messageLabel = null;
        _startButton = null;

        _canvas = null;
        _ui = null;
        _context = null;
        _requestScene = null;
    }

    private UiDropdown CreateDropdown(
        BattleDirection direction,
        string label,
        float y)
    {
        AddLabel(
            label,
            UiAnchor.TopCenter,
            new Vector2(-280, y),
            220,
            44,
            18);

        var dropdown = new UiDropdown
        {
            Width = 370,
            Height = 44
        };

        ConfigureAnchor(
            dropdown,
            UiAnchor.TopCenter,
            new Vector2(150, y));

        dropdown.AddOption("— SELECT DETACHMENT —");

        foreach (var definition in _deployment.AvailableDetachments)
        {
            dropdown.AddOption(
                $"{definition.Name} | HP {definition.MaxHealth} | ATK {definition.Attack}");
        }

        dropdown.SetSelectedIndex(0);

        dropdown.SelectionChanged +=
            (index, _) => OnSelectionChanged(
                direction,
                dropdown,
                index);

        _canvas!.AddChild(dropdown);

        return dropdown;
    }

    private void OnSelectionChanged(
        BattleDirection direction,
        UiDropdown dropdown,
        int selectedIndex)
    {
        if (_restoringSelection)
        {
            return;
        }

        var previous = _deployment.GetAssigned(direction);

        _deployment.RemoveAssignment(direction);

        if (selectedIndex > 0)
        {
            var selected =
                _deployment.AvailableDetachments[selectedIndex - 1];

            if (!_deployment.TryAssign(direction, selected.Id))
            {
                if (previous is not null)
                {
                    _deployment.TryAssign(
                        direction,
                        previous.Id);
                }

                RestoreSelection(dropdown, previous);

                RefreshUi(
                    "This detachment is already assigned elsewhere.");

                return;
            }
        }

        RefreshUi();
    }

    private void RestoreSelection(
        UiDropdown dropdown,
        DetachmentDefinition? definition)
    {
        var index = 0;

        if (definition is not null)
        {
            index = _deployment.AvailableDetachments
                .ToList()
                .FindIndex(item => item.Id == definition.Id) + 1;
        }

        _restoringSelection = true;

        dropdown.SetSelectedIndex(index);

        _restoringSelection = false;
    }

    private void RefreshUi(string? message = null)
    {
        if (_summaryLabel is null ||
            _messageLabel is null ||
            _startButton is null)
        {
            return;
        }

        var left = _deployment.GetAssigned(
            BattleDirection.LeftFlank)?.Name ?? "Not selected";

        var center = _deployment.GetAssigned(
            BattleDirection.Center)?.Name ?? "Not selected";

        var right = _deployment.GetAssigned(
            BattleDirection.RightFlank)?.Name ?? "Not selected";

        _summaryLabel.Text =
            $"LEFT FLANK: {left}\n" +
            $"CENTER: {center}\n" +
            $"RIGHT FLANK: {right}";

        _startButton.Enabled = _deployment.IsReadyForBattle;

        _messageLabel.Text = message ??
            (_deployment.IsReadyForBattle
                ? "Army ready."
                : "Select three different detachments.");
    }

    public bool TryAssignOrder(
    BattleDirection direction,
    string orderId)
    {
        var order = DetachmentOrders.All
            .FirstOrDefault(item => item.Id == orderId);

        if (order is null || !_orders.ContainsKey(direction))
            return false;

        _orders[direction] = order;
        return true;
    }

    public DetachmentOrderDefinition GetAssignedOrder(
        BattleDirection direction)
    {
        return _orders.TryGetValue(direction, out var order)
            ? order
            : throw new ArgumentOutOfRangeException(nameof(direction));
    }

    private void StartBattle()
    {
        if (!_deployment.IsReadyForBattle ||
            _campaign.CurrentOperation is not null ||
            _campaign.AreRegularOperationsCompleted)
        {
            return;
        }

        _campaign.StartNextOperation();

        _requestScene?.Invoke(
            new BattleScene(_campaign, _deployment));
    }

    private UiLabel AddLabel(
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
            HorizontalAlignment = UiHorizontalAlignment.Left,
            VerticalAlignment = UiVerticalAlignment.Top
        };

        ConfigureAnchor(label, anchor, offset);

        _canvas!.AddChild(label);

        return label;
    }

    private static void ConfigureAnchor(
        UiWidget widget,
        UiAnchor anchor,
        Vector2 offset)
    {
        widget.LayoutMode = UiLayoutMode.Anchor;
        widget.Anchor = anchor;
        widget.Offset = offset;
    }
}
