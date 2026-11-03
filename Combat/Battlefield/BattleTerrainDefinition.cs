namespace GameProject.Combat.Battlefield;

public sealed class BattleTerrainDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public int DefenseBonusPercent { get; }
    public int ArtilleryStrikeBonusPercent { get; }
    public int SmokeScreenBonusPercent { get; }
    public int SupplyBonusPercent { get; }
    public int MechanizedAttackPenaltyPercent { get; }

    public BattleTerrainDefinition(
        string id,
        string name,
        string description,
        int defenseBonusPercent = 0,
        int artilleryStrikeBonusPercent = 0,
        int smokeScreenBonusPercent = 0,
        int supplyBonusPercent = 0,
        int mechanizedAttackPenaltyPercent = 0)
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
    }

    private static void ValidatePercent(int value)
    {
        if (value is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}