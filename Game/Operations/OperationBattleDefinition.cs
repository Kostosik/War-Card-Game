using GameProject.Combat.Battlefield;

namespace GameProject.Game.Operations;

internal sealed class OperationBattleDefinition
{
    public string Name { get; }
    public string Briefing { get; }
    public EnemyBattlefieldDefinition EnemyForces { get; }

    public BattleObjectiveDefinition Objective { get; }
    public BattleTerrainDefinition LeftTerrain { get; }
    public BattleTerrainDefinition CenterTerrain { get; }
    public BattleTerrainDefinition RightTerrain { get; }
    public OperationBattleDefinition(
        string name,
        string briefing,
        EnemyBattlefieldDefinition enemyForces,
        BattleObjectiveDefinition? objective = null, BattleTerrainDefinition? leftTerrain = null,
BattleTerrainDefinition? centerTerrain = null,
BattleTerrainDefinition? rightTerrain = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(briefing);
        ArgumentNullException.ThrowIfNull(enemyForces);

        Name = name;
        Briefing = briefing;
        EnemyForces = enemyForces;

        Objective = objective ??
            BattleObjectiveDefinition.ControlTerritory;
        LeftTerrain = leftTerrain ?? BattleTerrainCatalog.OpenField;
        CenterTerrain = centerTerrain ?? BattleTerrainCatalog.OpenField;
        RightTerrain = rightTerrain ?? BattleTerrainCatalog.OpenField;
    }
}