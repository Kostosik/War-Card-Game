namespace GameProject.Combat.Cards;

public static class TacticalCardCatalog
{
    public static readonly CardDefinition ArtilleryStrike =
        new(
            "artillery-strike",
            CardKind.ArtilleryStrike,
            1,
            20);

    public static readonly CardDefinition SmokeScreen =
        new(
            "smoke-screen",
            CardKind.SmokeScreen,
            1,
            50);

    public static readonly CardDefinition Supply =
        new(
            "supply",
            CardKind.Supply,
            1,
            15);

    public static IReadOnlyList<CardDefinition> All { get; } =
        Array.AsReadOnly(new[]
        {
            ArtilleryStrike,
            SmokeScreen,
            Supply
        });
}