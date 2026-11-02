using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public sealed class EnemyBattlefieldDefinition
{
    public DetachmentDefinition LeftFlank { get; }
    public DetachmentDefinition Center { get; }
    public DetachmentDefinition RightFlank { get; }
    public int EnemyIncomingDamageReductionPercent { get; }
    public EnemyBattlefieldDefinition(
        DetachmentDefinition leftFlank,
        DetachmentDefinition center,
        DetachmentDefinition rightFlank, int enemyIncomingDamageReductionPercent = 0)
    {
        ArgumentNullException.ThrowIfNull(leftFlank);
        ArgumentNullException.ThrowIfNull(center);
        ArgumentNullException.ThrowIfNull(rightFlank);

        LeftFlank = leftFlank;
        Center = center;
        RightFlank = rightFlank;

        if (enemyIncomingDamageReductionPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(enemyIncomingDamageReductionPercent));
        }

        EnemyIncomingDamageReductionPercent =
            enemyIncomingDamageReductionPercent;
    }
}