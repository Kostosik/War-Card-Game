namespace GameProject.Combat.Cards;

public static class TacticalCardCatalog
{
    public static readonly CardDefinition ArtilleryStrike =
        new(
            "artillery-strike",
            "АРТИЛЛЕРИЯ",
            "Наносит 20 дополнительного урона",
            1,
            20,
            5,
            new CardAppearance(110, 55, 45),
            CardEffects.ArtilleryStrike);

    //CardEffects.ArtilleryStrike,
    //new AssetPath("Cards/ArtilleryStrike.png"));

    public static readonly CardDefinition SmokeScreen =
        new(
            "smoke-screen",
            "ДЫМОВАЯ ЗАВЕСА",
            "Снижает урон противника на 50%",
            1,
            50,
            5,
            new CardAppearance(55, 75, 95),
            CardEffects.SmokeScreen);

    public static readonly CardDefinition Supply =
        new(
            "supply",
            "СНАБЖЕНИЕ",
            "Восстанавливает 15 здоровья",
            1,
            15,
            5,
            new CardAppearance(55, 95, 65),
            CardEffects.Supply);

    public static readonly CardDefinition HeavyArtillery =
    new(
        "heavy-artillery",
        "ТЯЖЁЛАЯ АРТИЛЛЕРИЯ",
        "Наносит 30 дополнительного урона.",
        2,
        30,
        2,
        new CardAppearance(135, 65, 45),
        CardEffects.ArtilleryStrike);

    public static readonly CardDefinition EmergencySupply =
        new(
            "emergency-supply",
            "АВАРИЙНОЕ СНАБЖЕНИЕ",
            "Восстанавливает 25 здоровья.",
            1,
            25,
            2,
            new CardAppearance(75, 115, 75),
            CardEffects.Supply);

    public static IReadOnlyList<CardDefinition> Unlockable { get; } =
        Array.AsReadOnly(
        [
            HeavyArtillery,
        EmergencySupply
        ]);

    public static IReadOnlyList<CardDefinition> All { get; } =
        Array.AsReadOnly(
        [
            ArtilleryStrike,
            SmokeScreen,
            Supply
        ]);
}