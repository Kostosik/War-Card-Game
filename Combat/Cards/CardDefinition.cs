namespace GameProject.Combat.Cards;

public sealed record CardDefinition
{
    public string Id { get; }

    public CardKind Kind { get; }

    public int ActionCost { get; }

    public int Value { get; }

    public CardDefinition(
        string id,
        CardKind kind,
        int actionCost,
        int value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (actionCost < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(actionCost));
        }

        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value));
        }

        Id = id;
        Kind = kind;
        ActionCost = actionCost;
        Value = value;
    }
}