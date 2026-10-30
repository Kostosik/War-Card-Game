using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;

namespace GameProject.Combat;

public sealed class CombatState
{
    public CombatantState Player { get; }

    public CombatantState Enemy { get; }

    public CombatResult Result
    {
        get
        {
            if (Enemy.IsDefeated)
            {
                return CombatResult.PlayerVictory;
            }

            if (Player.IsDefeated)
            {
                return CombatResult.PlayerDefeat;
            }

            return CombatResult.InProgress;
        }
    }

    public bool IsFinished =>
        Result != CombatResult.InProgress;

    public CombatState(
        CombatantState player,
        CombatantState enemy)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(enemy);

        Player = player;
        Enemy = enemy;
    }

    public EnemyIntent? CurrentEnemyIntent { get; private set; }

    public void SetEnemyIntent(EnemyIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);

        if (IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot change enemy intent after combat has finished.");
        }

        CurrentEnemyIntent = intent;
    }
}