using GameProject.Combat.Battlefield;
using GameProject.Combat.Enemies;

namespace GameProject.Game.Operations;

internal static class OperationCatalog
{
    public static OperationDefinition FirstOperation { get; } =
        new(
            "oil-node",
            "Захват нефтяного узла",
            "Прорваться к нефтяному узлу и захватить объект.",
            "Минимальные потери пехоты.",
            "Пехота, механизированные части и артиллерия.",
            "Три боя на марше, затем решающее сражение " +
            "с усиленной обороной объекта.",

            new[]
            {
                new OperationBattleDefinition(
                    "Разведывательный дозор",
                    "Небольшие силы противника охраняют подступы.",
                    new EnemyBattlefieldDefinition(
                        EnemyDetachmentCatalog.ReconInfantry,
                        EnemyDetachmentCatalog.ScoutVehicles,
                        EnemyDetachmentCatalog.MortarTeam)),

                new OperationBattleDefinition(
                    "Укреплённый блокпост",
                    "Противник закрепился на дороге и усилил оборону.",
                    new EnemyBattlefieldDefinition(
                        EnemyDetachmentCatalog.LineInfantry,
                        EnemyDetachmentCatalog.MechanizedPatrol,
                        EnemyDetachmentCatalog.MachineGunPosition)),

                new OperationBattleDefinition(
                    "Контрудар подкрепления",
                    "К нефтяному узлу направлены механизированные " +
                    "и артиллерийские подразделения.",
                    new EnemyBattlefieldDefinition(
                        EnemyDetachmentCatalog.AssaultInfantry,
                        EnemyDetachmentCatalog.ArmoredCompany,
                        EnemyDetachmentCatalog.FieldArtillery)),

                new OperationBattleDefinition(
                    "Оборона нефтяного узла",
                    "Основные силы противника удерживают объект. " +
                    "Ожидается наиболее ожесточённое сопротивление.",
                    new EnemyBattlefieldDefinition(
                        EnemyDetachmentCatalog.FortifiedInfantry,
                        EnemyDetachmentCatalog.HeavyArmor,
                        EnemyDetachmentCatalog.EntrenchedArtillery,
                        enemyIncomingDamageReductionPercent: 20))
            });
}