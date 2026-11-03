using GameProject.Combat.Battlefield;
using GameProject.Game.Operations;

namespace GameProject.Game.Campaign;

internal sealed class CampaignSaveData
{
    public const int CurrentVersion = 1;
    public ulong? CampaignSeed { get; set; }
    public int Version { get; set; } = CurrentVersion;

    public int RegularOperationCount { get; set; }

    public CampaignStage Stage { get; set; }

    public WarPositionLevel WarPosition { get; set; }

    public List<OperationSaveData> CompletedOperations { get; set; } = [];

    public List<string> MissedOpportunityIds { get; set; } = [];

    public OperationSaveData? CurrentOperation { get; set; }

    public OperationSaveData? FinalOperationResult { get; set; }
    public CampaignProgressionSaveData? Progression { get; set; }
}

internal sealed class OperationSaveData
{
    public OperationPreparationSaveData? Preparation { get; set; }
    public List<OperationBattleSaveData> Battles { get; set; } = [];
    public string DefinitionId { get; set; } = string.Empty;

    public List<string> SelectedCardIds { get; set; } = [];

    public List<OperationBattleResult> BattleResults { get; set; } = [];

    public bool HasFailedAdditionalObjective { get; set; }

    public OperationOutcome? Outcome { get; set; }
}

internal sealed class OperationBattleSaveData
{
    public string Name { get; set; } = string.Empty;

    public string Briefing { get; set; } = string.Empty;

    public string LeftEnemyDetachmentId { get; set; } = string.Empty;

    public string CenterEnemyDetachmentId { get; set; } = string.Empty;

    public string RightEnemyDetachmentId { get; set; } = string.Empty;

    public int EnemyIncomingDamageReductionPercent { get; set; }

    public string ObjectiveId { get; set; } = string.Empty;

    public string LeftTerrainId { get; set; } = string.Empty;

    public string CenterTerrainId { get; set; } = string.Empty;

    public string RightTerrainId { get; set; } = string.Empty;

    public string FormationId { get; set; } = string.Empty;
}

internal sealed class OperationPreparationSaveData
{
    public List<string> PurchasedIds { get; set; } = [];

    public BattleDirection? TerrainDirection { get; set; }

    public string? TerrainId { get; set; }
}