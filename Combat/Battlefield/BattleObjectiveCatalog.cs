namespace GameProject.Combat.Battlefield;

public static class BattleObjectiveCatalog
{
    public static IReadOnlyList<BattleObjectiveDefinition> All { get; } =
        Array.AsReadOnly(Build());

    private static BattleObjectiveDefinition[] Build()
    {
        var result = new List<BattleObjectiveDefinition>
        {
            BattleObjectiveDefinition.ControlTerritory
        };

        var directions = new[]
        {
            BattleDirection.LeftFlank,
            BattleDirection.Center,
            BattleDirection.RightFlank
        };

        foreach (var direction in directions)
        {
            result.Add(
                BattleObjectiveDefinition.Breakthrough(direction));

            foreach (var turns in new[] { 2, 3, 4 })
            {
                result.Add(
                    BattleObjectiveDefinition.DefendPosition(
                        direction,
                        turns));
            }
        }

        foreach (var turns in new[] { 3, 4, 5 })
        {
            result.Add(
                BattleObjectiveDefinition.ContainEnemy(turns));
        }

        return result.ToArray();
    }

    public static BattleObjectiveDefinition? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.FirstOrDefault(
            objective => objective.Id == id);
    }
}