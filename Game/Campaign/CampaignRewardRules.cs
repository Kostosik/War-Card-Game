using GameProject.Game.Operations;

namespace GameProject.Game.Campaign;

internal static class CampaignRewardRules
{
    // Начальные значения баланса. Позже сможем настроить.
    public const int VictoryResources = 4;
    public const int DefeatResources = 2;
    public const int AdditionalObjectiveBonus = 1;

    public static int GetResources(OperationState operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var reward = operation.Outcome switch
        {
            OperationOutcome.Victory => VictoryResources,
            OperationOutcome.Defeat => DefeatResources,

            _ => throw new InvalidOperationException(
                "Cannot calculate a reward for an unfinished operation.")
        };

        if (!operation.HasFailedAdditionalObjective)
        {
            reward += AdditionalObjectiveBonus;
        }

        return reward;
    }
}