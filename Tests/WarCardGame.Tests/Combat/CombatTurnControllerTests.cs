using GameProject.Combat;
using GameProject.Combat.Cards;
using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;

namespace WarCardGame.Tests.Combat;

public sealed class CombatTurnControllerTests
{
    [Fact]
    public void Combat_ResolvesCardsAndEnemyTurns()
    {
        var cards =
            Enumerable.Range(0, 5)
                .Select(i => new CardDefinition(
                    $"defense_{i}",
                    CardKind.Defense,
                    1,
                    5))
                .Concat(
                    Enumerable.Range(0, 5)
                        .Select(i => new CardDefinition(
                            $"attack_{i}",
                            CardKind.Attack,
                            1,
                            6)))
                .ToArray();

        var deck = new CardDeck(cards, new Random(123));

        var combat = new CombatState(
            new CombatantState(60),
            new CombatantState(42));

        var turn = new CombatTurnState(3);

        var controller = new CombatTurnController(
            combat,
            deck,
            turn,
            new EnemyIntent(EnemyIntentKind.Attack, 12));

        controller.StartCombat();

        Assert.Equal(CombatPhase.PlayerTurn, controller.Phase);
        Assert.Equal(5, deck.Hand.Count);
        Assert.Equal(3, turn.AvailableActionPoints);

        // Первый ход: атака.

        Assert.True(controller.TryPlayCard(0));

        Assert.Equal(36, combat.Enemy.Health);
        Assert.Equal(2, turn.AvailableActionPoints);

        controller.EndPlayerTurn();

        Assert.Equal(48, combat.Player.Health);
        Assert.Equal(2, turn.TurnNumber);
        Assert.Equal(5, deck.Hand.Count);

        // Второй ход: две карты защиты.

        Assert.True(controller.TryPlayCard(0));
        Assert.True(controller.TryPlayCard(0));

        Assert.Equal(10, turn.Block);

        controller.EndPlayerTurn();

        Assert.Equal(46, combat.Player.Health);
        Assert.Equal(3, turn.TurnNumber);
        Assert.Equal(0, turn.Block);
        Assert.Equal(CombatPhase.PlayerTurn, controller.Phase);
    }
}