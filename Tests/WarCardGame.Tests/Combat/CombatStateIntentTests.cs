using GameProject.Combat;
using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;

namespace WarCardGame.Tests.Combat;

public sealed class CombatStateIntentTests
{
    [Fact]
    public void SetEnemyIntent_StoresCurrentIntent()
    {
        var combat = CreateCombat();

        var intent = new EnemyIntent(
            EnemyIntentKind.Attack,
            12);

        combat.SetEnemyIntent(intent);

        Assert.Same(intent, combat.CurrentEnemyIntent);
    }

    [Fact]
    public void SetEnemyIntent_WithNull_Throws()
    {
        var combat = CreateCombat();

        Assert.Throws<ArgumentNullException>(
            () => combat.SetEnemyIntent(null!));
    }

    [Fact]
    public void SetEnemyIntent_WhenCombatFinished_Throws()
    {
        var combat = CreateCombat();

        combat.Enemy.ApplyDamage(42);

        Assert.Throws<InvalidOperationException>(
            () => combat.SetEnemyIntent(
                new EnemyIntent(
                    EnemyIntentKind.Attack,
                    12)));
    }

    private static CombatState CreateCombat()
    {
        return new CombatState(
            new CombatantState(60),
            new CombatantState(42));
    }
}