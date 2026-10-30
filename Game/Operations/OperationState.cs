namespace GameProject.Game.Operations;

internal sealed class OperationState
{
    private readonly List<string> _selectedCardIds = [];

    public IReadOnlyList<string> SelectedCardIds => _selectedCardIds;

    public OperationOutcome? Outcome { get; private set; }

    public bool IsCompleted => Outcome.HasValue;

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
                if (string.Equals(ids[i], ids[j], StringComparison.Ordinal))
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
        Outcome = outcome;
    }

    private void EnsureNotCompleted()
    {
        if (IsCompleted)
        {
            throw new InvalidOperationException(
                "The operation has already been completed.");
        }
    }
}

internal enum OperationOutcome
{
    Victory,
    Defeat
}