namespace GameProject.Game.Operations;

internal sealed class OperationDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string MainObjective { get; }
    public string AdditionalObjective { get; }
    public string EnemyBriefing { get; }
    public string BattlePlan { get; }

    public OperationDefinition(
        string id,
        string name,
        string mainObjective,
        string additionalObjective,
        string enemyBriefing,
        string battlePlan)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(mainObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(enemyBriefing);
        ArgumentException.ThrowIfNullOrWhiteSpace(battlePlan);

        Id = id;
        Name = name;
        MainObjective = mainObjective;
        AdditionalObjective = additionalObjective;
        EnemyBriefing = enemyBriefing;
        BattlePlan = battlePlan;
    }
}