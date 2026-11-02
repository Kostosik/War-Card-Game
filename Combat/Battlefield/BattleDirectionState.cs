using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public sealed class BattleDirectionState
{
    public DetachmentState PlayerDetachment { get; }
    private int _pendingArtilleryDamage;
    private int _pendingSmokeReductionPercent;
    private int _pendingSupplyHealing;
    public int EnemyIncomingDamageReductionPercent { get; }
    public DetachmentState EnemyDetachment { get; }

    public int PendingArtilleryDamage =>
    _pendingArtilleryDamage;

    public int PendingSmokeReductionPercent =>
        _pendingSmokeReductionPercent;

    public int PendingSupplyHealing =>
        _pendingSupplyHealing;

    public bool IsFinished =>
        !PlayerDetachment.IsCombatReady ||
        !EnemyDetachment.IsCombatReady;

    public BattleDirectionState(
        DetachmentState playerDetachment,
        DetachmentState enemyDetachment, int enemyIncomingDamageReductionPercent = 0)
    {
        ArgumentNullException.ThrowIfNull(playerDetachment);
        ArgumentNullException.ThrowIfNull(enemyDetachment);

        PlayerDetachment = playerDetachment;
        EnemyDetachment = enemyDetachment;

        if (enemyIncomingDamageReductionPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(enemyIncomingDamageReductionPercent));
        }

        EnemyIncomingDamageReductionPercent =
            enemyIncomingDamageReductionPercent;
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
        var modifiedPlayerDamage = ScaleDamage(
            damageDealtByPlayer,
            100 - EnemyIncomingDamageReductionPercent);

        EnemyDetachment.ApplyDamage(modifiedPlayerDamage);
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

        // Снабжение действует до обмена ударами.
        PlayerDetachment.RestoreHealth(_pendingSupplyHealing);

        var playerDamage = DetachmentDamageCalculator.Calculate(
            PlayerDetachment);

        var enemyDamage = DetachmentDamageCalculator.Calculate(
            EnemyDetachment);

        // Артиллерия добавляет урон к атаке игрока.
        playerDamage = checked(
            playerDamage + _pendingArtilleryDamage);

        // Дымовая завеса уменьшает урон противника.
        enemyDamage = ScaleDamage(
            enemyDamage,
            100 - _pendingSmokeReductionPercent);

        // Обе стороны атакуют одновременно.
        ApplySimultaneousDamage(
            playerDamage,
            enemyDamage);

        PlayerDetachment.ApplyEndOfRoundEffects();
        EnemyDetachment.ApplyEndOfRoundEffects();

        // Все эффекты действовали только один раунд.
        ClearPendingEffects();
    }

    private static int ScaleDamage(int damage, int percent)
    {
        return (int)(((long)damage * percent + 50) / 100);
    }

    private void ClearPendingEffects()
    {
        _pendingArtilleryDamage = 0;
        _pendingSmokeReductionPercent = 0;
        _pendingSupplyHealing = 0;
    }

    public void QueueArtilleryStrike(int damage)
    {
        EnsureCanQueueEffect();

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(damage);

        _pendingArtilleryDamage =
            checked(_pendingArtilleryDamage + damage);
    }

    public void QueueSmokeScreen(int reductionPercent)
    {
        EnsureCanQueueEffect();

        if (reductionPercent <= 0 || reductionPercent > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reductionPercent));
        }

        _pendingSmokeReductionPercent = Math.Min(
            100,
            _pendingSmokeReductionPercent + reductionPercent);
    }

    public void QueueSupply(int healing)
    {
        EnsureCanQueueEffect();

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(healing);

        _pendingSupplyHealing =
            checked(_pendingSupplyHealing + healing);
    }

    private void EnsureCanQueueEffect()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot queue effects for a finished battle direction.");
        }
    }
}