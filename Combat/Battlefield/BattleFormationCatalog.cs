namespace GameProject.Combat.Battlefield;

public static class BattleFormationCatalog
{
    public static readonly BattleFormationDefinition LinearFront =
        new(
            "linear-front",
            "Линейный фронт",
            "Центр связан с обоими флангами.",
            [
                new BattleDirectionConnection(
                    BattleDirection.LeftFlank,
                    BattleDirection.Center),

                new BattleDirectionConnection(
                    BattleDirection.Center,
                    BattleDirection.RightFlank)
            ],
            supportDamagePerNeighbor: 1);

    public static IReadOnlyList<BattleFormationDefinition> All { get; } =
        Array.AsReadOnly(
        [
            LinearFront
        ]);

    public static BattleFormationDefinition? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.FirstOrDefault(
            formation => formation.Id == id);
    }
}