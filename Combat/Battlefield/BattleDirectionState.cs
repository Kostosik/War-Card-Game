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
    public BattleTerrainDefinition Terrain { get; }
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
        DetachmentState enemyDetachment, int enemyIncomingDamageReductionPercent = 0, BattleTerrainDefinition terrain = null)
    {
        ArgumentNullException.ThrowIfNull(playerDetachment);
        ArgumentNullException.ThrowIfNull(enemyDetachment);
        Terrain = terrain ?? BattleTerrainCatalog.OpenField;
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

    public void ResolveStep(
        int playerSupportDamage = 0,
        int enemySupportDamage = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(playerSupportDamage);
        ArgumentOutOfRangeException.ThrowIfNegative(enemySupportDamage);

        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot advance a finished battle direction.");
        }

        PlayerDetachment.RestoreHealth(_pendingSupplyHealing);

        var playerDamage = DetachmentDamageCalculator.Calculate(
            PlayerDetachment);

        var enemyDamage = DetachmentDamageCalculator.Calculate(
            EnemyDetachment);

        playerDamage = checked(
    playerDamage + playerSupportDamage);

        enemyDamage = checked(
            enemyDamage + enemySupportDamage);

        // Река снижает урон механизированных отрядов.
        playerDamage = ApplyTerrainAttackPenalty(
            PlayerDetachment,
            playerDamage);

        enemyDamage = ApplyTerrainAttackPenalty(
            EnemyDetachment,
            enemyDamage);

        // Артиллерийская поддержка добавляет урон.
        playerDamage = checked(
            playerDamage + _pendingArtilleryDamage);

        // Дым снижает входящий урон противника.
        enemyDamage = ScaleDamage(
            enemyDamage,
            100 - _pendingSmokeReductionPercent);

        // Город защищает обе стороны.
        playerDamage = ScaleDamage(
            playerDamage,
            100 - Terrain.DefenseBonusPercent);

        enemyDamage = ScaleDamage(
            enemyDamage,
            100 - Terrain.DefenseBonusPercent);

        ApplySimultaneousDamage(
            playerDamage,
            enemyDamage);

        PlayerDetachment.ApplyEndOfRoundEffects();
        EnemyDetachment.ApplyEndOfRoundEffects();

        ClearPendingEffects();
    }
    private int ApplyTerrainAttackPenalty(
    DetachmentState attacker,
    int damage)
    {
        if (attacker.Definition.Type != DetachmentType.Mechanized)
            return damage;

        return ScaleDamage(
            damage,
            100 - Terrain.MechanizedAttackPenaltyPercent);
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
        damage = ScaleDamage(
    damage,
    100 + Terrain.ArtilleryStrikeBonusPercent);
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
        reductionPercent = ScaleDamage(
    reductionPercent,
    100 + Terrain.SmokeScreenBonusPercent);
        _pendingSmokeReductionPercent = Math.Min(
            100,
            _pendingSmokeReductionPercent + reductionPercent);
    }

    public void QueueSupply(int healing)
    {
        EnsureCanQueueEffect();

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(healing);
        healing = ScaleDamage(
    healing,
    100 + Terrain.SupplyBonusPercent);
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