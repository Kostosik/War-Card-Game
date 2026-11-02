namespace GameProject.Combat.Army;

public sealed class DetachmentOrderDefinition
{
    public string Id { get; }
    public string Name { get; }

    public int AttackBonusPercent { get; }
    public int DamageReductionPercent { get; }
    public int EndOfRoundHealing { get; }

    public DetachmentOrderDefinition(
        string id,
        string name,
        int attackBonusPercent,
        int damageReductionPercent,
        int endOfRoundHealing)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ArgumentOutOfRangeException.ThrowIfNegative(attackBonusPercent);
        ArgumentOutOfRangeException.ThrowIfNegative(damageReductionPercent);
        ArgumentOutOfRangeException.ThrowIfNegative(endOfRoundHealing);

        if (attackBonusPercent > 100 || damageReductionPercent > 100)
            throw new ArgumentOutOfRangeException(
                nameof(attackBonusPercent));

        Id = id;
        Name = name;
        AttackBonusPercent = attackBonusPercent;
        DamageReductionPercent = damageReductionPercent;
        EndOfRoundHealing = endOfRoundHealing;
    }

    public int ModifyOutgoingDamage(int damage)
    {
        return Scale(damage, 100 + AttackBonusPercent);
    }

    public int ModifyIncomingDamage(int damage)
    {
        return Scale(damage, 100 - DamageReductionPercent);
    }

    private static int Scale(int value, int percent)
    {
        return (int)(((long)value * percent + 50) / 100);
    }
}