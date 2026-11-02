namespace GameProject.Game.Campaign;

internal enum CampaignStage
{
    RegularOperations,
    FinalOperationAvailable,
    FinalOperation,
    Completed
}

internal enum WarPositionLevel
{
    CriticalDefeat = -2,
    Retreat = -1,
    Balance = 0,
    Advantage = 1,
    DecisiveAdvantage = 2
}