namespace GameProject.Combat.Army;

public static class DetachmentDamageCalculator
{
    public static int Calculate(DetachmentState attacker)
    {
        ArgumentNullException.ThrowIfNull(attacker);

        if (!attacker.IsCombatReady)
            return 0;

        return attacker.Attack;
    }
}