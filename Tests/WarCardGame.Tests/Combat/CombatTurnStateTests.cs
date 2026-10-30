using GameProject.Combat;

namespace WarCardGame.Tests.Combat;

public sealed class CombatTurnStateTests
{
    [Fact]
    public void Constructor_InitializesState()
    {
        var turn = new CombatTurnState(3);

        Assert.Equal(0, turn.TurnNumber);
        Assert.Equal(3, turn.MaxActionPoints);
        Assert.Equal(0, turn.AvailableActionPoints);
        Assert.Equal(0, turn.Block);
    }

    [Fact]
    public void BeginTurn_RestoresActionPoints()
    {
        var turn = new CombatTurnState(3);

        turn.BeginTurn();

        Assert.Equal(1, turn.TurnNumber);
        Assert.Equal(3, turn.AvailableActionPoints);
    }

    [Fact]
    public void TrySpendActionPoints_ReducesAvailablePoints()
    {
        var turn = new CombatTurnState(3);

        turn.BeginTurn();

        var result = turn.TrySpendActionPoints(2);

        Assert.True(result);
        Assert.Equal(1, turn.AvailableActionPoints);
    }

    [Fact]
    public void TrySpendActionPoints_WhenInsufficient_ReturnsFalse()
    {
        var turn = new CombatTurnState(3);

        turn.BeginTurn();

        var result = turn.TrySpendActionPoints(4);

        Assert.False(result);
        Assert.Equal(3, turn.AvailableActionPoints);
    }

    [Fact]
    public void AddBlock_AccumulatesProtection()
    {
        var turn = new CombatTurnState(3);

        turn.AddBlock(5);
        turn.AddBlock(5);

        Assert.Equal(10, turn.Block);
    }

    [Fact]
    public void AbsorbDamage_ReducesIncomingDamage()
    {
        var turn = new CombatTurnState(3);

        turn.AddBlock(10);

        var remainingDamage = turn.AbsorbDamage(12);

        Assert.Equal(2, remainingDamage);
        Assert.Equal(0, turn.Block);
    }

    [Fact]
    public void AbsorbDamage_WhenBlockExceedsDamage_PreservesRemainingBlock()
    {
        var turn = new CombatTurnState(3);

        turn.AddBlock(10);

        var remainingDamage = turn.AbsorbDamage(4);

        Assert.Equal(0, remainingDamage);
        Assert.Equal(6, turn.Block);
    }

    [Fact]
    public void BeginTurn_ResetsBlockAndRestoresActionPoints()
    {
        var turn = new CombatTurnState(3);

        turn.BeginTurn();

        turn.TrySpendActionPoints(2);
        turn.AddBlock(10);

        turn.BeginTurn();

        Assert.Equal(2, turn.TurnNumber);
        Assert.Equal(3, turn.AvailableActionPoints);
        Assert.Equal(0, turn.Block);
    }

    [Fact]
    public void TrySpendActionPoints_WithNegativeAmount_Throws()
    {
        var turn = new CombatTurnState(3);

        turn.BeginTurn();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => turn.TrySpendActionPoints(-1));
    }

    [Fact]
    public void AddBlock_WithNegativeAmount_Throws()
    {
        var turn = new CombatTurnState(3);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => turn.AddBlock(-1));
    }

    [Fact]
    public void Constructor_WithInvalidActionPoints_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CombatTurnState(0));
    }
}