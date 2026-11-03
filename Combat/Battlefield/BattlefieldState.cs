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
        BattleObjectiveDefinition? objective = null)
    {
        ArgumentNullException.ThrowIfNull(leftFlank);
        ArgumentNullException.ThrowIfNull(center);
        ArgumentNullException.ThrowIfNull(rightFlank);

        LeftFlank = leftFlank;
        Center = center;
        RightFlank = rightFlank;

        _objective = objective ??
            BattleObjectiveDefinition.ControlTerritory;
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
        _objective.Type switch
        {
            BattleObjectiveType.ControlTerritory =>
                GetTerritoryResult(),

            BattleObjectiveType.Breakthrough =>
                GetBreakthroughResult(),

            BattleObjectiveType.DefendPosition =>
                GetDefendPositionResult(),

            BattleObjectiveType.ContainEnemy =>
                GetContainEnemyResult(),

            _ => throw new ArgumentOutOfRangeException()
        };

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

    private BattlefieldResult GetTerritoryResult()
    {
        if (PlayerControlledDirections >= 2)
            return BattlefieldResult.PlayerVictory;

        if (EnemyControlledDirections >= 2)
            return BattlefieldResult.PlayerDefeat;

        if (AreAllDirectionsFinished)
            return BattlefieldResult.PlayerDefeat;

        return BattlefieldResult.InProgress;
    }

    private BattlefieldResult GetBreakthroughResult()
    {
        var control = GetControl(GetTargetDirection());

        return control switch
        {
            DirectionControl.Player =>
                BattlefieldResult.PlayerVictory,

            DirectionControl.Enemy =>
                BattlefieldResult.PlayerDefeat,

            _ =>
                BattlefieldResult.InProgress
        };
    }

    private BattlefieldResult GetDefendPositionResult()
    {
        var control = GetControl(GetTargetDirection());

        if (control == DirectionControl.Enemy)
            return BattlefieldResult.PlayerDefeat;

        if (StepNumber >= _objective.RequiredTurns)
            return BattlefieldResult.PlayerVictory;

        return BattlefieldResult.InProgress;
    }

    private BattlefieldResult GetContainEnemyResult()
    {
        if (EnemyControlledDirections >= 2)
            return BattlefieldResult.PlayerDefeat;

        if (StepNumber >= _objective.RequiredTurns)
            return BattlefieldResult.PlayerVictory;

        return BattlefieldResult.InProgress;
    }

    private const int SupportDamagePerDirection = 1;

    public int GetSupportDamageBonus(
        BattleDirection direction,
        DirectionControl side)
    {
        if (side == DirectionControl.None)
            return 0;

        if (side is not (DirectionControl.Player or DirectionControl.Enemy))
            throw new ArgumentOutOfRangeException(nameof(side));

        var adjacentCount = direction switch
        {
            BattleDirection.LeftFlank =>
                GetControl(BattleDirection.Center) == side ? 1 : 0,

            BattleDirection.Center =>
                (GetControl(BattleDirection.LeftFlank) == side ? 1 : 0) +
                (GetControl(BattleDirection.RightFlank) == side ? 1 : 0),

            BattleDirection.RightFlank =>
                GetControl(BattleDirection.Center) == side ? 1 : 0,

            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        return adjacentCount * SupportDamagePerDirection;
    }

    private BattleDirection GetTargetDirection()
    {
        return _objective.TargetDirection
            ?? throw new InvalidOperationException(
                "This objective requires a target direction.");
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