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
    public BattleFormationDefinition Formation { get; }
    private readonly BattleObjectiveDefinition _objective;

    public int StepNumber { get; private set; }

    public BattleObjectiveDefinition Objective => _objective;

    public BattleDirectionState LeftFlank { get; }

    public BattleDirectionState Center { get; }

    public BattleDirectionState RightFlank { get; }

    public BattlefieldState(
        BattleDirectionState leftFlank,
        BattleDirectionState center,
        BattleDirectionState rightFlank,
        BattleObjectiveDefinition? objective = null,
        BattleFormationDefinition formation = null)
    {
        ArgumentNullException.ThrowIfNull(leftFlank);
        ArgumentNullException.ThrowIfNull(center);
        ArgumentNullException.ThrowIfNull(rightFlank);

        LeftFlank = leftFlank;
        Center = center;
        RightFlank = rightFlank;

        _objective = objective ??
            BattleObjectiveDefinition.ControlTerritory;
        Formation = formation ?? BattleFormationCatalog.LinearFront;
    }

    public int PlayerControlledDirections =>
        CountControlledDirections(DirectionControl.Player);

    public int EnemyControlledDirections =>
        CountControlledDirections(DirectionControl.Enemy);

    public bool AreAllDirectionsFinished =>
        LeftFlank.IsFinished &&
        Center.IsFinished &&
        RightFlank.IsFinished;

    public BattlefieldResult Result =>
        _objective.Rule.Evaluate(CreateSnapshot());

    public string ObjectiveStatus =>
        _objective.Rule.BuildStatus(CreateSnapshot());

    public bool IsFinished =>
        Result != BattlefieldResult.InProgress;

    public void ResolveStep()
    {
        if (IsFinished)
            throw new InvalidOperationException(
                "Battlefield has already finished.");

        // Сначала рассчитываем поддержку всех направлений.
        // Состояние поля пока не изменяется.
        var leftPlayerSupport =
            GetSupportDamageBonus(BattleDirection.LeftFlank, DirectionControl.Player);
        var leftEnemySupport =
            GetSupportDamageBonus(BattleDirection.LeftFlank, DirectionControl.Enemy);

        var centerPlayerSupport =
            GetSupportDamageBonus(BattleDirection.Center, DirectionControl.Player);
        var centerEnemySupport =
            GetSupportDamageBonus(BattleDirection.Center, DirectionControl.Enemy);

        var rightPlayerSupport =
            GetSupportDamageBonus(BattleDirection.RightFlank, DirectionControl.Player);
        var rightEnemySupport =
            GetSupportDamageBonus(BattleDirection.RightFlank, DirectionControl.Enemy);

        // Только после расчёта всех бонусов разрешаем бой.
        ResolveDirection(
            LeftFlank,
            leftPlayerSupport,
            leftEnemySupport);

        ResolveDirection(
            Center,
            centerPlayerSupport,
            centerEnemySupport);

        ResolveDirection(
            RightFlank,
            rightPlayerSupport,
            rightEnemySupport);

        StepNumber++;
    }

    private BattlefieldSnapshot CreateSnapshot()
    {
        return new BattlefieldSnapshot(
            StepNumber,
            LeftFlank.Control,
            Center.Control,
            RightFlank.Control,
            AreAllDirectionsFinished);
    }

    private void ResolveDirection(
    BattleDirectionState direction,
    int playerSupport,
    int enemySupport)
    {
        if (!direction.IsFinished)
            direction.ResolveStep(playerSupport, enemySupport);
    }

    public void QueueArtilleryStrike(
        BattleDirection direction,
        int damage)
    {
        GetActiveDirection(direction)
            .QueueArtilleryStrike(damage);
    }

    public void QueueSmokeScreen(
        BattleDirection direction,
        int reductionPercent)
    {
        GetActiveDirection(direction)
            .QueueSmokeScreen(reductionPercent);
    }

    public void QueueSupply(
        BattleDirection direction,
        int healing)
    {
        GetActiveDirection(direction)
            .QueueSupply(healing);
    }

    public DirectionControl GetControl(
        BattleDirection direction)
    {
        return GetDirectionState(direction).Control;
    }

    private const int SupportDamagePerDirection = 1;

    public int GetSupportDamageBonus(
    BattleDirection direction,
    DirectionControl side)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction));

        if (side == DirectionControl.None)
            return 0;

        if (!Enum.IsDefined(side))
            throw new ArgumentOutOfRangeException(nameof(side));

        var supportedNeighbors = Formation
            .GetNeighbors(direction)
            .Count(neighbor => GetControl(neighbor) == side);

        return supportedNeighbors *
               Formation.SupportDamagePerNeighbor;
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

            _ => throw new ArgumentOutOfRangeException(
                nameof(direction))
        };
    }

    private void ResolveDirection(
        BattleDirectionState direction)
    {
        if (!direction.IsFinished)
        {
            direction.ResolveStep();
        }
    }

    private int CountControlledDirections(
        DirectionControl control)
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