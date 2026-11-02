namespace GameProject.Combat.Army;

public sealed class DetachmentDefinition
{
    public string Id { get; }

    public string Name { get; }

    public int MaxHealth { get; }

    public int Attack { get; }

    public DetachmentDefinition(
        string id,
        string name,
        int maxHealth,
        int attack)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHealth);
        ArgumentOutOfRangeException.ThrowIfNegative(attack);

        Id = id;
        Name = name;
        MaxHealth = maxHealth;
        Attack = attack;
    }
}