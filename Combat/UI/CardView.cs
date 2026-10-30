using Engine.Core.Math;
using Engine.Graphics.Resources;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Input;
using Engine.UI.Layout;
using GameProject.Combat.Cards;

namespace GameProject.Combat.UI;

internal sealed class CardView : UiPanel, IUiDragSource
{
    private readonly UiAssetInstance _instance;

    private readonly UiImage _artwork;
    private readonly UiLabel _title;
    private readonly UiLabel _cost;
    private readonly UiLabel _description;
    private UiCanvas? _dragCanvas;
    private Vector2 _originalPosition;
    private Vector2 _pointerOffset;
    private CardDefinition _definition;

    public CardView(
        UiAsset asset,
        UiAssetInstance instance,
        CardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(definition);

        _instance = instance;
        _definition = definition;

        Width = asset.CanvasSize.X;
        Height = asset.CanvasSize.Y;

        _artwork = FindElement<UiImage>(
            asset,
            instance,
            "Artwork");

        _title = FindElement<UiLabel>(
            asset,
            instance,
            "Title");

        _cost = FindElement<UiLabel>(
            asset,
            instance,
            "Cost");

        _description = FindElement<UiLabel>(
            asset,
            instance,
            "Description");

        instance.Root.IsHitTestVisible = false;

        AddChild(instance.Root);

        Refresh();
    }

    public CardDefinition Definition => _definition;

    public void OnDragStarted(UiDragContext context)
    {
        _dragCanvas = Parent as UiCanvas
            ?? throw new InvalidOperationException(
                "CardView must be placed inside a UiCanvas.");

        _originalPosition =
            _dragCanvas.GetPosition(this);

        _pointerOffset = new Vector2(
            context.StartPosition.X - Bounds.X,
            context.StartPosition.Y - Bounds.Y);
    }

    public void OnDragMoved(UiDragContext context)
    {
        if (_dragCanvas is null)
        {
            return;
        }

        _dragCanvas.SetPosition(
            this,
            new Vector2(
                context.Position.X - _pointerOffset.X,
                context.Position.Y - _pointerOffset.Y));
    }

    public void OnDragEnded(
        UiDragContext context,
        UiDragEndReason reason)
    {
        if (_dragCanvas is not null &&
            reason != UiDragEndReason.Dropped)
        {
            _dragCanvas.SetPosition(
                this,
                _originalPosition);
        }

        _dragCanvas = null;
    }

    public void SetArtwork(TextureHandle texture)
    {
        _artwork.Texture = texture;
    }

    public void SetDefinition(CardDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        _definition = definition;

        Refresh();
    }

    public UiDragPayload? CreateDragPayload(
        Vector2 startPosition)
    {
        return new UiDragPayload<CardDefinition>(
            _definition);
    }

    private void Refresh()
    {
        _title.Text =
            _definition.Kind switch
            {
                CardKind.Attack => "ATTACK",
                CardKind.Defense => "DEFENSE",
                _ => _definition.Kind.ToString()
            };

        _cost.Text =
            $"AP {_definition.ActionCost}";

        _description.Text =
            _definition.Kind switch
            {
                CardKind.Attack =>
                    $"Deal {_definition.Value} damage",

                CardKind.Defense =>
                    $"Gain {_definition.Value} block",

                _ => string.Empty
            };
    }

    private static T FindElement<T>(
        UiAsset asset,
        UiAssetInstance instance,
        string name)
        where T : UiWidget
    {
        var element = asset.Elements.SingleOrDefault(
            element => element.Name == name)
            ?? throw new InvalidDataException(
                $"Card UI element '{name}' was not found.");

        return instance.Get(element.Id) as T
            ?? throw new InvalidDataException(
                $"Card UI element '{name}' has an unexpected type.");
    }
}