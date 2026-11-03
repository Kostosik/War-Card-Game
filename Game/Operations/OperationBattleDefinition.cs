using GameProject.Combat.Battlefield;

namespace GameProject.Game.Operations;

internal sealed class OperationBattleDefinition
{
    public string Name { get; }
    public string Briefing { get; }

    public BattleDefinition Definition { get; }

    // Совместимые свойства для существующего кода.
    public EnemyBattlefieldDefinition EnemyForces =>
        Definition.EnemyForces;

    public BattleObjectiveDefinition Objective =>
        Definition.Objective;

    public BattleTerrainDefinition LeftTerrain =>
        Definition.LeftTerrain;

    public BattleTerrainDefinition CenterTerrain =>
        Definition.CenterTerrain;

    public BattleTerrainDefinition RightTerrain =>
        Definition.RightTerrain;

    public BattleFormationDefinition Formation =>
        Definition.Formation;

    // Новый конструктор.
    public OperationBattleDefinition(
        string name,
        string briefing,
        BattleDefinition definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(briefing);
        ArgumentNullException.ThrowIfNull(definition);

        Name = name;
        Briefing = briefing;
        Definition = definition;
    }

    // Старый конструктор сохраняем.
    // Существующие каталоги операций пока не требуют переписывания.
    public OperationBattleDefinition(
        string name,
        string briefing,
        EnemyBattlefieldDefinition enemyForces,
        BattleObjectiveDefinition? objective = null,
        BattleTerrainDefinition? leftTerrain = null,
        BattleTerrainDefinition? centerTerrain = null,
        BattleTerrainDefinition? rightTerrain = null,
        BattleFormationDefinition? formation = null)
        : this(
            name,
            briefing,
            new BattleDefinition(
                enemyForces,
                objective,
                leftTerrain,
                centerTerrain,
                rightTerrain,
                formation))
    {
    }
}