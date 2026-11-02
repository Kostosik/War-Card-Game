using Engine.Graphics.Commands;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Input;
using Engine.UI.Layout;
using GameProject.Combat.Battlefield;
using GameProject.Combat.Cards;

namespace GameProject.Combat.UI;

internal sealed class BattleDirectionDropTarget :
    UiPanel,
    IUiDropTarget
{
    private readonly Func<
        BattleDirection,
        CardDefinition,
        bool> _tryPlayCard;
    private readonly UiLabel _effectsLabel;
    private readonly UiLabel _playerLabel;
    private readonly UiLabel _enemyLabel;
    private readonly UiLabel _orderLabel;
    private readonly UiLabel _hintLabel;

    private bool _dragHovered;
    private bool _finished;

    private readonly UiColor _normalColor =
        new(35, 43, 55);

    private readonly UiColor _hoverColor =
        new(65, 100, 125);

    private readonly UiColor _playerControlledColor =
        new(45, 75, 50);

    private readonly UiColor _enemyControlledColor =
        new(85, 45, 45);

    private readonly UiColor _neutralFinishedColor =
        new(65, 65, 70);

    private DirectionControl _control = DirectionControl.None;

    public BattleDirection Direction { get; }

    public BattleDirectionDropTarget(
        BattleDirection direction,
        string title,
        Func<BattleDirection, CardDefinition, bool> tryPlayCard)
    {
        ArgumentNullException.ThrowIfNull(tryPlayCard);

        Direction = direction;
        _tryPlayCard = tryPlayCard;

        Padding = new UiThickness(16);
        Background = _normalColor;

        var content = new UiStackPanel
        {
            Orientation = UiOrientation.Vertical,
            Spacing = 12
        };

        content.AddChild(new UiLabel(title)
        {
            Height = 32,
            FontSize = 21
        });

        _playerLabel = new UiLabel()
        {
            Height = 42,
            FontSize = 15
        };

        _enemyLabel = new UiLabel()
        {
            Height = 42,
            FontSize = 15
        };

        _orderLabel = new UiLabel()
        {
            Height = 30,
            FontSize = 14
        };

        _effectsLabel = new UiLabel("ЭФФЕКТЫ: НЕТ")
        {
            Height = 60,
            FontSize = 13,
            Color = new UiColor(240, 205, 135)
        };

        _hintLabel = new UiLabel("ПЕРЕТАЩИТЕ КАРТУ СЮДА")
        {
            Height = 35,
            FontSize = 13,
            Color = new UiColor(210, 220, 230)
        };

        content.AddChild(_playerLabel);
        content.AddChild(_enemyLabel);
        content.AddChild(_orderLabel);
        content.AddChild(_effectsLabel);
        content.AddChild(_hintLabel);

        AddChild(content);
    }

    public void SetState(
        string playerInfo,
        string enemyInfo,
        string orderInfo,
        string effectsInfo,
        DirectionControl control,bool finished)
    {
        _playerLabel.Text = playerInfo;
        _enemyLabel.Text = enemyInfo;
        _orderLabel.Text = orderInfo;
        _effectsLabel.Text = effectsInfo;
        _control = control;
        _finished = finished;
        Enabled = !finished;

        UpdateBackground();
    }

    public bool CanAcceptDrop(UiDragPayload payload)
    {
        return !_finished &&
               payload is UiDragPayload<CardDefinition>;
    }

    public void OnDragEnter(UiDragContext context)
    {
        _dragHovered = true;
        UpdateBackground();
    }

    public void OnDragLeave(UiDragContext context)
    {
        _dragHovered = false;
        UpdateBackground();
    }

    public bool OnDrop(UiDragContext context)
    {
        if (context.Payload is not UiDragPayload<CardDefinition> payload)
            return false;

        return _tryPlayCard(
            Direction,
            payload.Value);
    }

    private void UpdateBackground()
    {
        if (!_finished)
        {
            Background = _dragHovered
                ? _hoverColor
                : _normalColor;

            return;
        }

        Background = _control switch
        {
            DirectionControl.Player => _playerControlledColor,
            DirectionControl.Enemy => _enemyControlledColor,
            _ => _neutralFinishedColor
        };
    }
}