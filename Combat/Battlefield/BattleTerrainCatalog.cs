namespace GameProject.Combat.Battlefield;

public static class BattleTerrainCatalog
{
    public static readonly BattleTerrainDefinition OpenField =
        new(
            "open-field",
            "Открытая местность",
            "Местность не даёт дополнительных преимуществ.");

    public static readonly BattleTerrainDefinition Forest =
        new(
            "forest",
            "Лес",
            "Дымовая завеса действует эффективнее.",
            smokeScreenBonusPercent: 25);

    public static readonly BattleTerrainDefinition Hill =
        new(
            "hill",
            "Возвышенность",
            "Артиллерийские удары наносят больше урона.",
            artilleryStrikeBonusPercent: 25);

    public static readonly BattleTerrainDefinition Ruins =
        new(
            "ruins",
            "Руины",
            "Снабжение эффективнее восстанавливает здоровье.",
            supplyBonusPercent: 25);

    public static readonly BattleTerrainDefinition City =
        new(
            "city",
            "Город",
            "Застройка обеспечивает дополнительную защиту.",
            defenseBonusPercent: 20);

    public static readonly BattleTerrainDefinition River =
        new(
            "river",
            "Река",
            "Переправа затрудняет действия механизированных войск.",
            mechanizedAttackPenaltyPercent: 25);

    public static IReadOnlyList<BattleTerrainDefinition> All { get; } =
        Array.AsReadOnly(
        [
            OpenField,
            Forest,
            Hill,
            Ruins,
            City,
            River
        ]);
}