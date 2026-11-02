using GameProject.Combat.Battlefield;

namespace GameProject.Game.Operations;

internal sealed class OperationBattleDefinition
{
    public string Name { get; }
    public string Briefing { get; }
    public EnemyBattlefieldDefinition EnemyForces { get; }

    public OperationBattleDefinition(
        string name,
        string briefing,
        EnemyBattlefieldDefinition enemyForces)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(briefing);
        ArgumentNullException.ThrowIfNull(enemyForces);

        Name = name;
        Briefing = briefing;
        EnemyForces = enemyForces;
    }
}