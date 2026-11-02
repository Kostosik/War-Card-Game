using GameProject.Combat.Army;

namespace GameProject.Combat.Enemies;

internal static class EnemyDetachmentCatalog
{
    // Сражение 1 — разведывательный дозор.
    public static readonly DetachmentDefinition ReconInfantry =
        new("enemy-recon-infantry", "Разведывательная пехота", 75, 15);

    public static readonly DetachmentDefinition ScoutVehicles =
        new("enemy-scout-vehicles", "Разведывательная бронегруппа", 85, 17);

    public static readonly DetachmentDefinition MortarTeam =
        new("enemy-mortar-team", "Миномётный расчёт", 70, 14);

    // Сражение 2 — укреплённый блокпост.
    public static readonly DetachmentDefinition LineInfantry =
        new("enemy-line-infantry", "Линейная пехота", 95, 19);

    public static readonly DetachmentDefinition MechanizedPatrol =
        new("enemy-mechanized-patrol", "Механизированный дозор", 105, 21);

    public static readonly DetachmentDefinition MachineGunPosition =
        new("enemy-machine-gun", "Пулемётная позиция", 90, 23);

    // Сражение 3 — контрудар подкрепления.
    public static readonly DetachmentDefinition AssaultInfantry =
        new("enemy-assault-infantry", "Штурмовая пехота", 110, 23);

    public static readonly DetachmentDefinition ArmoredCompany =
        new("enemy-armored-company", "Бронегруппа", 125, 25);

    public static readonly DetachmentDefinition FieldArtillery =
        new("enemy-field-artillery", "Полевая артиллерия", 100, 26);

    // Сражение 4 — оборона нефтяного узла.
    public static readonly DetachmentDefinition FortifiedInfantry =
        new("enemy-fortified-infantry", "Укреплённая пехота", 135, 26);

    public static readonly DetachmentDefinition HeavyArmor =
        new("enemy-heavy-armor", "Тяжёлая бронегруппа", 145, 28);

    public static readonly DetachmentDefinition EntrenchedArtillery =
        new("enemy-entrenched-artillery", "Укреплённая артиллерия", 120, 30);
}