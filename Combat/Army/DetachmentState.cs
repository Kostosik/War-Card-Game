namespace GameProject.Combat.Army;

public sealed class DetachmentState
{
    public DetachmentDefinition Definition { get; }

    public int MaxHealth => Definition.MaxHealth;

    public int Health { get; private set; }

    public int Attack => Definition.Attack;

    public bool IsCombatReady => Health > 0;

    public DetachmentState(DetachmentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Definition = definition;
        Health = definition.MaxHealth;
    }

    public int ApplyDamage(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);

        if (!IsCombatReady || damage == 0)
        {
            return 0;
        }

        var appliedDamage = Math.Min(damage, Health);

        Health -= appliedDamage;

        return appliedDamage;
    }
}