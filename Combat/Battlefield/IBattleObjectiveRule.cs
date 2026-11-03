namespace GameProject.Combat.Battlefield;

public interface IBattleObjectiveRule
{
    BattlefieldResult Evaluate(BattlefieldSnapshot battlefield);

    string BuildStatus(BattlefieldSnapshot battlefield);
}