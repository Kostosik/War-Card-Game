using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;

namespace WarCardGame.Tests.Combat.Battlefield;

public sealed class BattleDirectionStateTests
{
    [Fact]
    public void Constructor_StoresBothDetachments()
    {
        var player = CreateDetachment();
        var enemy = CreateDetachment();

        var direction = new BattleDirectionState(player, enemy);

        Assert.Same(player, direction.PlayerDetachment);
        Assert.Same(enemy, direction.EnemyDetachment);
        Assert.False(direction.IsFinished);
    }

    [Fact]
    public void ApplySimultaneousDamage_DamagesBothDetachments()
    {
        var player = CreateDetachment(100);
        var enemy = CreateDetachment(100);

        var direction = new BattleDirectionState(player, enemy);

        direction.ApplySimultaneousDamage(30, 40);

        Assert.Equal(60, player.Health);
        Assert.Equal(70, enemy.Health);
    }

    [Fact]
    public void ApplySimultaneousDamage_WhenBothReachZero_DamagesBoth()
    {
        var player = CreateDetachment(30);
        var enemy = CreateDetachment(40);

        var direction = new BattleDirectionState(player, enemy);

        direction.ApplySimultaneousDamage(40, 30);

        Assert.Equal(0, player.Health);
        Assert.Equal(0, enemy.Health);
        Assert.True(direction.IsFinished);
    }

    [Fact]
    public void ApplySimultaneousDamage_WhenDirectionFinished_Throws()
    {
        var player = CreateDetachment(30);
        var enemy = CreateDetachment(40);

        var direction = new BattleDirectionState(player, enemy);

        direction.ApplySimultaneousDamage(40, 30);

        Assert.Throws<InvalidOperationException>(
            () => direction.ApplySimultaneousDamage(10, 10));
    }

    [Fact]
    public void ResolveStep_CalculatesDamageUsingAttackAndDefense()
    {
        var player = new DetachmentState(100, 100, 30, 10);
        var enemy = new DetachmentState(100, 100, 20, 5);

        var direction = new BattleDirectionState(player, enemy);

        direction.ResolveStep();

        Assert.Equal(90, player.Health);
        Assert.Equal(75, enemy.Health);
    }

    [Fact]
    public void ResolveStep_WhenAttackDoesNotExceedDefense_DealsNoDamage()
    {
        var player = new DetachmentState(100, 100, 10, 20);
        var enemy = new DetachmentState(100, 100, 15, 10);

        var direction = new BattleDirectionState(player, enemy);

        direction.ResolveStep();

        Assert.Equal(100, player.Health);
        Assert.Equal(100, enemy.Health);
    }

    [Fact]
    public void ResolveStep_AppliesBothAttacksBeforeCheckingResult()
    {
        var player = new DetachmentState(100, 20, 30, 0);
        var enemy = new DetachmentState(100, 30, 20, 0);

        var direction = new BattleDirectionState(player, enemy);

        direction.ResolveStep();

        Assert.False(player.IsCombatReady);
        Assert.False(enemy.IsCombatReady);
        Assert.True(direction.IsFinished);
    }

    [Fact]
    public void ApplySimultaneousDamage_WithNegativeDamage_Throws()
    {
        var player = CreateDetachment();
        var enemy = CreateDetachment();

        var direction = new BattleDirectionState(player, enemy);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => direction.ApplySimultaneousDamage(-1, 10));

        Assert.Equal(100, player.Health);
        Assert.Equal(100, enemy.Health);
    }

    private static DetachmentState CreateDetachment(int health = 100)
    {
        return new DetachmentState(100, health, 20, 10);
    }
}