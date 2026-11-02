
using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
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
                deployment.GetAssignedState(BattleDirection.LeftFlank),
                enemyBattlefield.LeftFlank,
                deployment.GetAssignedOrder(BattleDirection.LeftFlank),
                enemyBattlefield.EnemyIncomingDamageReductionPercent),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.Center),
                enemyBattlefield.Center,
                deployment.GetAssignedOrder(BattleDirection.Center),
                enemyBattlefield.EnemyIncomingDamageReductionPercent),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.RightFlank),
                enemyBattlefield.RightFlank,
                deployment.GetAssignedOrder(BattleDirection.RightFlank),
                enemyBattlefield.EnemyIncomingDamageReductionPercent));
    }

    private static BattleDirectionState CreateDirection(
        DetachmentState playerDetachment,
        DetachmentDefinition enemy,
        DetachmentOrderDefinition playerOrder,
        int enemyIncomingDamageReductionPercent)
    {
        playerDetachment.AssignOrder(playerOrder);

        return new BattleDirectionState(
            playerDetachment,
            new DetachmentState(enemy),
            enemyIncomingDamageReductionPercent);
    }
}
