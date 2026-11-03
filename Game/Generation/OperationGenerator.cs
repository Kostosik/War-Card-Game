using Engine.Core.Determinism;
using GameProject.Game.Operations;

namespace GameProject.Game.Generation;

internal sealed class OperationGenerator
{
    public OperationDefinition Generate(
        OperationDefinition template,
        ulong campaignSeed,
        int battleCount)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (battleCount <= 0 || battleCount > template.BattleCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(battleCount),
                $"Battle count must be between 1 and {template.BattleCount}.");
        }

        var battles = new List<OperationBattleDefinition>(battleCount);

        for (var i = 0; i < battleCount; i++)
        {
            var battleNumber = i + 1;
            var templateBattle = template.Battles[i];

            var battleSeed = GameRandomSeeds.ForBattleGeneration(
                campaignSeed,
                template.Id,
                battleNumber);

            var battleGenerator = new BattleGenerator(
                new DeterministicRandom(battleSeed));

            var battleDefinition = battleGenerator.Generate();

            battles.Add(
                new OperationBattleDefinition(
                    templateBattle.Name,
                    templateBattle.Briefing,
                    battleDefinition));
        }

        return new OperationDefinition(
            template.Id,
            template.Name,
            template.MainObjective,
            template.AdditionalObjective,
            template.EnemyBriefing,
            template.BattlePlan,
            battles,
            template.AdditionalObjectiveDetachmentIds);
    }
}