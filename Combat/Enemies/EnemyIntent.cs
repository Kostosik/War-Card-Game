namespace GameProject.Combat.Enemies;

public sealed record EnemyIntent
{
    public EnemyIntentKind Kind { get; }

    public int Amount { get; }

    public EnemyIntent(
        EnemyIntentKind kind,
        int amount)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        Kind = kind;
        Amount = amount;
    }
}