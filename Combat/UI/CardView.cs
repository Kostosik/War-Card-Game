using Engine.Core.Math;
using Engine.Graphics.Commands;
using Engine.Graphics.Resources;
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

    public CardView(
    CardDefinition definition,
    TextureResourceManager textureResources)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(textureResources);

        Definition = definition;

        Width = CardWidth;
        Height = CardHeight;

        Padding = new UiThickness(8);

        Background = new UiColor(
            definition.Appearance.Red,
            definition.Appearance.Green,
            definition.Appearance.Blue);

        _contentCanvas = new UiCanvas
        {
            ClipChildren = true
        };

        AddChild(_contentCanvas);

        if (definition.TexturePath is { } texturePath)
        {
            var texture = textureResources.Load(texturePath);

            if (!textureResources.TryGetDescription(
                    texturePath,
                    out var description))
            {
                throw new InvalidOperationException(
                    $"Could not get texture description: {texturePath}");
            }

            var image = new UiImage(
                texture,
                new Vector2(description.Width, description.Height))
            {
                Width = 126,
                Height = 58,
                ZIndex = -1
            };

            _contentCanvas.AddChild(image);

            _contentCanvas.SetPosition(
                image,
                Vector2.Zero);

            AddLabel(
                definition.Title,
                0,
                62,
                126,
                28,
                13);

            AddLabel(
                $"{definition.ActionCost} AP",
                0,
                92,
                126,
                20,
                12);

            AddLabel(
                definition.Description,
                0,
                116,
                126,
                46,
                11);
        }
        else
        {
            AddLabel(
                definition.Title,
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
                definition.Description,
                0,
                76,
                126,
                80,
                13);
        }
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
}