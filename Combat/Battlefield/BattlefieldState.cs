namespace GameProject.Combat.Battlefield;

public enum BattleDirection
{
    LeftFlank,
    Center,
    RightFlank
}

public enum DirectionControl
{
    None,
    Player,
    Enemy
}

public enum BattlefieldResult
{
    InProgress,
    PlayerVictory,
    PlayerDefeat
}

public sealed class BattlefieldState
{
    public int StepNumber { get; private set; }
    public BattleDirectionState LeftFlank { get; }

    public BattleDirectionState Center { get; }

    public BattleDirectionState RightFlank { get; }

    public BattlefieldState(
        BattleDirectionState leftFlank,
        BattleDirectionState center,
        BattleDirectionState rightFlank)
    {
        ArgumentNullException.ThrowIfNull(leftFlank);
        ArgumentNullException.ThrowIfNull(center);
        ArgumentNullException.ThrowIfNull(rightFlank);

        LeftFlank = leftFlank;
        Center = center;
        RightFlank = rightFlank;
    }

    public int PlayerControlledDirections =>
        CountControlledDirections(DirectionControl.Player);

    public int EnemyControlledDirections =>
        CountControlledDirections(DirectionControl.Enemy);
    public bool AreAllDirectionsFinished =>
    LeftFlank.IsFinished &&
    Center.IsFinished &&
    RightFlank.IsFinished;
    public BattlefieldResult Result
    {
        get
        {
            if (PlayerControlledDirections >= 2)
                return BattlefieldResult.PlayerVictory;

            if (EnemyControlledDirections >= 2)
                return BattlefieldResult.PlayerDefeat;

            // Все направления завершены, но игрок
            // не смог захватить два направления.
            // Победа остаётся за обороняющейся стороной.
            if (AreAllDirectionsFinished)
                return BattlefieldResult.PlayerDefeat;

            return BattlefieldResult.InProgress;
        }
    }

    public bool IsFinished =>
        Result != BattlefieldResult.InProgress;

    public void ResolveStep()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Battlefield has already finished.");
        }

        ResolveDirection(LeftFlank);
        ResolveDirection(Center);
        ResolveDirection(RightFlank);
        StepNumber++;
    }

    public void QueueArtilleryStrike(
    BattleDirection direction,
    int damage)
    {
        GetActiveDirection(direction).QueueArtilleryStrike(damage);
    }

    public void QueueSmokeScreen(
        BattleDirection direction,
        int reductionPercent)
    {
        GetActiveDirection(direction).QueueSmokeScreen(reductionPercent);
    }

    public void QueueSupply(
        BattleDirection direction,
        int healing)
    {
        GetActiveDirection(direction).QueueSupply(healing);
    }

    public DirectionControl GetControl(BattleDirection direction)
    {
        return GetDirectionState(direction).Control;
    }

    private BattleDirectionState GetActiveDirection(
    BattleDirection direction)
    {
        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Battlefield has already finished.");
        }

        var state = GetDirectionState(direction);

        if (state.IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot apply card effects to a finished direction.");
        }

        return state;
    }

    private BattleDirectionState GetDirectionState(
        BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.LeftFlank => LeftFlank,
            BattleDirection.Center => Center,
            BattleDirection.RightFlank => RightFlank,

            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    private void ResolveDirection(BattleDirectionState direction)
    {
        if (!direction.IsFinished)
        {
            direction.ResolveStep();
        }
    }

    private int CountControlledDirections(DirectionControl control)
    {
        var count = 0;

        if (GetControl(BattleDirection.LeftFlank) == control)
            count++;

        if (GetControl(BattleDirection.Center) == control)
            count++;

        if (GetControl(BattleDirection.RightFlank) == control)
            count++;

        return count;
    }
}