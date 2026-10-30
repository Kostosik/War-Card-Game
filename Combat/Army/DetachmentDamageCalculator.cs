namespace GameProject.Combat.Army;

public static class DetachmentDamageCalculator
{
    public static int Calculate(
        DetachmentState attacker,
        DetachmentState defender)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        ArgumentNullException.ThrowIfNull(defender);

        if (!attacker.IsCombatReady)
        {
            return 0;
        }

        return Math.Max(
            0,
            attacker.Attack - defender.Defense);
    }
}