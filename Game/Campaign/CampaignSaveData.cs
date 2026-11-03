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
}

internal sealed class OperationSaveData
{
    public string DefinitionId { get; set; } = string.Empty;

    public List<string> SelectedCardIds { get; set; } = [];

    public List<OperationBattleResult> BattleResults { get; set; } = [];

    public bool HasFailedAdditionalObjective { get; set; }

    public OperationOutcome? Outcome { get; set; }
}