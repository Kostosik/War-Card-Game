using Engine.Core.Determinism;
using GameProject.Game.Generation;
using GameProject.Game.Operations;

namespace GameProject.Game.Campaign;

internal sealed class CampaignState
{
    public const int DefaultRegularOperationCount = 3;
    public CampaignProgressionState Progression { get; private set; } = new();
    private readonly List<OperationState> _completedOperations = [];
    private readonly List<string> _missedOpportunityIds = [];
    public OperationState? FinalOperationResult { get; private set; }
    public CampaignEvent? LatestEvent { get; private set; }
    public ulong CampaignSeed { get; }
    public IReadOnlyList<string> MissedOpportunityIds =>
    _missedOpportunityIds;

    public OperationChoiceSet? PendingOperationChoices { get; private set; }
    public CampaignState(
        int regularOperationCount = DefaultRegularOperationCount,
        ulong? campaignSeed = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            regularOperationCount);

        RegularOperationCount = regularOperationCount;

        CampaignSeed = campaignSeed ??
            unchecked((ulong)Random.Shared.NextInt64());
    }

    public int RegularOperationCount { get; }

    public CampaignStage Stage { get; private set; } =
        CampaignStage.RegularOperations;

    public WarPositionLevel WarPosition { get; private set; } =
        WarPositionLevel.Balance;

    public string WarPositionDescription => WarPosition switch
    {
        WarPositionLevel.CriticalDefeat => "Критическое поражение",
        WarPositionLevel.Retreat => "Отступление",
        WarPositionLevel.Balance => "Равновесие",
        WarPositionLevel.Advantage => "Преимущество",
        WarPositionLevel.DecisiveAdvantage => "Решающее преимущество",
        _ => throw new ArgumentOutOfRangeException()
    };

    public int CompletedRegularOperations =>
        _completedOperations.Count;

    public int RemainingRegularOperations =>
        RegularOperationCount - CompletedRegularOperations;

    public bool AreRegularOperationsCompleted =>
        RemainingRegularOperations == 0;

    public OperationState? CurrentOperation { get; private set; }

    public int? CurrentOperationNumber =>
        CurrentOperation is null
            ? null
            : CompletedRegularOperations + 1;

    public int? NextOperationNumber =>
        Stage == CampaignStage.RegularOperations
            ? CompletedRegularOperations + 1
            : null;

    public IReadOnlyList<OperationState> OperationHistory =>
        _completedOperations;

    public OperationState StartNextOperation(
        OperationDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (CurrentOperation is not null)
        {
            throw new InvalidOperationException(
                "An operation is already in progress.");
        }

        if (PendingOperationChoices is not null)
        {
            throw new InvalidOperationException(
                "Select an offered operation before starting another one.");
        }

        if (Stage != CampaignStage.RegularOperations)
        {
            throw new InvalidOperationException(
                "Regular operations are no longer available.");
        }

        CurrentOperation = new OperationState(definition);

        return CurrentOperation;
    }

    public void SetNextOperationChoices(OperationChoiceSet choices)
    {
        ArgumentNullException.ThrowIfNull(choices);

        if (CurrentOperation is not null)
            throw new InvalidOperationException(
                "Cannot offer operations while another operation is active.");

        if (Stage != CampaignStage.RegularOperations ||
            CompletedRegularOperations == 0 ||
            AreRegularOperationsCompleted)
        {
            throw new InvalidOperationException(
                "Operation choices are not available at this campaign stage.");
        }

        if (PendingOperationChoices is not null)
            throw new InvalidOperationException(
                "Operation choices have already been offered.");

        foreach (var offer in choices.Offers)
        {
            var id = offer.Operation.Id;

            if (_completedOperations.Any(
                    operation => operation.Definition.Id == id) ||
                _missedOpportunityIds.Contains(id))
            {
                throw new InvalidOperationException(
                    $"Operation '{id}' has already been resolved.");
            }
        }

        PendingOperationChoices = choices;
    }

    public OperationState SelectNextOperation(string operationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);

        if (CurrentOperation is not null)
            throw new InvalidOperationException(
                "An operation is already in progress.");

        var choices = PendingOperationChoices
            ?? throw new InvalidOperationException(
                "There are no pending operation choices.");

        var selected = choices.Offers.SingleOrDefault(
            offer => offer.Operation.Id == operationId)
            ?? throw new ArgumentException(
                "Selected operation is not among the available choices.",
                nameof(operationId));

        foreach (var offer in choices.Offers)
        {
            if (offer.Operation.Id != operationId)
            {
                _missedOpportunityIds.Add(offer.Operation.Id);
            }
        }
        PendingOperationChoices = null;

        CurrentOperation = new OperationState(selected.Operation);

        return CurrentOperation;
    }

    public OperationState StartFinalOperation(
    OperationDefinition definition)
    {
        if (Stage != CampaignStage.FinalOperationAvailable)
        {
            throw new InvalidOperationException(
                "Final operation is not available.");
        }

        CurrentOperation = new OperationState(definition);
        Stage = CampaignStage.FinalOperation;

        return CurrentOperation;
    }

    public void CompleteCurrentOperation(OperationOutcome outcome)
    {
        var operation = CurrentOperation
            ?? throw new InvalidOperationException(
                "There is no active operation.");

        operation.Complete(outcome);
        CurrentOperation = null;

        if (Stage == CampaignStage.FinalOperation)
        {
            FinalOperationResult = operation;
            Stage = CampaignStage.Completed;
            return;
        }

        _completedOperations.Add(operation);
        Progression.AddResources(
            CampaignRewardRules.GetResources(operation));

        var rewardSeed = GameRandomSeeds.ForOperationRewards(
            CampaignSeed,
            operation.Definition.Id,
            CompletedRegularOperations);

        var rewardIds = CampaignRewardCatalog.CreateOffers(
            Progression,
            new DeterministicRandom(rewardSeed));

        Progression.SetPendingRewards(rewardIds);

        var positionChange = outcome == OperationOutcome.Victory
            ? 1
            : -1;

        if (outcome == OperationOutcome.Victory &&
            !operation.HasFailedAdditionalObjective)
        {
            positionChange++;
        }

        AdjustWarPosition(positionChange);

        LatestEvent = CampaignEventCatalog.GetAfterOperation(
            CompletedRegularOperations);

        if (LatestEvent is not null)
        {
            AdjustWarPosition(LatestEvent.WarPositionChange);
        }

        if (AreRegularOperationsCompleted)
        {
            Stage = CampaignStage.FinalOperationAvailable;
        }
    }

    public void AdjustWarPosition(int amount)
    {
        if (CurrentOperation is not null)
        {
            throw new InvalidOperationException(
                "War position can only change between operations.");
        }

        var newValue = Math.Clamp(
            (int)WarPosition + amount,
            (int)WarPositionLevel.CriticalDefeat,
            (int)WarPositionLevel.DecisiveAdvantage);

        WarPosition = (WarPositionLevel)newValue;
    }

    internal CampaignSaveData CreateSaveData()
    {
        return new CampaignSaveData
        {
            RegularOperationCount = RegularOperationCount,
            Stage = Stage,
            WarPosition = WarPosition,

            CompletedOperations = _completedOperations
                .Select(CaptureOperation)
                .ToList(),
            Progression = Progression.CreateSaveData(),
            MissedOpportunityIds = _missedOpportunityIds.ToList(),

            CurrentOperation = CurrentOperation is null
                ? null
                : CaptureOperation(CurrentOperation),
            CampaignSeed = CampaignSeed,
            FinalOperationResult = FinalOperationResult is null
                ? null
                : CaptureOperation(FinalOperationResult)

        };
    }

    internal static CampaignState Restore(CampaignSaveData data)
    {
        var campaign = new CampaignState(
            data.RegularOperationCount,
            data.CampaignSeed)
        {
            Stage = data.Stage,
            WarPosition = data.WarPosition
        };

        campaign.Progression = data.Progression is null
    ? new CampaignProgressionState()
    : CampaignProgressionState.Restore(data.Progression);

        campaign._completedOperations.AddRange(
            data.CompletedOperations.Select(
                operation => RestoreOperation(operation, data.WarPosition)));

        campaign._missedOpportunityIds.AddRange(
            data.MissedOpportunityIds);

        campaign.CurrentOperation = data.CurrentOperation is null
            ? null
            : RestoreOperation(data.CurrentOperation, data.WarPosition);

        campaign.FinalOperationResult = data.FinalOperationResult is null
            ? null
            : RestoreOperation(data.FinalOperationResult, data.WarPosition);

        if (campaign.CompletedRegularOperations > 0)
        {
            campaign.LatestEvent = CampaignEventCatalog.GetAfterOperation(
                campaign.CompletedRegularOperations);
        }

        if (campaign.Stage == CampaignStage.RegularOperations &&
            campaign.CurrentOperation is null)
        {
            campaign.PendingOperationChoices =
                OperationChoiceCatalog.GetAfterOperation(
                    campaign.CompletedRegularOperations);
        }

        return campaign;
    }

    private static OperationSaveData CaptureOperation(OperationState operation)
    {
        return new OperationSaveData
        {
            DefinitionId = operation.Definition.Id,
            SelectedCardIds = operation.SelectedCardIds.ToList(),
            BattleResults = operation.BattleResults.ToList(),
            HasFailedAdditionalObjective =
                operation.HasFailedAdditionalObjective,
            Outcome = operation.Outcome
        };
    }

    private static OperationState RestoreOperation(
        OperationSaveData data,
        WarPositionLevel warPosition)
    {
        var definition = OperationCatalog.FindById(data.DefinitionId);

        if (definition is null)
        {
            if (data.DefinitionId != FinalOperationCatalog.Id)
            {
                throw new InvalidDataException(
                    $"Unknown operation ID: {data.DefinitionId}");
            }

            definition = FinalOperationCatalog.Create(warPosition);
        }

        return OperationState.Restore(
            definition,
            data.SelectedCardIds,
            data.BattleResults,
            data.HasFailedAdditionalObjective,
            data.Outcome);
    }
}