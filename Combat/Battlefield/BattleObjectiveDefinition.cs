namespace GameProject.Combat.Battlefield;

public enum BattleObjectiveType
{
    ControlTerritory,
    Breakthrough,
    DefendPosition,
    ContainEnemy
}

public sealed class BattleObjectiveDefinition
{
    public BattleObjectiveType Type { get; }

    public BattleDirection? TargetDirection { get; }

    public int RequiredTurns { get; }

    public static BattleObjectiveDefinition ControlTerritory { get; } =
        new(BattleObjectiveType.ControlTerritory, null, 0);

    public static BattleObjectiveDefinition Breakthrough(
        BattleDirection direction)
    {
        return new BattleObjectiveDefinition(
            BattleObjectiveType.Breakthrough,
            direction,
            0);
    }

    public static BattleObjectiveDefinition DefendPosition(
        BattleDirection direction,
        int requiredTurns)
    {
        return new BattleObjectiveDefinition(
            BattleObjectiveType.DefendPosition,
            direction,
            requiredTurns);
    }

    public static BattleObjectiveDefinition ContainEnemy(
        int requiredTurns)
    {
        return new BattleObjectiveDefinition(
            BattleObjectiveType.ContainEnemy,
            null,
            requiredTurns);
    }

    private BattleObjectiveDefinition(
        BattleObjectiveType type,
        BattleDirection? targetDirection,
        int requiredTurns)
    {
        switch (type)
        {
            case BattleObjectiveType.ControlTerritory:
                if (targetDirection.HasValue || requiredTurns != 0)
                    throw new ArgumentException(
                        "Territory control does not require additional parameters.");
                break;

            case BattleObjectiveType.Breakthrough:
                ValidateDirection(targetDirection);
                if (requiredTurns != 0)
                    throw new ArgumentException(
                        "Breakthrough does not require a turn limit.");
                break;

            case BattleObjectiveType.DefendPosition:
                ValidateDirection(targetDirection);
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
                    requiredTurns);
                break;

            case BattleObjectiveType.ContainEnemy:
                if (targetDirection.HasValue)
                    throw new ArgumentException(
                        "Containment does not require a target direction.");

                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
                    requiredTurns);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }

        Type = type;
        TargetDirection = targetDirection;
        RequiredTurns = requiredTurns;
    }

    private static void ValidateDirection(
        BattleDirection? direction)
    {
        if (direction is not (
            BattleDirection.LeftFlank or
            BattleDirection.Center or
            BattleDirection.RightFlank))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }
    }
}