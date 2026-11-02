using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;

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
                        DetachmentCatalog.Enemy.ReconInfantry,
                        DetachmentCatalog.Enemy.ScoutVehicles,
                        DetachmentCatalog.Enemy.MortarTeam)),

                new OperationBattleDefinition(
                    "Укреплённый блокпост",
                    "Противник закрепился на дороге и усилил оборону.",
                    new EnemyBattlefieldDefinition(
                        DetachmentCatalog.Enemy.LineInfantry,
                        DetachmentCatalog.Enemy.MechanizedPatrol,
                        DetachmentCatalog.Enemy.MachineGunPosition)),

                new OperationBattleDefinition(
                    "Контрудар подкрепления",
                    "К нефтяному узлу направлены механизированные " +
                    "и артиллерийские подразделения.",
                    new EnemyBattlefieldDefinition(
                        DetachmentCatalog.Enemy.AssaultInfantry,
                        DetachmentCatalog.Enemy.ArmoredCompany,
                        DetachmentCatalog.Enemy.FieldArtillery)),

                new OperationBattleDefinition(
                    "Оборона нефтяного узла",
                    "Основные силы противника удерживают объект. " +
                    "Ожидается наиболее ожесточённое сопротивление.",
                    new EnemyBattlefieldDefinition(
                        DetachmentCatalog.Enemy.FortifiedInfantry,
                        DetachmentCatalog.Enemy.HeavyArmor,
                        DetachmentCatalog.Enemy.EntrenchedArtillery,
                        enemyIncomingDamageReductionPercent: 20))
            },
            new[]
            {
                DetachmentCatalog.Player.Infantry.Id
            });

    public static OperationDefinition IndustrialFacilityOperation { get; } =
        new(
            "industrial-facility",
            "Захват промышленного объекта",
            "Захватить промышленный комплекс и обеспечить его работу.",
            "Сохранить пехотные подразделения.",
            "Укреплённая пехота, бронегруппы и артиллерия.",
            "Прорваться через внешнюю оборону и захватить промышленный комплекс.",
            new[]
            {
                Battle("Внешний периметр", "Разведывательные силы противника.",
                    DetachmentCatalog.Enemy.ReconInfantry,
                    DetachmentCatalog.Enemy.ScoutVehicles,
                    DetachmentCatalog.Enemy.MortarTeam),

                Battle("Промышленная зона", "Объект прикрывают линейные части.",
                    DetachmentCatalog.Enemy.LineInfantry,
                    DetachmentCatalog.Enemy.MachineGunPosition,
                    DetachmentCatalog.Enemy.MechanizedPatrol),

                Battle("Контратака", "Противник перебрасывает подкрепления.",
                    DetachmentCatalog.Enemy.AssaultInfantry,
                    DetachmentCatalog.Enemy.ArmoredCompany,
                    DetachmentCatalog.Enemy.FieldArtillery),

                Battle("Оборона комплекса", "Основные силы защищают объект.",
                    DetachmentCatalog.Enemy.FortifiedInfantry,
                    DetachmentCatalog.Enemy.HeavyArmor,
                    DetachmentCatalog.Enemy.EntrenchedArtillery,
                    15)
            },
            new[]
            {
                DetachmentCatalog.Player.Infantry.Id
            });

    public static OperationDefinition RiverCrossingOperation { get; } =
        new(
            "river-crossing",
            "Оборона переправы",
            "Удержать переправу и не допустить прорыва противника.",
            "Не допустить уничтожения механизированного отряда.",
            "Противник готовит наступление с нескольких направлений.",
            "Укрепить позиции и отразить последовательные атаки.",
            new[]
            {
                Battle("Подходы к реке", "Передовые силы противника.",
                    DetachmentCatalog.Enemy.LineInfantry,
                    DetachmentCatalog.Enemy.ReconInfantry,
                    DetachmentCatalog.Enemy.MortarTeam),

                Battle("Плацдарм", "Противник пытается закрепиться у переправы.",
                    DetachmentCatalog.Enemy.ScoutVehicles,
                    DetachmentCatalog.Enemy.MechanizedPatrol,
                    DetachmentCatalog.Enemy.MachineGunPosition),

                Battle("Подкрепления", "К переправе направлены дополнительные части.",
                    DetachmentCatalog.Enemy.AssaultInfantry,
                    DetachmentCatalog.Enemy.FieldArtillery,
                    DetachmentCatalog.Enemy.ArmoredCompany),

                Battle("Решающий натиск", "Противник бросает основные силы.",
                    DetachmentCatalog.Enemy.HeavyArmor,
                    DetachmentCatalog.Enemy.FortifiedInfantry,
                    DetachmentCatalog.Enemy.EntrenchedArtillery,
                    10)
            },
            new[]
            {
                DetachmentCatalog.Player.Mechanized.Id
            });

    public static OperationDefinition ArtilleryHubOperation { get; } =
        new(
            "artillery-hub",
            "Уничтожение артиллерийского узла",
            "Вывести из строя артиллерийские позиции противника.",
            "Сохранить механизированные войска.",
            "Противник прикрывает артиллерийские батареи пехотой и бронёй.",
            "Прорваться через оборону и уничтожить артиллерийский узел.",
            new[]
            {
                Battle("Передовые позиции", "Разведка и охрана батарей.",
                    DetachmentCatalog.Enemy.ScoutVehicles,
                    DetachmentCatalog.Enemy.ReconInfantry,
                    DetachmentCatalog.Enemy.MortarTeam),

                Battle("Линия обороны", "Основные силы охраняют подступы.",
                    DetachmentCatalog.Enemy.MechanizedPatrol,
                    DetachmentCatalog.Enemy.LineInfantry,
                    DetachmentCatalog.Enemy.MachineGunPosition),

                Battle("Резерв противника", "Противник пытается остановить наступление.",
                    DetachmentCatalog.Enemy.AssaultInfantry,
                    DetachmentCatalog.Enemy.ArmoredCompany,
                    DetachmentCatalog.Enemy.EntrenchedArtillery),

                Battle("Артиллерийский узел", "Батареи прикрыты усиленной обороной.",
                    DetachmentCatalog.Enemy.FieldArtillery,
                    DetachmentCatalog.Enemy.HeavyArmor,
                    DetachmentCatalog.Enemy.FortifiedInfantry,
                    15)
            },
            new[]
            {
                DetachmentCatalog.Player.Mechanized.Id
            });

    public static OperationDefinition SupplyRouteOperation { get; } =
        new(
            "supply-route",
            "Обеспечение линии снабжения",
            "Захватить маршрут снабжения и обеспечить проход конвоев.",
            "Сохранить пехотные подразделения.",
            "Противник готовит засады и охраняет склады.",
            "Расчистить маршрут и удержать ключевые точки снабжения.",
            new[]
            {
                Battle("Разведка маршрута", "На маршруте замечены дозоры.",
                    DetachmentCatalog.Enemy.ReconInfantry,
                    DetachmentCatalog.Enemy.LineInfantry,
                    DetachmentCatalog.Enemy.ScoutVehicles),

                Battle("Складской район", "Склады охраняют регулярные части.",
                    DetachmentCatalog.Enemy.MortarTeam,
                    DetachmentCatalog.Enemy.MachineGunPosition,
                    DetachmentCatalog.Enemy.MechanizedPatrol),

                Battle("Засада", "Противник атакует силы сопровождения.",
                    DetachmentCatalog.Enemy.FieldArtillery,
                    DetachmentCatalog.Enemy.AssaultInfantry,
                    DetachmentCatalog.Enemy.ArmoredCompany),

                Battle("Удержание маршрута", "Последняя попытка противника сорвать снабжение.",
                    DetachmentCatalog.Enemy.EntrenchedArtillery,
                    DetachmentCatalog.Enemy.FortifiedInfantry,
                    DetachmentCatalog.Enemy.HeavyArmor,
                    10)
            },
            new[]
            {
                DetachmentCatalog.Player.Infantry.Id
            });

    private static OperationBattleDefinition Battle(
        string name,
        string briefing,
        DetachmentDefinition left,
        DetachmentDefinition center,
        DetachmentDefinition right,
        int enemyDamageReduction = 0)
    {
        return new OperationBattleDefinition(
            name,
            briefing,
            new EnemyBattlefieldDefinition(
                left,
                center,
                right,
                enemyDamageReduction));
    }

    public static IReadOnlyList<OperationDefinition> All { get; } =
    Array.AsReadOnly(
    [
        FirstOperation,
        IndustrialFacilityOperation,
        RiverCrossingOperation,
        ArtilleryHubOperation,
        SupplyRouteOperation
    ]);

    public static OperationDefinition? FindById(string id)
    {
        return All.FirstOrDefault(operation => operation.Id == id);
    }
}