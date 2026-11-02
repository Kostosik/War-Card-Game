using Engine.Core.Math;
using Engine.Graphics.Commands;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Input;
using Engine.UI.Layout;
using GameProject.Combat.Cards;

namespace GameProject.Combat.UI;

internal sealed class CardView : UiPanel, IUiDragSource
{
    private const float CardWidth = 142;
    private const float CardHeight = 180;

    private readonly UiCanvas _contentCanvas;
    private bool _wasHitTestVisible;
    private UiCanvas? _dragCanvas;

    private Vector2 _originalPosition;
    private Vector2 _pointerOffset;

    public CardDefinition Definition { get; }

    public CardView(CardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Definition = definition;

        Width = CardWidth;
        Height = CardHeight;

        Padding = new UiThickness(8);
        Background = GetBackground(definition.Kind);

        _contentCanvas = new UiCanvas
        {
            ClipChildren = true
        };

        AddChild(_contentCanvas);

        AddLabel(
            GetTitle(definition.Kind),
            0,
            5,
            126,
            32,
            15);

        AddLabel(
            $"{definition.ActionCost} AP",
            0,
            42,
            126,
            24,
            13);

        AddLabel(
            GetDescription(definition),
            0,
            76,
            126,
            80,
            13);
    }

    public UiDragPayload? CreateDragPayload(
        Vector2 startPosition)
    {
        return new UiDragPayload<CardDefinition>(
            Definition);
    }

    public void OnDragStarted(UiDragContext context)
    {
        _dragCanvas = Parent as UiCanvas
            ?? throw new InvalidOperationException(
                "CardView must be placed inside a UiCanvas.");

        _originalPosition = _dragCanvas.GetPosition(this);

        _pointerOffset = new Vector2(
            context.StartPosition.X - Bounds.X,
            context.StartPosition.Y - Bounds.Y);

        _wasHitTestVisible = IsHitTestVisible;
        IsHitTestVisible = false;
    }

    public void OnDragMoved(UiDragContext context)
    {
        if (_dragCanvas is null)
            return;

        _dragCanvas.SetPosition(
            this,
            new Vector2(
                context.Position.X -
                _pointerOffset.X -
                _dragCanvas.Bounds.X,

                context.Position.Y -
                _pointerOffset.Y -
                _dragCanvas.Bounds.Y));
    }

    public void OnDragEnded(
        UiDragContext context,
        UiDragEndReason reason)
    {
        IsHitTestVisible = _wasHitTestVisible;

        if (_dragCanvas is not null &&
            reason != UiDragEndReason.Dropped &&
            ReferenceEquals(Parent, _dragCanvas))
        {
            _dragCanvas.SetPosition(
                this,
                _originalPosition);
        }

        _dragCanvas = null;
    }

    private void AddLabel(
        string text,
        float x,
        float y,
        float width,
        float height,
        float fontSize)
    {
        var label = new UiLabel(text)
        {
            Width = width,
            Height = height,
            FontSize = fontSize,
            Color = UiColor.White
        };

        _contentCanvas.AddChild(label);

        _contentCanvas.SetPosition(
            label,
            new Vector2(x, y));
    }

    private static string GetTitle(CardKind kind)
    {
        return kind switch
        {
            CardKind.ArtilleryStrike => "АРТИЛЛЕРИЯ",
            CardKind.SmokeScreen => "ДЫМОВАЯ ЗАВЕСА",
            CardKind.Supply => "СНАБЖЕНИЕ",
            CardKind.Attack => "АТАКА",
            CardKind.Defense => "ЗАЩИТА",
            _ => kind.ToString()
        };
    }

    private static string GetDescription(
        CardDefinition definition)
    {
        return definition.Kind switch
        {
            CardKind.ArtilleryStrike =>
                $"Наносит {definition.Value} дополнительного урона",

            CardKind.SmokeScreen =>
                $"Снижает урон противника на {definition.Value}%",

            CardKind.Supply =>
                $"Восстанавливает {definition.Value} здоровья",

            CardKind.Attack =>
                $"Наносит {definition.Value} урона",

            CardKind.Defense =>
                $"Защита: {definition.Value}",

            _ => string.Empty
        };
    }

    private static UiColor GetBackground(CardKind kind)
    {
        return kind switch
        {
            CardKind.ArtilleryStrike =>
                new UiColor(110, 55, 45),

            CardKind.SmokeScreen =>
                new UiColor(55, 75, 95),

            CardKind.Supply =>
                new UiColor(55, 95, 65),

            _ =>
                new UiColor(55, 55, 65)
        };
    }
}