namespace GameProject.Game.Operations;

internal sealed class OperationDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string MainObjective { get; }
    public string AdditionalObjective { get; }
    public string EnemyBriefing { get; }
    public string BattlePlan { get; }

    public IReadOnlyList<string> AdditionalObjectiveDetachmentIds { get; }
    public IReadOnlyList<OperationBattleDefinition> Battles { get; }

    public int BattleCount => Battles.Count;

    public OperationDefinition(
        string id,
        string name,
        string mainObjective,
        string additionalObjective,
        string enemyBriefing,
        string battlePlan,
        IEnumerable<OperationBattleDefinition> battles,
        IReadOnlyList<string> additionalObjectiveDetachmentIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(mainObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(enemyBriefing);
        ArgumentException.ThrowIfNullOrWhiteSpace(battlePlan);

        ArgumentNullException.ThrowIfNull(battles);
        ArgumentNullException.ThrowIfNull(additionalObjectiveDetachmentIds);

        var battleArray = battles.ToArray();

        if (battleArray.Length == 0)
        {
            throw new ArgumentException(
                "An operation must contain at least one battle.",
                nameof(battles));
        }

        if (battleArray.Any(battle => battle is null))
        {
            throw new ArgumentException(
                "Battle definitions cannot contain null entries.",
                nameof(battles));
        }

        Id = id;
        Name = name;
        MainObjective = mainObjective;
        AdditionalObjective = additionalObjective;
        EnemyBriefing = enemyBriefing;
        BattlePlan = battlePlan;

        Battles = Array.AsReadOnly(battleArray);

        AdditionalObjectiveDetachmentIds =
            Array.AsReadOnly(additionalObjectiveDetachmentIds.ToArray());
    }
}