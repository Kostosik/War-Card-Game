using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public static class BattlefieldFactory
{
    public static BattlefieldState Create(
        ArmyDeploymentState deployment,
        BattleDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(deployment);
        ArgumentNullException.ThrowIfNull(definition);

        if (!deployment.IsReadyForBattle)
        {
            throw new InvalidOperationException(
                "Army deployment is incomplete.");
        }

        var enemy = definition.EnemyForces;

        return new BattlefieldState(
            CreateDirection(
                deployment.GetAssignedState(BattleDirection.LeftFlank),
                enemy.LeftFlank,
                deployment.GetAssignedOrder(BattleDirection.LeftFlank),
                enemy.EnemyIncomingDamageReductionPercent,
                definition.LeftTerrain),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.Center),
                enemy.Center,
                deployment.GetAssignedOrder(BattleDirection.Center),
                enemy.EnemyIncomingDamageReductionPercent,
                definition.CenterTerrain),

            CreateDirection(
                deployment.GetAssignedState(BattleDirection.RightFlank),
                enemy.RightFlank,
                deployment.GetAssignedOrder(BattleDirection.RightFlank),
                enemy.EnemyIncomingDamageReductionPercent,
                definition.RightTerrain),

            definition.Objective,
            definition.Formation);
    }

    // Старый метод оставляем для совместимости.
    public static BattlefieldState CreatePrototype(
        ArmyDeploymentState deployment,
        EnemyBattlefieldDefinition enemyBattlefield,
        BattleObjectiveDefinition? objective = null,
        BattleTerrainDefinition? leftTerrain = null,
        BattleTerrainDefinition? centerTerrain = null,
        BattleTerrainDefinition? rightTerrain = null,
        BattleFormationDefinition? formation = null)
    {
        return Create(
            deployment,
            new BattleDefinition(
                enemyBattlefield,
                objective,
                leftTerrain,
                centerTerrain,
                rightTerrain,
                formation));
    }

    private static BattleDirectionState CreateDirection(
        DetachmentState playerDetachment,
        DetachmentDefinition enemy,
        DetachmentOrderDefinition playerOrder,
        int enemyIncomingDamageReductionPercent,
        BattleTerrainDefinition terrain)
    {
        playerDetachment.AssignOrder(playerOrder);

        return new BattleDirectionState(
            playerDetachment,
            new DetachmentState(enemy),
            enemyIncomingDamageReductionPercent,
            terrain);
    }
}