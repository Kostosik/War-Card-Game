namespace GameProject.Combat.Battlefield;

public readonly record struct BattleDirectionConnection
{
    public BattleDirection First { get; }
    public BattleDirection Second { get; }

    public BattleDirectionConnection(
        BattleDirection first,
        BattleDirection second)
    {
        if (!Enum.IsDefined(first))
            throw new ArgumentOutOfRangeException(nameof(first));

        if (!Enum.IsDefined(second))
            throw new ArgumentOutOfRangeException(nameof(second));

        if (first == second)
            throw new ArgumentException(
                "A direction cannot be connected to itself.");

        if ((int)first < (int)second)
        {
            First = first;
            Second = second;
        }
        else
        {
            First = second;
            Second = first;
        }
    }
}