
using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
    private static readonly DetachmentDefinition EnemyInfantry =
        new("enemy-infantry", "Enemy Infantry", 100, 20);

    private static readonly DetachmentDefinition EnemyMechanized =
        new("enemy-mechanized", "Enemy Mechanized", 110, 22);

    private static readonly DetachmentDefinition EnemyArtillery =
        new("enemy-artillery", "Enemy Artillery", 100, 18);

    public static BattlefieldState CreatePrototype(
        ArmyDeploymentState deployment,
        EnemyBattlefieldDefinition enemyBattlefield)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        ArgumentNullException.ThrowIfNull(enemyBattlefield);

        if (!deployment.IsReadyForBattle)
        {
            throw new InvalidOperationException(
                "Army deployment is incomplete.");
        }

        return new BattlefieldState(
            CreateDirection(
                GetAssigned(deployment, BattleDirection.LeftFlank),
                enemyBattlefield.LeftFlank,
                deployment.GetAssignedOrder(BattleDirection.LeftFlank), enemyBattlefield.EnemyIncomingDamageReductionPercent),

            CreateDirection(
                GetAssigned(deployment, BattleDirection.Center),
                enemyBattlefield.Center,
                deployment.GetAssignedOrder(BattleDirection.Center), enemyBattlefield.EnemyIncomingDamageReductionPercent),

            CreateDirection(
                GetAssigned(deployment, BattleDirection.RightFlank),
                enemyBattlefield.RightFlank,
                deployment.GetAssignedOrder(BattleDirection.RightFlank), enemyBattlefield.EnemyIncomingDamageReductionPercent));
    }

    private static DetachmentDefinition GetAssigned(
        ArmyDeploymentState deployment,
        BattleDirection direction)
    {
        return deployment.GetAssigned(direction)
            ?? throw new InvalidOperationException(
                $"No detachment assigned to {direction}.");
    }

    private static BattleDirectionState CreateDirection(
        DetachmentDefinition player,
        DetachmentDefinition enemy,
        DetachmentOrderDefinition playerOrder, int enemyIncomingDamageReductionPercent)
    {
        var playerDetachment = new DetachmentState(player);
        playerDetachment.AssignOrder(playerOrder);

        return new BattleDirectionState(
            playerDetachment,
            new DetachmentState(enemy),
            enemyIncomingDamageReductionPercent);
    }
}
