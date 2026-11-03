using GameProject.Combat.Army;
using GameProject.Combat.Modifiers;

namespace GameProject.Combat.Battlefield;

public sealed class BattleTerrainDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    // Старые свойства сохраняются для совместимости.
    public int DefenseBonusPercent { get; }
    public int ArtilleryStrikeBonusPercent { get; }
    public int SmokeScreenBonusPercent { get; }
    public int SupplyBonusPercent { get; }
    public int MechanizedAttackPenaltyPercent { get; }

    public IReadOnlyList<CombatModifierDefinition> Modifiers { get; }

    public BattleTerrainDefinition(
        string id,
        string name,
        string description,
        int defenseBonusPercent = 0,
        int artilleryStrikeBonusPercent = 0,
        int smokeScreenBonusPercent = 0,
        int supplyBonusPercent = 0,
        int mechanizedAttackPenaltyPercent = 0,
        IEnumerable<CombatModifierDefinition>? modifiers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        ValidatePercent(defenseBonusPercent);
        ValidatePercent(artilleryStrikeBonusPercent);
        ValidatePercent(smokeScreenBonusPercent);
        ValidatePercent(supplyBonusPercent);
        ValidatePercent(mechanizedAttackPenaltyPercent);

        Id = id;
        Name = name;
        Description = description;

        DefenseBonusPercent = defenseBonusPercent;
        ArtilleryStrikeBonusPercent = artilleryStrikeBonusPercent;
        SmokeScreenBonusPercent = smokeScreenBonusPercent;
        SupplyBonusPercent = supplyBonusPercent;
        MechanizedAttackPenaltyPercent =
            mechanizedAttackPenaltyPercent;

        var result = new List<CombatModifierDefinition>();

        if (defenseBonusPercent > 0)
        {
            result.Add(new(
                $"{id}-defense",
                CombatModifierStats.IncomingDamage,
                CombatModifierOperation.MultiplyPercent,
                100 - defenseBonusPercent,
                priority: 100));
        }

        if (artilleryStrikeBonusPercent > 0)
        {
            result.Add(new(
                $"{id}-artillery",
                CombatModifierStats.ArtilleryDamage,
                CombatModifierOperation.MultiplyPercent,
                100 + artilleryStrikeBonusPercent,
                priority: 100));
        }

        if (smokeScreenBonusPercent > 0)
        {
            result.Add(new(
                $"{id}-smoke",
                CombatModifierStats.SmokeReduction,
                CombatModifierOperation.MultiplyPercent,
                100 + smokeScreenBonusPercent,
                priority: 100));
        }

        if (supplyBonusPercent > 0)
        {
            result.Add(new(
                $"{id}-supply",
                CombatModifierStats.SupplyHealing,
                CombatModifierOperation.MultiplyPercent,
                100 + supplyBonusPercent,
                priority: 100));
        }

        if (mechanizedAttackPenaltyPercent > 0)
        {
            result.Add(new(
                $"{id}-mechanized",
                CombatModifierStats.OutgoingDamage,
                CombatModifierOperation.MultiplyPercent,
                100 - mechanizedAttackPenaltyPercent,
                requiredSubjectTags:
                [
                    DetachmentTags.Mechanized
                ],
                priority: 100));
        }

        if (modifiers is not null)
        {
            result.AddRange(modifiers);
        }

        if (result.Any(modifier => modifier is null))
            throw new ArgumentException("Terrain modifiers cannot contain null.");

        Modifiers = Array.AsReadOnly(result.ToArray());
    }

    private static void ValidatePercent(int value)
    {
        if (value is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}