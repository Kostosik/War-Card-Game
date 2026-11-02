using GameProject.Combat.Army;
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
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.ScoutVehicles,
                EnemyDetachmentCatalog.MortarTeam),

            Battle("Промышленная зона", "Объект прикрывают линейные части.",
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.MachineGunPosition,
                EnemyDetachmentCatalog.MechanizedPatrol),

            Battle("Контратака", "Противник перебрасывает подкрепления.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany,
                EnemyDetachmentCatalog.FieldArtillery),

            Battle("Оборона комплекса", "Основные силы защищают объект.",
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.EntrenchedArtillery,
                15)
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
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.MortarTeam),

            Battle("Плацдарм", "Противник пытается закрепиться у переправы.",
                EnemyDetachmentCatalog.ScoutVehicles,
                EnemyDetachmentCatalog.MechanizedPatrol,
                EnemyDetachmentCatalog.MachineGunPosition),

            Battle("Подкрепления", "К переправе направлены дополнительные части.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.FieldArtillery,
                EnemyDetachmentCatalog.ArmoredCompany),

            Battle("Решающий натиск", "Противник бросает основные силы.",
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.EntrenchedArtillery,
                10)
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
                EnemyDetachmentCatalog.ScoutVehicles,
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.MortarTeam),

            Battle("Линия обороны", "Основные силы охраняют подступы.",
                EnemyDetachmentCatalog.MechanizedPatrol,
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.MachineGunPosition),

            Battle("Резерв противника", "Противник пытается остановить наступление.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany,
                EnemyDetachmentCatalog.EntrenchedArtillery),

            Battle("Артиллерийский узел", "Батареи прикрыты усиленной обороной.",
                EnemyDetachmentCatalog.FieldArtillery,
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.FortifiedInfantry,
                15)
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
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.ScoutVehicles),

            Battle("Складской район", "Склады охраняют регулярные части.",
                EnemyDetachmentCatalog.MortarTeam,
                EnemyDetachmentCatalog.MachineGunPosition,
                EnemyDetachmentCatalog.MechanizedPatrol),

            Battle("Засада", "Противник атакует силы сопровождения.",
                EnemyDetachmentCatalog.FieldArtillery,
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany),

            Battle("Удержание маршрута", "Последняя попытка противника сорвать снабжение.",
                EnemyDetachmentCatalog.EntrenchedArtillery,
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.HeavyArmor,
                10)
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

    public static OperationDefinition? FindById(string id)
    {
        return id switch
        {
            "oil-node" => FirstOperation,
            "industrial-facility" => IndustrialFacilityOperation,
            "river-crossing" => RiverCrossingOperation,
            "artillery-hub" => ArtilleryHubOperation,
            "supply-route" => SupplyRouteOperation,
            _ => null
        };
    }
}