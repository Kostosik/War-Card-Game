namespace GameProject.Combat.Army;

public sealed class DetachmentState
{
    public DetachmentDefinition Definition { get; }

    public int MaxHealth => Definition.MaxHealth;

    public int Health { get; private set; }

    public int Attack => Definition.Attack;

    public DetachmentOrderDefinition? Order { get; private set; }

    public bool IsCombatReady => Health > 0;

    public DetachmentState(DetachmentDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Definition = definition;
        Health = definition.MaxHealth;
    }

    public void AssignOrder(DetachmentOrderDefinition order)
    {
        ArgumentNullException.ThrowIfNull(order);

        Order = order;
    }

    public int ApplyDamage(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);

        if (!IsCombatReady || damage == 0)
            return 0;

        var modifiedDamage =
            Order?.ModifyIncomingDamage(damage) ?? damage;

        var appliedDamage = Math.Min(modifiedDamage, Health);

        Health -= appliedDamage;

        return appliedDamage;
    }

    public int RestoreHealth(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (!IsCombatReady || amount == 0)
            return 0;

        var restored = Math.Min(amount, MaxHealth - Health);

        Health += restored;

        return restored;
    }

    public int ApplyEndOfRoundEffects()
    {
        if (!IsCombatReady)
            return 0;

        var healing = Order?.EndOfRoundHealing ?? 0;
        var actualHealing = Math.Min(healing, MaxHealth - Health);

        Health += actualHealing;

        return actualHealing;
    }
}