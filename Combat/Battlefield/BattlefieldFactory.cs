using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
    public static BattlefieldState CreatePrototype()
    {
        return new BattlefieldState(
            CreateDirection(
                player: (100, 100, 24, 8),
                enemy: (100, 100, 20, 6)),

            CreateDirection(
                player: (120, 120, 20, 12),
                enemy: (110, 110, 22, 10)),

            CreateDirection(
                player: (90, 90, 28, 4),
                enemy: (100, 100, 18, 8)));
    }

    private static BattleDirectionState CreateDirection(
        (int Personnel, int Health, int Attack, int Defense) player,
        (int Personnel, int Health, int Attack, int Defense) enemy)
    {
        return new BattleDirectionState(
            new DetachmentState(
                player.Personnel,
                player.Health,
                player.Attack,
                player.Defense),

            new DetachmentState(
                enemy.Personnel,
                enemy.Health,
                enemy.Attack,
                enemy.Defense));
    }
}