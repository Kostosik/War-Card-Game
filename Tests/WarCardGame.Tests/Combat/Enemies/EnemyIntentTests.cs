using GameProject.Combat.Enemies;

namespace WarCardGame.Tests.Combat.Enemies;

public sealed class EnemyIntentTests
{
    [Fact]
    public void Constructor_StoresIntent()
    {
        var intent = new EnemyIntent(
            EnemyIntentKind.Attack,
            12);

        Assert.Equal(EnemyIntentKind.Attack, intent.Kind);
        Assert.Equal(12, intent.Amount);
    }

    [Fact]
    public void Constructor_WithZeroAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EnemyIntent(
                EnemyIntentKind.Attack,
                0));
    }

    [Fact]
    public void Constructor_WithNegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EnemyIntent(
                EnemyIntentKind.Attack,
                -1));
    }

    [Fact]
    public void Constructor_WithInvalidKind_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new EnemyIntent(
                (EnemyIntentKind)999,
                12));
    }
}