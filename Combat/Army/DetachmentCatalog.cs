using System.Collections.ObjectModel;

namespace GameProject.Combat.Army;

public static class DetachmentCatalog
{
    public static class Player
    {
        public static readonly DetachmentDefinition Infantry =
            new(
                "infantry",
                "Пехота",
                100,
                24,
                [DetachmentTags.Infantry]);

        public static readonly DetachmentDefinition Mechanized =
            new(
                "mechanized",
                "Механизированные войска",
                120,
                20,
                [
                    DetachmentTags.Mechanized,
                    DetachmentTags.Armored
                ]);

        public static readonly DetachmentDefinition Artillery =
            new(
                "artillery",
                "Артиллерия",
                90,
                28,
                [DetachmentTags.Artillery]);

        public static ReadOnlyCollection<DetachmentDefinition> All { get; } =
            Array.AsReadOnly(
            [
                Infantry,
                Mechanized,
                Artillery
            ]);
    }

    public static class Enemy
    {
        public static readonly DetachmentDefinition ReconInfantry =
            new(
                "enemy-recon-infantry",
                "Разведывательная пехота",
                75,
                15,
                [
                    DetachmentTags.Infantry,
                    DetachmentTags.Recon
                ]);

        public static readonly DetachmentDefinition ScoutVehicles =
            new(
                "enemy-scout-vehicles",
                "Разведывательная бронегруппа",
                85,
                17,
                [
                    DetachmentTags.Mechanized,
                    DetachmentTags.Armored,
                    DetachmentTags.Recon
                ]);

        public static readonly DetachmentDefinition MortarTeam =
            new(
                "enemy-mortar-team",
                "Миномётный расчёт",
                70,
                14,
                [DetachmentTags.Artillery]);

        public static readonly DetachmentDefinition LineInfantry =
            new(
                "enemy-line-infantry",
                "Линейная пехота",
                95,
                19,
                [DetachmentTags.Infantry]);

        public static readonly DetachmentDefinition MechanizedPatrol =
            new(
                "enemy-mechanized-patrol",
                "Механизированный дозор",
                105,
                21,
                [
                    DetachmentTags.Mechanized,
                    DetachmentTags.Armored
                ]);

        public static readonly DetachmentDefinition MachineGunPosition =
            new(
                "enemy-machine-gun",
                "Пулемётная позиция",
                90,
                23,
                [
                    DetachmentTags.Infantry,
                    DetachmentTags.Fortified
                ]);

        public static readonly DetachmentDefinition AssaultInfantry =
            new(
                "enemy-assault-infantry",
                "Штурмовая пехота",
                110,
                23,
                [
                    DetachmentTags.Infantry,
                    DetachmentTags.Assault
                ]);

        public static readonly DetachmentDefinition ArmoredCompany =
            new(
                "enemy-armored-company",
                "Бронегруппа",
                125,
                25,
                [
                    DetachmentTags.Mechanized,
                    DetachmentTags.Armored
                ]);

        public static readonly DetachmentDefinition FieldArtillery =
            new(
                "enemy-field-artillery",
                "Полевая артиллерия",
                100,
                26,
                [DetachmentTags.Artillery]);

        public static readonly DetachmentDefinition FortifiedInfantry =
            new(
                "enemy-fortified-infantry",
                "Укреплённая пехота",
                135,
                26,
                [
                    DetachmentTags.Infantry,
                    DetachmentTags.Fortified
                ]);

        public static readonly DetachmentDefinition HeavyArmor =
            new(
                "enemy-heavy-armor",
                "Тяжёлая бронегруппа",
                145,
                28,
                [
                    DetachmentTags.Mechanized,
                    DetachmentTags.Armored,
                    DetachmentTags.Heavy
                ]);

        public static readonly DetachmentDefinition EntrenchedArtillery =
            new(
                "enemy-entrenched-artillery",
                "Укреплённая артиллерия",
                120,
                30,
                [
                    DetachmentTags.Artillery,
                    DetachmentTags.Fortified
                ]);

        public static ReadOnlyCollection<DetachmentDefinition> All { get; } =
            Array.AsReadOnly(
            [
                ReconInfantry,
                ScoutVehicles,
                MortarTeam,
                LineInfantry,
                MechanizedPatrol,
                MachineGunPosition,
                AssaultInfantry,
                ArmoredCompany,
                FieldArtillery,
                FortifiedInfantry,
                HeavyArmor,
                EntrenchedArtillery
            ]);
    }

    public static ReadOnlyCollection<DetachmentDefinition> All { get; } =
        Array.AsReadOnly(
            Player.All.Concat(Enemy.All).ToArray());

    public static ReadOnlyCollection<DetachmentDefinition> CreateInitialRoster()
    {
        return Player.All;
    }
}