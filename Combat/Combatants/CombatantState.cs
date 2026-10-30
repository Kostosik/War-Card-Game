namespace GameProject.Combat.Combatants;

public sealed class CombatantState
{
    public int MaxHealth { get; }

    public int Health { get; private set; }

    public bool IsDefeated => Health == 0;

    public CombatantState(int maxHealth)
    {
        if (maxHealth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxHealth));
        }

        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public int ApplyDamage(int damage)
    {
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage));
        }

        if (IsDefeated || damage == 0)
        {
            return 0;
        }

        var appliedDamage = Math.Min(damage, Health);

        Health -= appliedDamage;

        return appliedDamage;
    }
}