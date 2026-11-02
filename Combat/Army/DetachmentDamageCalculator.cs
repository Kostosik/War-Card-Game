namespace GameProject.Combat.Army;

public static class DetachmentDamageCalculator
{
    public static int Calculate(DetachmentState attacker)
    {
        ArgumentNullException.ThrowIfNull(attacker);

        return attacker.IsCombatReady
            ? attacker.Attack
            : 0;
    }
}