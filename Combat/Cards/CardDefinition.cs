using Engine.Core.Assets;
using GameProject.Combat.Battlefield;

namespace GameProject.Combat.Cards;

public delegate void CardEffect(
    BattlefieldState battlefield,
    BattleDirection direction,
    int value);

public readonly record struct CardAppearance(
    byte Red,
    byte Green,
    byte Blue);

public sealed record CardDefinition
{
    public string Id { get; }
    public string Title { get; }
    public string Description { get; }

    public int ActionCost { get; }
    public int Value { get; }
    public int CopiesInDeck { get; }

    public CardAppearance Appearance { get; }

    public AssetPath? TexturePath { get; }

    public CardEffect? Effect { get; }

    public CardDefinition(
        string id,
        string title,
        string description,
        int actionCost,
        int value,
        int copiesInDeck,
        CardAppearance appearance,
        CardEffect effect,
        AssetPath? texturePath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(effect);

        if (actionCost < 0)
            throw new ArgumentOutOfRangeException(nameof(actionCost));

        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(copiesInDeck);

        Id = id;
        Title = title;
        Description = description;
        ActionCost = actionCost;
        Value = value;
        CopiesInDeck = copiesInDeck;
        Appearance = appearance;
        Effect = effect;
        TexturePath = texturePath;
    }

    // Совместимость со старым конструктором.
    public CardDefinition(
        string id,
        CardKind kind,
        int actionCost,
        int value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (actionCost < 0)
            throw new ArgumentOutOfRangeException(nameof(actionCost));

        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Id = id;
        Title = id;
        Description = string.Empty;
        ActionCost = actionCost;
        Value = value;
        CopiesInDeck = 1;
        Appearance = new CardAppearance(55, 55, 65);
        Effect = null;
        TexturePath = null;
    }
}