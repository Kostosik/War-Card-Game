
using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
    public static BattlefieldState CreatePrototype(
        ArmyDeploymentState deployment,
        EnemyBattlefieldDefinition enemyBattlefield,
        BattleObjectiveDefinition? objective = null, BattleTerrainDefinition? leftTerrain = null,
BattleTerrainDefinition? centerTerrain = null,
BattleTerrainDefinition? rightTerrain = null)
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
                enemyBattlefield.EnemyIncomingDamageReductionPercent,leftTerrain),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.Center),
                enemyBattlefield.Center,
                deployment.GetAssignedOrder(BattleDirection.Center),
                enemyBattlefield.EnemyIncomingDamageReductionPercent,centerTerrain),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.RightFlank),
                enemyBattlefield.RightFlank,
                deployment.GetAssignedOrder(BattleDirection.RightFlank),
                enemyBattlefield.EnemyIncomingDamageReductionPercent,rightTerrain),
            objective);
    }

    private static BattleDirectionState CreateDirection(
        DetachmentState playerDetachment,
        DetachmentDefinition enemy,
        DetachmentOrderDefinition playerOrder,
        int enemyIncomingDamageReductionPercent, BattleTerrainDefinition? terrain)
    {
        playerDetachment.AssignOrder(playerOrder);

        return new BattleDirectionState(
            playerDetachment,
            new DetachmentState(enemy),
            enemyIncomingDamageReductionPercent,
            terrain);
    }
}
