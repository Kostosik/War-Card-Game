using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;
using GameProject.Game.Campaign;

namespace GameProject.Game.Operations;

internal static class FinalOperationCatalog
{
    public const string Id = "final-command-center";

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
            Id,
            "Штурм командного штаба противника",
            "Захватить командный центр и сорвать управление " +
            "войсками противника.",
            "Сохранить пехотный отряд боеспособным.",
            briefing,
            "Прорваться через внешнюю оборону, уничтожить резервы " +
            "и захватить командный центр.",
                        battles,
            new[]
            {
                DetachmentCatalog.Player.Infantry.Id
            });
    }

    private static OperationBattleDefinition[] CreateHardBattles()
    {
        return
        [
            Battle(
                "Внешний периметр",
                "Противник укрепил подступы к штабу.",
                DetachmentCatalog.Enemy.LineInfantry,
                DetachmentCatalog.Enemy.MechanizedPatrol,
                DetachmentCatalog.Enemy.MachineGunPosition,
                5),

            Battle(
                "Укреплённый рубеж",
                "На пути наступления находятся бронетанковые части.",
                DetachmentCatalog.Enemy.AssaultInfantry,
                DetachmentCatalog.Enemy.ArmoredCompany,
                DetachmentCatalog.Enemy.FieldArtillery,
                10),

            Battle(
                "Резерв штаба",
                "Противник бросает основные резервные силы.",
                DetachmentCatalog.Enemy.FortifiedInfantry,
                DetachmentCatalog.Enemy.HeavyArmor,
                DetachmentCatalog.Enemy.FieldArtillery,
                15),

            Battle(
                "Командный центр",
                "Последний рубеж обороны штаба.",
                DetachmentCatalog.Enemy.FortifiedInfantry,
                DetachmentCatalog.Enemy.HeavyArmor,
                DetachmentCatalog.Enemy.EntrenchedArtillery,
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
                DetachmentCatalog.Enemy.ReconInfantry,
                DetachmentCatalog.Enemy.ScoutVehicles,
                DetachmentCatalog.Enemy.MortarTeam),

            Battle(
                "Укреплённый рубеж",
                "Противник подготовил оборонительные позиции.",
                DetachmentCatalog.Enemy.LineInfantry,
                DetachmentCatalog.Enemy.MechanizedPatrol,
                DetachmentCatalog.Enemy.MachineGunPosition,
                5),

            Battle(
                "Резерв штаба",
                "К месту сражения прибывают подкрепления.",
                DetachmentCatalog.Enemy.AssaultInfantry,
                DetachmentCatalog.Enemy.ArmoredCompany,
                DetachmentCatalog.Enemy.FieldArtillery,
                5),

            Battle(
                "Командный центр",
                "Основные силы противника защищают штаб.",
                DetachmentCatalog.Enemy.FortifiedInfantry,
                DetachmentCatalog.Enemy.HeavyArmor,
                DetachmentCatalog.Enemy.EntrenchedArtillery,
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
                DetachmentCatalog.Enemy.ReconInfantry,
                DetachmentCatalog.Enemy.ScoutVehicles,
                DetachmentCatalog.Enemy.MortarTeam),

            Battle(
                "Укреплённый рубеж",
                "Противник пытается организовать сопротивление.",
                DetachmentCatalog.Enemy.LineInfantry,
                DetachmentCatalog.Enemy.MechanizedPatrol,
                DetachmentCatalog.Enemy.MortarTeam),

            Battle(
                "Резерв штаба",
                "Оставшиеся силы противника пытаются остановить наступление.",
                DetachmentCatalog.Enemy.AssaultInfantry,
                DetachmentCatalog.Enemy.LineInfantry,
                DetachmentCatalog.Enemy.FieldArtillery),

            Battle(
                "Командный центр",
                "Последние защитники удерживают штаб.",
                DetachmentCatalog.Enemy.AssaultInfantry,
                DetachmentCatalog.Enemy.ArmoredCompany,
                DetachmentCatalog.Enemy.FieldArtillery)
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