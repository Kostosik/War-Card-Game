namespace GameProject.Combat.Battlefield;

public sealed class ControlTerritoryObjectiveRule : IBattleObjectiveRule
{
    public BattlefieldResult Evaluate(BattlefieldSnapshot battlefield)
    {
        if (battlefield.PlayerControlledDirections >= 2)
            return BattlefieldResult.PlayerVictory;

        if (battlefield.EnemyControlledDirections >= 2)
            return BattlefieldResult.PlayerDefeat;

        if (battlefield.AreAllDirectionsFinished)
            return BattlefieldResult.PlayerDefeat;

        return BattlefieldResult.InProgress;
    }

    public string BuildStatus(BattlefieldSnapshot battlefield)
    {
        return
            $"ЦЕЛЬ: ЗАХВАТИТЬ 2 ИЗ 3 НАПРАВЛЕНИЙ | " +
            $"НАШ КОНТРОЛЬ: {battlefield.PlayerControlledDirections}/2 | " +
            $"ВРАЖЕСКИЙ: {battlefield.EnemyControlledDirections}/2";
    }
}

public sealed class BreakthroughObjectiveRule : IBattleObjectiveRule
{
    public BattleDirection TargetDirection { get; }

    public BreakthroughObjectiveRule(BattleDirection targetDirection)
    {
        ValidateDirection(targetDirection);
        TargetDirection = targetDirection;
    }

    public BattlefieldResult Evaluate(BattlefieldSnapshot battlefield)
    {
        return battlefield.GetControl(TargetDirection) switch
        {
            DirectionControl.Player => BattlefieldResult.PlayerVictory,
            DirectionControl.Enemy => BattlefieldResult.PlayerDefeat,
            _ => BattlefieldResult.InProgress
        };
    }

    public string BuildStatus(BattlefieldSnapshot battlefield)
    {
        var control = battlefield.GetControl(TargetDirection);

        return
            $"ЦЕЛЬ: ПРОРЫВ — ЗАХВАТИТЬ {GetDirectionName(TargetDirection)} | " +
            $"СОСТОЯНИЕ: {GetControlName(control)}";
    }

    internal static void ValidateDirection(BattleDirection direction)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction));
    }

    internal static string GetDirectionName(BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.LeftFlank => "ЛЕВЫЙ ФЛАНГ",
            BattleDirection.Center => "ЦЕНТР",
            BattleDirection.RightFlank => "ПРАВЫЙ ФЛАНГ",
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    internal static string GetControlName(DirectionControl control)
    {
        return control switch
        {
            DirectionControl.None => "БОЙ ПРОДОЛЖАЕТСЯ",
            DirectionControl.Player => "НАШ КОНТРОЛЬ",
            DirectionControl.Enemy => "НАПРАВЛЕНИЕ ПОТЕРЯНО",
            _ => throw new ArgumentOutOfRangeException(nameof(control))
        };
    }
}

public sealed class DefendPositionObjectiveRule : IBattleObjectiveRule
{
    public BattleDirection TargetDirection { get; }
    public int RequiredTurns { get; }

    public DefendPositionObjectiveRule(
        BattleDirection targetDirection,
        int requiredTurns)
    {
        BreakthroughObjectiveRule.ValidateDirection(targetDirection);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requiredTurns);

        TargetDirection = targetDirection;
        RequiredTurns = requiredTurns;
    }

    public BattlefieldResult Evaluate(BattlefieldSnapshot battlefield)
    {
        var control = battlefield.GetControl(TargetDirection);

        if (control == DirectionControl.Enemy)
            return BattlefieldResult.PlayerDefeat;

        if (battlefield.StepNumber >= RequiredTurns)
            return BattlefieldResult.PlayerVictory;

        return BattlefieldResult.InProgress;
    }

    public string BuildStatus(BattlefieldSnapshot battlefield)
    {
        var progress = Math.Min(
            battlefield.StepNumber,
            RequiredTurns);

        var control = battlefield.GetControl(TargetDirection);

        var positionStatus = control == DirectionControl.Enemy
            ? "ПОЗИЦИЯ ПОТЕРЯНА"
            : "ПОЗИЦИЯ УДЕРЖИВАЕТСЯ";

        return
            $"ЦЕЛЬ: ОБОРОНА — " +
            $"{BreakthroughObjectiveRule.GetDirectionName(TargetDirection)} | " +
            $"ХОДЫ: {progress}/{RequiredTurns} | " +
            positionStatus;
    }
}

public sealed class ContainEnemyObjectiveRule : IBattleObjectiveRule
{
    public int RequiredTurns { get; }

    public ContainEnemyObjectiveRule(int requiredTurns)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requiredTurns);
        RequiredTurns = requiredTurns;
    }

    public BattlefieldResult Evaluate(BattlefieldSnapshot battlefield)
    {
        if (battlefield.EnemyControlledDirections >= 2)
            return BattlefieldResult.PlayerDefeat;

        if (battlefield.StepNumber >= RequiredTurns)
            return BattlefieldResult.PlayerVictory;

        return BattlefieldResult.InProgress;
    }

    public string BuildStatus(BattlefieldSnapshot battlefield)
    {
        var progress = Math.Min(
            battlefield.StepNumber,
            RequiredTurns);

        return
            $"ЦЕЛЬ: СДЕРЖАТЬ ПРОТИВНИКА | " +
            $"ХОДЫ: {progress}/{RequiredTurns} | " +
            $"ВРАЖЕСКИЙ КОНТРОЛЬ: " +
            $"{battlefield.EnemyControlledDirections}/2";
    }
}