using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public sealed class BattleDirectionState
{
    public DetachmentState PlayerDetachment { get; }

    public DetachmentState EnemyDetachment { get; }

    public bool IsFinished =>
        !PlayerDetachment.IsCombatReady ||
        !EnemyDetachment.IsCombatReady;

    public BattleDirectionState(
        DetachmentState playerDetachment,
        DetachmentState enemyDetachment)
    {
        ArgumentNullException.ThrowIfNull(playerDetachment);
        ArgumentNullException.ThrowIfNull(enemyDetachment);

        PlayerDetachment = playerDetachment;
        EnemyDetachment = enemyDetachment;
    }

    public void ApplySimultaneousDamage(
        int damageDealtByPlayer,
        int damageDealtByEnemy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(
            damageDealtByPlayer);

        ArgumentOutOfRangeException.ThrowIfNegative(
            damageDealtByEnemy);

        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot advance a finished battle direction.");
        }

        // Обе стороны наносят урон,
        // даже если одна из них получит смертельный урон.
        EnemyDetachment.ApplyDamage(damageDealtByPlayer);
        PlayerDetachment.ApplyDamage(damageDealtByEnemy);
    }

    public DirectionControl Control
    {
        get
        {
            var playerReady = PlayerDetachment.IsCombatReady;
            var enemyReady = EnemyDetachment.IsCombatReady;

            if (playerReady && !enemyReady)
                return DirectionControl.Player;

            if (!playerReady && enemyReady)
                return DirectionControl.Enemy;

            return DirectionControl.None;
        }
    }

    public void ResolveStep()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot advance a finished battle direction.");
        }

        var playerDamage = DetachmentDamageCalculator.Calculate(
            PlayerDetachment);

        var enemyDamage = DetachmentDamageCalculator.Calculate(
            EnemyDetachment);

        ApplySimultaneousDamage(
            playerDamage,
            enemyDamage);

        PlayerDetachment.ApplyEndOfRoundEffects();
        EnemyDetachment.ApplyEndOfRoundEffects();
    }
}