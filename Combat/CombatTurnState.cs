namespace GameProject.Combat;

public sealed class CombatTurnState
{
    public int TurnNumber { get; private set; }

    public int MaxActionPoints { get; }

    public int AvailableActionPoints { get; private set; }

    public int Block { get; private set; }

    public CombatTurnState(int maxActionPoints)
    {
        if (maxActionPoints <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxActionPoints));
        }

        MaxActionPoints = maxActionPoints;
    }

    public void BeginTurn()
    {
        TurnNumber = checked(TurnNumber + 1);

        AvailableActionPoints = MaxActionPoints;

        Block = 0;
    }

    public bool TrySpendActionPoints(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount));
        }

        if (amount > AvailableActionPoints)
        {
            return false;
        }

        AvailableActionPoints -= amount;

        return true;
    }

    public void AddBlock(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount));
        }

        Block = checked(Block + amount);
    }

    public int AbsorbDamage(int incomingDamage)
    {
        if (incomingDamage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(incomingDamage));
        }

        var absorbed = Math.Min(
            Block,
            incomingDamage);

        Block -= absorbed;

        return incomingDamage - absorbed;
    }
}