namespace GameProject.Combat.Army;

public sealed class DetachmentState
{
    public int PersonnelCount { get; }

    public int MaxHealth { get; }

    public int Health { get; private set; }

    public int Attack { get; }

    public int Defense { get; }

    public bool IsCombatReady => Health > 0;

    public DetachmentState(
        int personnelCount,
        int maxHealth,
        int attack,
        int defense)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            personnelCount);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            maxHealth);

        ArgumentOutOfRangeException.ThrowIfNegative(attack);
        ArgumentOutOfRangeException.ThrowIfNegative(defense);

        PersonnelCount = personnelCount;
        MaxHealth = maxHealth;
        Health = maxHealth;
        Attack = attack;
        Defense = defense;
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