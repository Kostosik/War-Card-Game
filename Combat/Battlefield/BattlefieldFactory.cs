using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
    private static readonly DetachmentDefinition PlayerInfantry =
        new("player-infantry", "Пехота", 100, 24);

    private static readonly DetachmentDefinition EnemyInfantry =
        new("enemy-infantry", "Пехота противника", 100, 20);

    private static readonly DetachmentDefinition PlayerMechanized =
        new("player-mechanized", "Механизированные войска", 140, 20);

    private static readonly DetachmentDefinition EnemyMechanized =
        new("enemy-mechanized", "Механизированные войска противника", 110, 22);

    private static readonly DetachmentDefinition PlayerArtillery =
        new("player-artillery", "Артиллерия", 90, 28);

    private static readonly DetachmentDefinition EnemyArtillery =
        new("enemy-artillery", "Артиллерия противника", 100, 18);

    public static BattlefieldState CreatePrototype()
    {
        return new BattlefieldState(
            CreateDirection(PlayerInfantry, EnemyInfantry),
            CreateDirection(PlayerMechanized, EnemyMechanized),
            CreateDirection(PlayerArtillery, EnemyArtillery));
    }

    private static BattleDirectionState CreateDirection(
        DetachmentDefinition player,
        DetachmentDefinition enemy)
    {
        return new BattleDirectionState(
            new DetachmentState(player),
            new DetachmentState(enemy));
    }
}