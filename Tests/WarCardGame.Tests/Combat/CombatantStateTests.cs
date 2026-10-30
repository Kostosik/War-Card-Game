using GameProject.Combat;
using GameProject.Combat.Combatants;

namespace WarCardGame.Tests.Combat;

public sealed class CombatantStateTests
{
    [Fact]
    public void Constructor_InitializesHealth()
    {
        var combatant = new CombatantState(60);

        Assert.Equal(60, combatant.MaxHealth);
        Assert.Equal(60, combatant.Health);
        Assert.False(combatant.IsDefeated);
    }

    [Fact]
    public void ApplyDamage_ReducesHealth()
    {
        var combatant = new CombatantState(60);

        var appliedDamage = combatant.ApplyDamage(12);

        Assert.Equal(12, appliedDamage);
        Assert.Equal(48, combatant.Health);
    }

    [Fact]
    public void ApplyDamage_WhenDamageExceedsHealth_ClampsToZero()
    {
        var combatant = new CombatantState(10);

        var appliedDamage = combatant.ApplyDamage(20);

        Assert.Equal(10, appliedDamage);
        Assert.Equal(0, combatant.Health);
        Assert.True(combatant.IsDefeated);
    }

    [Fact]
    public void ApplyDamage_WhenAlreadyDefeated_DoesNothing()
    {
        var combatant = new CombatantState(10);

        combatant.ApplyDamage(10);

        var appliedDamage = combatant.ApplyDamage(5);

        Assert.Equal(0, appliedDamage);
        Assert.Equal(0, combatant.Health);
    }

    [Fact]
    public void Constructor_StoresCombatants()
    {
        var player = new CombatantState(60);
        var enemy = new CombatantState(42);

        var combat = new CombatState(player, enemy);

        Assert.Same(player, combat.Player);
        Assert.Same(enemy, combat.Enemy);
    }

    [Fact]
    public void Result_WhenBothCombatantsAlive_IsInProgress()
    {
        var combat = CreateCombat();

        Assert.Equal(
            CombatResult.InProgress,
            combat.Result);

        Assert.False(combat.IsFinished);
    }

    [Fact]
    public void Result_WhenEnemyDefeated_IsPlayerVictory()
    {
        var combat = CreateCombat();

        combat.Enemy.ApplyDamage(42);

        Assert.Equal(
            CombatResult.PlayerVictory,
            combat.Result);

        Assert.True(combat.IsFinished);
    }

    [Fact]
    public void Result_WhenPlayerDefeated_IsPlayerDefeat()
    {
        var combat = CreateCombat();

        combat.Player.ApplyDamage(60);

        Assert.Equal(
            CombatResult.PlayerDefeat,
            combat.Result);

        Assert.True(combat.IsFinished);
    }

    [Fact]
    public void Constructor_WithNullPlayer_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CombatState(
                null!,
                new CombatantState(42)));
    }

    [Fact]
    public void Constructor_WithNullEnemy_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CombatState(
                new CombatantState(60),
                null!));
    }

    private static CombatState CreateCombat()
    {
        return new CombatState(
            new CombatantState(60),
            new CombatantState(42));
    }

    [Fact]
    public void ApplyDamage_WithZeroDamage_DoesNothing()
    {
        var combatant = new CombatantState(60);

        var appliedDamage = combatant.ApplyDamage(0);

        Assert.Equal(0, appliedDamage);
        Assert.Equal(60, combatant.Health);
    }

    [Fact]
    public void ApplyDamage_WithNegativeDamage_Throws()
    {
        var combatant = new CombatantState(60);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => combatant.ApplyDamage(-1));
    }

    [Fact]
    public void Constructor_WithInvalidHealth_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CombatantState(0));
    }
}