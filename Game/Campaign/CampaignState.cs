using GameProject.Game.Operations;

namespace GameProject.Game.Campaign;

internal sealed class CampaignState
{
    public const int DefaultRegularOperationCount = 6;

    private readonly List<OperationState> _completedOperations = [];

    public CampaignState(
        int regularOperationCount = DefaultRegularOperationCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            regularOperationCount);

        RegularOperationCount = regularOperationCount;
    }

    public int RegularOperationCount { get; }

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
        AreRegularOperationsCompleted
            ? null
            : CompletedRegularOperations + 1;

    public IReadOnlyList<OperationState> OperationHistory =>
        _completedOperations;

    public OperationState StartNextOperation()
    {
        if (CurrentOperation is not null)
        {
            throw new InvalidOperationException(
                "An operation is already in progress.");
        }

        if (AreRegularOperationsCompleted)
        {
            throw new InvalidOperationException(
                "All regular operations have been completed.");
        }

        CurrentOperation = new OperationState();

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
    }
}