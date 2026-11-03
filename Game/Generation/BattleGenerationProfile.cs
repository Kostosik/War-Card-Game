using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;

namespace GameProject.Game.Generation;

internal sealed class BattleGenerationProfile
{
    public static BattleGenerationProfile Standard { get; } =
        new(
            DetachmentCatalog.Enemy.All,
            BattleObjectiveCatalog.All,
            BattleTerrainCatalog.All,
            BattleFormationCatalog.All);

    public IReadOnlyList<DetachmentDefinition> EnemyDetachments { get; }

    public IReadOnlyList<BattleObjectiveDefinition> Objectives { get; }

    public IReadOnlyList<BattleTerrainDefinition> Terrains { get; }

    public IReadOnlyList<BattleFormationDefinition> Formations { get; }

    public int EnemyIncomingDamageReductionPercent { get; }

    public BattleGenerationProfile(
        IEnumerable<DetachmentDefinition> enemyDetachments,
        IEnumerable<BattleObjectiveDefinition> objectives,
        IEnumerable<BattleTerrainDefinition> terrains,
        IEnumerable<BattleFormationDefinition> formations,
        int enemyIncomingDamageReductionPercent = 0)
    {
        EnemyDetachments = CreateSnapshot(
            enemyDetachments,
            nameof(enemyDetachments));

        Objectives = CreateSnapshot(
            objectives,
            nameof(objectives));

        Terrains = CreateSnapshot(
            terrains,
            nameof(terrains));

        Formations = CreateSnapshot(
            formations,
            nameof(formations));

        if (EnemyDetachments.Count < 3)
        {
            throw new ArgumentException(
                "At least three enemy detachments are required.",
                nameof(enemyDetachments));
        }

        if (enemyIncomingDamageReductionPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(enemyIncomingDamageReductionPercent));
        }

        EnemyIncomingDamageReductionPercent =
            enemyIncomingDamageReductionPercent;
    }

    private static IReadOnlyList<T> CreateSnapshot<T>(
        IEnumerable<T> source,
        string parameterName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(source, parameterName);

        var items = source.ToArray();

        if (items.Length == 0)
        {
            throw new ArgumentException(
                "Generation candidates cannot be empty.",
                parameterName);
        }

        if (items.Any(item => item is null))
        {
            throw new ArgumentException(
                "Generation candidates cannot contain null values.",
                parameterName);
        }

        return Array.AsReadOnly(items);
    }
}