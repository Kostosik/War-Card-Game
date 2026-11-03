namespace GameProject.Combat.Battlefield;

public enum BattleObjectiveType
{
    ControlTerritory,
    Breakthrough,
    DefendPosition,
    ContainEnemy,
    Custom
}

public sealed class BattleObjectiveDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public IBattleObjectiveRule Rule { get; }

    // Старые свойства пока сохраняем.
    // Новые игровые системы должны использовать Rule.
    public BattleObjectiveType Type { get; }
    public BattleDirection? TargetDirection { get; }
    public int RequiredTurns { get; }

    public static BattleObjectiveDefinition ControlTerritory { get; } =
        new(
            "control-territory",
            "Контроль территории",
            "Захватить два из трёх направлений.",
            new ControlTerritoryObjectiveRule(),
            BattleObjectiveType.ControlTerritory);

    public static BattleObjectiveDefinition Breakthrough(
        BattleDirection direction)
    {
        return new BattleObjectiveDefinition(
            $"breakthrough-{direction}",
            "Прорыв",
            "Захватить указанное направление.",
            new BreakthroughObjectiveRule(direction),
            BattleObjectiveType.Breakthrough,
            direction);
    }

    public static BattleObjectiveDefinition DefendPosition(
        BattleDirection direction,
        int requiredTurns)
    {
        return new BattleObjectiveDefinition(
            $"defend-{direction}-{requiredTurns}",
            "Оборона позиции",
            $"Удерживать направление в течение {requiredTurns} ходов.",
            new DefendPositionObjectiveRule(direction, requiredTurns),
            BattleObjectiveType.DefendPosition,
            direction,
            requiredTurns);
    }

    public static BattleObjectiveDefinition ContainEnemy(int requiredTurns)
    {
        return new BattleObjectiveDefinition(
            $"contain-enemy-{requiredTurns}",
            "Сдерживание противника",
            $"Не позволить противнику захватить два направления в течение {requiredTurns} ходов.",
            new ContainEnemyObjectiveRule(requiredTurns),
            BattleObjectiveType.ContainEnemy,
            null,
            requiredTurns);
    }

    // Для добавления пользовательских целей.
    public BattleObjectiveDefinition(
        string id,
        string name,
        string description,
        IBattleObjectiveRule rule)
        : this(
            id,
            name,
            description,
            rule,
            BattleObjectiveType.Custom)
    {
    }

    private BattleObjectiveDefinition(
        string id,
        string name,
        string description,
        IBattleObjectiveRule rule,
        BattleObjectiveType type,
        BattleDirection? targetDirection = null,
        int requiredTurns = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(rule);

        Id = id;
        Name = name;
        Description = description;
        Rule = rule;

        Type = type;
        TargetDirection = targetDirection;
        RequiredTurns = requiredTurns;
    }
}