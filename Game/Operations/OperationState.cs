namespace GameProject.Game.Operations;

internal sealed class OperationState
{
    public const int TotalBattleCount = OperationDefinition.BattleCount;

    private readonly List<string> _selectedCardIds = [];
    private readonly List<OperationBattleResult> _battleResults = [];
    public OperationBattleDefinition CurrentBattle =>
    Definition.Battles[CurrentBattleNumber - 1];
    public IReadOnlyList<string> SelectedCardIds => _selectedCardIds;
    public bool HasFailedAdditionalObjective { get; private set; }
    public IReadOnlyList<OperationBattleResult> BattleResults =>
        _battleResults;

    public int CurrentBattleNumber { get; private set; } = 1;

    public int CompletedBattleCount => _battleResults.Count;

    public int WonBattleCount =>
        _battleResults.Count(
            result => result == OperationBattleResult.Victory);

    public bool IsFinalBattle =>
        CurrentBattleNumber == TotalBattleCount;

    public OperationOutcome? Outcome { get; private set; }

    public bool IsCompleted => Outcome.HasValue;

    public OperationDefinition Definition { get; }

    public OperationState(OperationDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
    }

    public bool RegisterBattleVictory()
    {
        EnsureNotCompleted();
        EnsureBattleCanBeRecorded();

        _battleResults.Add(OperationBattleResult.Victory);

        if (IsFinalBattle)
            return true;

        CurrentBattleNumber++;

        return false;
    }

    public void RegisterBattleDefeat()
    {
        EnsureNotCompleted();
        EnsureBattleCanBeRecorded();

        _battleResults.Add(OperationBattleResult.Defeat);
    }

    public void FailAdditionalObjective()
    {
        EnsureNotCompleted();
        HasFailedAdditionalObjective = true;
    }
    public void SetSelectedCards(IEnumerable<string> cardIds)
    {
        ArgumentNullException.ThrowIfNull(cardIds);
        EnsureNotCompleted();

        var ids = cardIds.ToArray();

        if (ids.Length > 3)
        {
            throw new ArgumentException(
                "An operation can have at most three selected cards.",
                nameof(cardIds));
        }

        for (var i = 0; i < ids.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(ids[i]))
            {
                throw new ArgumentException(
                    "Card IDs cannot be empty.",
                    nameof(cardIds));
            }

            for (var j = i + 1; j < ids.Length; j++)
            {
                if (string.Equals(
                    ids[i],
                    ids[j],
                    StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Selected cards must be unique.",
                        nameof(cardIds));
                }
            }
        }

        _selectedCardIds.Clear();
        _selectedCardIds.AddRange(ids);
    }

    public void Complete(OperationOutcome outcome)
    {
        EnsureNotCompleted();

        switch (outcome)
        {
            case OperationOutcome.Victory:
                if (CompletedBattleCount != TotalBattleCount ||
                    WonBattleCount != TotalBattleCount)
                {
                    throw new InvalidOperationException(
                        "An operation can only be won after all battles have been won.");
                }

                break;

            case OperationOutcome.Defeat:
                if (!_battleResults.Contains(OperationBattleResult.Defeat))
                {
                    throw new InvalidOperationException(
                        "An operation can only be defeated after losing a battle.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(outcome),
                    outcome,
                    "Unknown operation outcome.");
        }

        Outcome = outcome;
    }

    private void EnsureBattleCanBeRecorded()
    {
        if (CompletedBattleCount >= TotalBattleCount)
        {
            throw new InvalidOperationException(
                "All battles in this operation have already been completed.");
        }
    }

    private void EnsureNotCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException(
                "The operation has already been completed.");
        }
    }

    internal static OperationState Restore(
    OperationDefinition definition,
    IEnumerable<string> selectedCardIds,
    IEnumerable<OperationBattleResult> battleResults,
    bool hasFailedAdditionalObjective,
    OperationOutcome? outcome)
    {
        var state = new OperationState(definition);

        state._selectedCardIds.AddRange(selectedCardIds);
        state._battleResults.AddRange(battleResults);

        state.HasFailedAdditionalObjective =
            hasFailedAdditionalObjective;

        state.Outcome = outcome;

        state.CurrentBattleNumber = Math.Min(
            state._battleResults.Count + 1,
            TotalBattleCount);

        return state;
    }
}

internal enum OperationBattleResult
{
    Victory,
    Defeat
}

internal enum OperationOutcome
{
    Victory,
    Defeat
}