using GameProject.Combat.Army;

namespace WarCardGame.Tests.Combat.Army;

public sealed class DetachmentStateTests
{
    [Fact]
    public void Constructor_InitializesState()
    {
        var detachment = new DetachmentState(
            100, 150, 20, 10);

        Assert.Equal(100, detachment.PersonnelCount);
        Assert.Equal(150, detachment.MaxHealth);
        Assert.Equal(150, detachment.Health);
        Assert.Equal(20, detachment.Attack);
        Assert.Equal(10, detachment.Defense);
        Assert.True(detachment.IsCombatReady);
    }

    [Fact]
    public void ApplyDamage_ReducesHealth()
    {
        var detachment = new DetachmentState(
            100, 150, 20, 10);

        var applied = detachment.ApplyDamage(40);

        Assert.Equal(40, applied);
        Assert.Equal(110, detachment.Health);
    }

    [Fact]
    public void ApplyDamage_WhenHealthReachesZero_DisablesDetachment()
    {
        var detachment = new DetachmentState(
            100, 50, 20, 10);

        detachment.ApplyDamage(50);

        Assert.Equal(0, detachment.Health);
        Assert.False(detachment.IsCombatReady);
    }

    [Fact]
    public void ApplyDamage_WhenDamageExceedsHealth_ClampsToZero()
    {
        var detachment = new DetachmentState(
            100, 50, 20, 10);

        var applied = detachment.ApplyDamage(80);

        Assert.Equal(50, applied);
        Assert.Equal(0, detachment.Health);
    }

    [Fact]
    public void ApplyDamage_WhenIncapacitated_DoesNothing()
    {
        var detachment = new DetachmentState(
            100, 50, 20, 10);

        detachment.ApplyDamage(50);

        var applied = detachment.ApplyDamage(10);

        Assert.Equal(0, applied);
        Assert.Equal(0, detachment.Health);
    }
}