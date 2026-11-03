using Engine.Core.Determinism;
using GameProject.Combat.Army;
using GameProject.Combat.Battlefield;

namespace GameProject.Game.Generation;

internal sealed class BattleGenerator
{
    private DeterministicRandom _random;

    public BattleGenerator(DeterministicRandom random)
    {
        _random = random;
    }

    public BattleDefinition Generate(
        BattleGenerationProfile? profile = null)
    {
        profile ??= BattleGenerationProfile.Standard;

        var enemyDetachments = PickDistinct(
            profile.EnemyDetachments,
            3);

        var leftTerrain = Pick(profile.Terrains);
        var centerTerrain = Pick(profile.Terrains);
        var rightTerrain = Pick(profile.Terrains);

        var objective = Pick(profile.Objectives);
        var formation = Pick(profile.Formations);

        var enemyForces = new EnemyBattlefieldDefinition(
            enemyDetachments[0],
            enemyDetachments[1],
            enemyDetachments[2],
            profile.EnemyIncomingDamageReductionPercent);

        return new BattleDefinition(
            enemyForces,
            objective,
            leftTerrain,
            centerTerrain,
            rightTerrain,
            formation);
    }

    private T Pick<T>(IReadOnlyList<T> candidates)
    {
        var index = _random.NextInt(0, candidates.Count);
        return candidates[index];
    }

    private T[] PickDistinct<T>(
        IReadOnlyList<T> candidates,
        int count)
    {
        var pool = candidates.ToArray();

        // Частичное перемешивание Фишера — Йетса.
        // Выбираем count элементов без повторений.
        for (var i = 0; i < count; i++)
        {
            var selectedIndex = _random.NextInt(i, pool.Length);

            (pool[i], pool[selectedIndex]) =
                (pool[selectedIndex], pool[i]);
        }

        var result = new T[count];
        Array.Copy(pool, result, count);

        return result;
    }
}