using GameProject.Game.Operations;

namespace GameProject.Game.Campaign;

internal sealed class CampaignState
{
    public const int DefaultRegularOperationCount = 3;

    private readonly List<OperationState> _completedOperations = [];
    private readonly List<string> _missedOpportunityIds = [];
    public OperationState? FinalOperationResult { get; private set; }
    public CampaignEvent? LatestEvent { get; private set; }
    public IReadOnlyList<string> MissedOpportunityIds =>
    _missedOpportunityIds;

    public OperationChoiceSet? PendingOperationChoices { get; private set; }
    public CampaignState(
        int regularOperationCount = DefaultRegularOperationCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            regularOperationCount);

        RegularOperationCount = regularOperationCount;
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

        var missed = choices.Offers.Single(
            offer => offer.Operation.Id != operationId);

        _missedOpportunityIds.Add(missed.Operation.Id);
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

        _completedOperations.Add(operation);
        CurrentOperation = null;

        if (Stage == CampaignStage.FinalOperation)
        {
            FinalOperationResult = operation;
            Stage = CampaignStage.Completed;
            return;
        }

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
}