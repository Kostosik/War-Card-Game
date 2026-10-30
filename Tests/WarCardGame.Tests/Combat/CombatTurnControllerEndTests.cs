using GameProject.Combat;
using GameProject.Combat.Cards;
using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;

namespace WarCardGame.Tests.Combat;

public sealed class CombatTurnControllerEndTests
{
    [Fact]
    public void EnemyDefeated_DoesNotAttack_PlayerWins()
    {
        var deck = new CardDeck(
        [
            new CardDefinition(
                "finisher",
                CardKind.Attack,
                1,
                6)
        ]);

        var combat = new CombatState(
            new CombatantState(60),
            new CombatantState(6));

        var controller = new CombatTurnController(
            combat,
            deck,
            new CombatTurnState(3),
            new EnemyIntent(EnemyIntentKind.Attack, 12));

        controller.StartCombat();

        Assert.True(controller.TryPlayCard(0));

        Assert.Equal(CombatResult.PlayerVictory, combat.Result);
        Assert.Equal(CombatPhase.Finished, controller.Phase);
        Assert.Equal(60, combat.Player.Health);
        Assert.False(controller.TryPlayCard(0));
    }

    [Fact]
    public void PlayerDefeated_DoesNotStartNextTurn()
    {
        var combat = new CombatState(
            new CombatantState(10),
            new CombatantState(42));

        var controller = new CombatTurnController(
            combat,
            new CardDeck(Array.Empty<CardDefinition>()),
            new CombatTurnState(3),
            new EnemyIntent(EnemyIntentKind.Attack, 12));

        controller.StartCombat();
        controller.EndPlayerTurn();

        Assert.Equal(CombatResult.PlayerDefeat, combat.Result);
        Assert.Equal(CombatPhase.Finished, controller.Phase);
        Assert.Equal(0, combat.Player.Health);
        Assert.Equal(1, controller.Turn.TurnNumber);
    }
}