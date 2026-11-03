namespace GameProject.Combat.Battlefield;

public sealed class BattleDefinition
{
    public EnemyBattlefieldDefinition EnemyForces { get; }

    public BattleObjectiveDefinition Objective { get; }

    public BattleTerrainDefinition LeftTerrain { get; }
    public BattleTerrainDefinition CenterTerrain { get; }
    public BattleTerrainDefinition RightTerrain { get; }

    public BattleFormationDefinition Formation { get; }

    public BattleDefinition(
        EnemyBattlefieldDefinition enemyForces,
        BattleObjectiveDefinition? objective = null,
        BattleTerrainDefinition? leftTerrain = null,
        BattleTerrainDefinition? centerTerrain = null,
        BattleTerrainDefinition? rightTerrain = null,
        BattleFormationDefinition? formation = null)
    {
        ArgumentNullException.ThrowIfNull(enemyForces);

        EnemyForces = enemyForces;

        Objective = objective ??
            BattleObjectiveDefinition.ControlTerritory;

        LeftTerrain = leftTerrain ??
            BattleTerrainCatalog.OpenField;

        CenterTerrain = centerTerrain ??
            BattleTerrainCatalog.OpenField;

        RightTerrain = rightTerrain ??
            BattleTerrainCatalog.OpenField;

        Formation = formation ??
            BattleFormationCatalog.LinearFront;
    }
}