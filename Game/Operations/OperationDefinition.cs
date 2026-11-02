namespace GameProject.Game.Operations;

internal sealed class OperationDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string MainObjective { get; }
    public string AdditionalObjective { get; }
    public string EnemyBriefing { get; }
    public string BattlePlan { get; }

    public const int BattleCount = 4;

    public IReadOnlyList<OperationBattleDefinition> Battles { get; }

    public OperationDefinition(
        string id,
        string name,
        string mainObjective,
        string additionalObjective,
        string enemyBriefing,
        string battlePlan, IEnumerable<OperationBattleDefinition> battles)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(mainObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalObjective);
        ArgumentException.ThrowIfNullOrWhiteSpace(enemyBriefing);
        ArgumentException.ThrowIfNullOrWhiteSpace(battlePlan);

        ArgumentNullException.ThrowIfNull(battles);

        var battleArray = battles.ToArray();

        if (battleArray.Length != BattleCount)
        {
            throw new ArgumentException(
                $"An operation must contain exactly {BattleCount} battles.",
                nameof(battles));
        }

        if (battleArray.Any(battle => battle is null))
        {
            throw new ArgumentException(
                "Battle definitions cannot contain null entries.",
                nameof(battles));
        }

        Battles = Array.AsReadOnly(battleArray);

        Id = id;
        Name = name;
        MainObjective = mainObjective;
        AdditionalObjective = additionalObjective;
        EnemyBriefing = enemyBriefing;
        BattlePlan = battlePlan;
        
    }
}