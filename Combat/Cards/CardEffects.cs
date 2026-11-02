using GameProject.Combat.Battlefield;

namespace GameProject.Combat.Cards;

internal static class CardEffects
{
    public static void ArtilleryStrike(
        BattlefieldState battlefield,
        BattleDirection direction,
        int value)
    {
        battlefield.QueueArtilleryStrike(direction, value);
    }

    public static void SmokeScreen(
        BattlefieldState battlefield,
        BattleDirection direction,
        int value)
    {
        battlefield.QueueSmokeScreen(direction, value);
    }

    public static void Supply(
        BattlefieldState battlefield,
        BattleDirection direction,
        int value)
    {
        battlefield.QueueSupply(direction, value);
    }
}