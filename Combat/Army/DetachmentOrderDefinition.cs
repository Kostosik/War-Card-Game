using GameProject.Combat.Modifiers;

namespace GameProject.Combat.Army;

public sealed class DetachmentOrderDefinition
{
    public string Id { get; }
    public string Name { get; }

    // Старые параметры сохраняем.
    public int AttackBonusPercent { get; }
    public int DamageReductionPercent { get; }
    public int EndOfRoundHealing { get; }

    public IReadOnlyList<CombatModifierDefinition> Modifiers { get; }

    public DetachmentOrderDefinition(
        string id,
        string name,
        int attackBonusPercent,
        int damageReductionPercent,
        int endOfRoundHealing)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ArgumentOutOfRangeException.ThrowIfNegative(attackBonusPercent);
        ArgumentOutOfRangeException.ThrowIfNegative(damageReductionPercent);
        ArgumentOutOfRangeException.ThrowIfNegative(endOfRoundHealing);

        if (attackBonusPercent > 100 || damageReductionPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(attackBonusPercent));

        Id = id;
        Name = name;

        AttackBonusPercent = attackBonusPercent;
        DamageReductionPercent = damageReductionPercent;
        EndOfRoundHealing = endOfRoundHealing;

        var modifiers = new List<CombatModifierDefinition>();

        if (attackBonusPercent > 0)
        {
            modifiers.Add(new(
                $"{id}-attack",
                CombatModifierStats.OutgoingDamage,
                CombatModifierOperation.MultiplyPercent,
                100 + attackBonusPercent,
                CombatSide.Player,
                priority: 100));
        }

        if (damageReductionPercent > 0)
        {
            modifiers.Add(new(
                $"{id}-defense",
                CombatModifierStats.IncomingDamage,
                CombatModifierOperation.MultiplyPercent,
                100 - damageReductionPercent,
                CombatSide.Player,
                priority: 100));
        }

        if (endOfRoundHealing > 0)
        {
            modifiers.Add(new(
                $"{id}-healing",
                CombatModifierStats.EndOfRoundHealing,
                CombatModifierOperation.AddFlat,
                endOfRoundHealing,
                CombatSide.Player));
        }

        Modifiers = Array.AsReadOnly(modifiers.ToArray());
    }

    // Новый конструктор для нестандартных приказов.
    public DetachmentOrderDefinition(
        string id,
        string name,
        IEnumerable<CombatModifierDefinition> modifiers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(modifiers);

        Id = id;
        Name = name;

        AttackBonusPercent = 0;
        DamageReductionPercent = 0;
        EndOfRoundHealing = 0;

        var array = modifiers.ToArray();

        if (array.Any(modifier => modifier is null))
            throw new ArgumentException("Order modifiers cannot contain null.");

        Modifiers = Array.AsReadOnly(array);
    }

    // Старые методы сохраняем.
    public int ModifyOutgoingDamage(int damage)
    {
        return Scale(damage, 100 + AttackBonusPercent);
    }

    public int ModifyIncomingDamage(int damage)
    {
        return Scale(damage, 100 - DamageReductionPercent);
    }

    private static int Scale(int value, int percent)
    {
        return (int)(((long)value * percent + 50) / 100);
    }
}