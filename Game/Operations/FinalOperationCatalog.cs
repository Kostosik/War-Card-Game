using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;
using GameProject.Combat.Enemies;
using GameProject.Game.Campaign;

namespace GameProject.Game.Operations;

internal static class FinalOperationCatalog
{
    public static OperationDefinition Create(
        WarPositionLevel warPosition)
    {
        var battles = warPosition switch
        {
            WarPositionLevel.CriticalDefeat or
            WarPositionLevel.Retreat => CreateHardBattles(),

            WarPositionLevel.Advantage or
            WarPositionLevel.DecisiveAdvantage => CreateEasyBattles(),

            _ => CreateBalancedBattles()
        };

        var briefing = warPosition switch
        {
            WarPositionLevel.CriticalDefeat or
            WarPositionLevel.Retreat =>
                "Противник удерживает штаб усиленными силами. " +
                "Ожидается тяжёлое сопротивление.",

            WarPositionLevel.Advantage or
            WarPositionLevel.DecisiveAdvantage =>
                "Противник ослаблен предыдущими операциями. " +
                "Союзное превосходство облегчает наступление.",

            _ =>
                "Противник подготовил оборону штаба. " +
                "Исход сражения будет решаться на поле боя."
        };

        return new OperationDefinition(
            "final-command-center",
            "Штурм командного штаба противника",
            "Захватить командный центр и сорвать управление " +
            "войсками противника.",
            "Сохранить пехотный отряд боеспособным.",
            briefing,
            "Прорваться через внешнюю оборону, уничтожить резервы " +
            "и захватить командный центр.",
            battles);
    }

    private static OperationBattleDefinition[] CreateHardBattles()
    {
        return
        [
            Battle(
                "Внешний периметр",
                "Противник укрепил подступы к штабу.",
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.MechanizedPatrol,
                EnemyDetachmentCatalog.MachineGunPosition,
                5),

            Battle(
                "Укреплённый рубеж",
                "На пути наступления находятся бронетанковые части.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany,
                EnemyDetachmentCatalog.FieldArtillery,
                10),

            Battle(
                "Резерв штаба",
                "Противник бросает основные резервные силы.",
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.FieldArtillery,
                15),

            Battle(
                "Командный центр",
                "Последний рубеж обороны штаба.",
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.EntrenchedArtillery,
                20)
        ];
    }

    private static OperationBattleDefinition[] CreateBalancedBattles()
    {
        return
        [
            Battle(
                "Внешний периметр",
                "Разведывательные силы противника охраняют подступы.",
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.ScoutVehicles,
                EnemyDetachmentCatalog.MortarTeam),

            Battle(
                "Укреплённый рубеж",
                "Противник подготовил оборонительные позиции.",
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.MechanizedPatrol,
                EnemyDetachmentCatalog.MachineGunPosition,
                5),

            Battle(
                "Резерв штаба",
                "К месту сражения прибывают подкрепления.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany,
                EnemyDetachmentCatalog.FieldArtillery,
                5),

            Battle(
                "Командный центр",
                "Основные силы противника защищают штаб.",
                EnemyDetachmentCatalog.FortifiedInfantry,
                EnemyDetachmentCatalog.HeavyArmor,
                EnemyDetachmentCatalog.EntrenchedArtillery,
                10)
        ];
    }

    private static OperationBattleDefinition[] CreateEasyBattles()
    {
        return
        [
            Battle(
                "Внешний периметр",
                "Разрозненные силы охраняют подступы.",
                EnemyDetachmentCatalog.ReconInfantry,
                EnemyDetachmentCatalog.ScoutVehicles,
                EnemyDetachmentCatalog.MortarTeam),

            Battle(
                "Укреплённый рубеж",
                "Противник пытается организовать сопротивление.",
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.MechanizedPatrol,
                EnemyDetachmentCatalog.MortarTeam),

            Battle(
                "Резерв штаба",
                "Оставшиеся силы противника пытаются остановить наступление.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.LineInfantry,
                EnemyDetachmentCatalog.FieldArtillery),

            Battle(
                "Командный центр",
                "Последние защитники удерживают штаб.",
                EnemyDetachmentCatalog.AssaultInfantry,
                EnemyDetachmentCatalog.ArmoredCompany,
                EnemyDetachmentCatalog.FieldArtillery)
        ];
    }

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
}