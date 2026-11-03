namespace GameProject.Combat.Army;

public sealed class DetachmentDefinition
{
    public string Id { get; }
    public string Name { get; }
    public int MaxHealth { get; }
    public int Attack { get; }
    public DetachmentType Type { get; }

    public DetachmentDefinition(
        string id,
        string name,
        int maxHealth,
        int attack,
        DetachmentType type = DetachmentType.Infantry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHealth);
        ArgumentOutOfRangeException.ThrowIfNegative(attack);

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        Id = id;
        Name = name;
        MaxHealth = maxHealth;
        Attack = attack;
        Type = type;
    }
}