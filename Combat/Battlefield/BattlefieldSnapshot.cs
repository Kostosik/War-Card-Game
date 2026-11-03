namespace GameProject.Combat.Battlefield;

public sealed class BattlefieldSnapshot
{
    private readonly DirectionControl _left;
    private readonly DirectionControl _center;
    private readonly DirectionControl _right;

    public int StepNumber { get; }

    public bool AreAllDirectionsFinished { get; }

    public int PlayerControlledDirections { get; }

    public int EnemyControlledDirections { get; }

    public BattlefieldSnapshot(
        int stepNumber,
        DirectionControl left,
        DirectionControl center,
        DirectionControl right,
        bool areAllDirectionsFinished)
    {
        StepNumber = stepNumber;

        _left = left;
        _center = center;
        _right = right;

        AreAllDirectionsFinished = areAllDirectionsFinished;

        PlayerControlledDirections = Count(
            DirectionControl.Player);

        EnemyControlledDirections = Count(
            DirectionControl.Enemy);
    }

    public DirectionControl GetControl(BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.LeftFlank => _left,
            BattleDirection.Center => _center,
            BattleDirection.RightFlank => _right,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    private int Count(DirectionControl control)
    {
        var result = 0;

        if (_left == control) result++;
        if (_center == control) result++;
        if (_right == control) result++;

        return result;
    }
}