using GameProject.Combat.Army;
using GameProject.Combat.Modifiers;

namespace GameProject.Combat.Battlefield;

public sealed class BattleDirectionState
{
    private readonly CombatModifierSet _modifiers = new();

    public DetachmentState PlayerDetachment { get; }
    public DetachmentState EnemyDetachment { get; }

    public BattleTerrainDefinition Terrain { get; }

    public int EnemyIncomingDamageReductionPercent { get; }

    public int PendingArtilleryDamage =>
        _modifiers.Apply(
            CombatModifierStats.ArtilleryDamage,
            0,
            PlayerDetachment,
            CombatSide.Player,
            EnemyDetachment);

    public int PendingSmokeReductionPercent =>
        Math.Clamp(
            _modifiers.Apply(
                CombatModifierStats.SmokeReduction,
                0,
                PlayerDetachment,
                CombatSide.Player,
                EnemyDetachment),
            0,
            100);

    public int PendingSupplyHealing =>
        _modifiers.Apply(
            CombatModifierStats.SupplyHealing,
            0,
            PlayerDetachment,
            CombatSide.Player,
            EnemyDetachment);

    public bool IsFinished =>
        !PlayerDetachment.IsCombatReady ||
        !EnemyDetachment.IsCombatReady;

    public BattleDirectionState(
        DetachmentState playerDetachment,
        DetachmentState enemyDetachment,
        int enemyIncomingDamageReductionPercent = 0,
        BattleTerrainDefinition? terrain = null)
    {
        ArgumentNullException.ThrowIfNull(playerDetachment);
        ArgumentNullException.ThrowIfNull(enemyDetachment);

        if (enemyIncomingDamageReductionPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(
                nameof(enemyIncomingDamageReductionPercent));

        PlayerDetachment = playerDetachment;
        EnemyDetachment = enemyDetachment;

        Terrain = terrain ?? BattleTerrainCatalog.OpenField;

        EnemyIncomingDamageReductionPercent =
            enemyIncomingDamageReductionPercent;

        // Местность.
        _modifiers.AddPermanentRange(Terrain.Modifiers);

        // Приказы обеих сторон.
        if (PlayerDetachment.Order is { } playerOrder)
            _modifiers.AddPermanentRange(playerOrder.Modifiers);

        if (EnemyDetachment.Order is { } enemyOrder)
            _modifiers.AddPermanentRange(enemyOrder.Modifiers);

        // Сложность обороны противника.
        if (enemyIncomingDamageReductionPercent > 0)
        {
            _modifiers.AddPermanent(new CombatModifierDefinition(
                "enemy-battlefield-defense",
                CombatModifierStats.IncomingDamage,
                CombatModifierOperation.MultiplyPercent,
                100 - enemyIncomingDamageReductionPercent,
                CombatSide.Enemy,
                priority: 100));
        }
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

    public void AddModifier(CombatModifierDefinition modifier)
    {
        EnsureCanQueueEffect();
        _modifiers.AddPermanent(modifier);
    }

    public void AddTemporaryModifier(CombatModifierDefinition modifier)
    {
        EnsureCanQueueEffect();
        _modifiers.AddTemporary(modifier);
    }

    public void QueueArtilleryStrike(int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(damage);

        AddTemporaryModifier(new CombatModifierDefinition(
            "card-artillery-strike",
            CombatModifierStats.ArtilleryDamage,
            CombatModifierOperation.AddFlat,
            damage,
            CombatSide.Player));
    }

    public void QueueSmokeScreen(int reductionPercent)
    {
        if (reductionPercent is <= 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(reductionPercent));

        AddTemporaryModifier(new CombatModifierDefinition(
            "card-smoke-screen",
            CombatModifierStats.SmokeReduction,
            CombatModifierOperation.AddFlat,
            reductionPercent,
            CombatSide.Player));
    }

    public void QueueSupply(int healing)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(healing);

        AddTemporaryModifier(new CombatModifierDefinition(
            "card-supply",
            CombatModifierStats.SupplyHealing,
            CombatModifierOperation.AddFlat,
            healing,
            CombatSide.Player));
    }

    public void ApplySimultaneousDamage(
        int damageDealtByPlayer,
        int damageDealtByEnemy)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damageDealtByPlayer);
        ArgumentOutOfRangeException.ThrowIfNegative(damageDealtByEnemy);

        if (IsFinished)
            throw new InvalidOperationException(
                "Cannot advance a finished battle direction.");

        var modifiedPlayerDamage = _modifiers.Apply(
            CombatModifierStats.IncomingDamage,
            damageDealtByPlayer,
            EnemyDetachment,
            CombatSide.Enemy,
            PlayerDetachment);

        var modifiedEnemyDamage = _modifiers.Apply(
            CombatModifierStats.IncomingDamage,
            damageDealtByEnemy,
            PlayerDetachment,
            CombatSide.Player,
            EnemyDetachment);

        // Обе стороны получают урон одновременно.
        EnemyDetachment.ApplyDamage(modifiedPlayerDamage);
        PlayerDetachment.ApplyDamage(modifiedEnemyDamage);
    }

    public void ResolveStep(
        int playerSupportDamage = 0,
        int enemySupportDamage = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(playerSupportDamage);
        ArgumentOutOfRangeException.ThrowIfNegative(enemySupportDamage);

        if (IsFinished)
            throw new InvalidOperationException(
                "Cannot advance a finished battle direction.");

        // Поддержка соседних направлений действует один раунд.
        if (playerSupportDamage > 0)
        {
            _modifiers.AddTemporary(new CombatModifierDefinition(
                "direction-support-player",
                CombatModifierStats.OutgoingDamage,
                CombatModifierOperation.AddFlat,
                playerSupportDamage,
                CombatSide.Player,
                priority: 200));
        }

        if (enemySupportDamage > 0)
        {
            _modifiers.AddTemporary(new CombatModifierDefinition(
                "direction-support-enemy",
                CombatModifierStats.OutgoingDamage,
                CombatModifierOperation.AddFlat,
                enemySupportDamage,
                CombatSide.Enemy,
                priority: 200));
        }

        // Снабжение действует до обмена ударами.
        var supplyHealing = PendingSupplyHealing;

        PlayerDetachment.RestoreHealth(supplyHealing);

        // Базовый урон.
        var playerDamage = DetachmentDamageCalculator.Calculate(
            PlayerDetachment);

        var enemyDamage = DetachmentDamageCalculator.Calculate(
            EnemyDetachment);

        // Модификаторы атаки: приказы, местность, поддержка.
        playerDamage = _modifiers.Apply(
            CombatModifierStats.OutgoingDamage,
            playerDamage,
            PlayerDetachment,
            CombatSide.Player,
            EnemyDetachment);

        enemyDamage = _modifiers.Apply(
            CombatModifierStats.OutgoingDamage,
            enemyDamage,
            EnemyDetachment,
            CombatSide.Enemy,
            PlayerDetachment);

        // Артиллерийская карта.
        playerDamage = checked(
            playerDamage + PendingArtilleryDamage);

        // Дымовая завеса.
        enemyDamage = ScaleDamage(
            enemyDamage,
            100 - PendingSmokeReductionPercent);

        ApplySimultaneousDamage(
            playerDamage,
            enemyDamage);

        // Восстановление после раунда.
        var playerHealing = _modifiers.Apply(
            CombatModifierStats.EndOfRoundHealing,
            0,
            PlayerDetachment,
            CombatSide.Player,
            EnemyDetachment);

        var enemyHealing = _modifiers.Apply(
            CombatModifierStats.EndOfRoundHealing,
            0,
            EnemyDetachment,
            CombatSide.Enemy,
            PlayerDetachment);

        PlayerDetachment.RestoreHealth(playerHealing);
        EnemyDetachment.RestoreHealth(enemyHealing);

        // Карты и поддержка действовали один раунд.
        _modifiers.ClearTemporary();
    }

    private static int ScaleDamage(int damage, int percent)
    {
        return (int)(((long)damage * percent + 50) / 100);
    }

    private void EnsureCanQueueEffect()
    {
        if (IsFinished)
            throw new InvalidOperationException(
                "Cannot queue effects for a finished battle direction.");
    }
}