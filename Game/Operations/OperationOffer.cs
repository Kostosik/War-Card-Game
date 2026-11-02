namespace GameProject.Game.Operations;

internal sealed class OperationOffer
{
    public OperationDefinition Operation { get; }
    public string RiskDescription { get; }
    public string ExpectedResult { get; }

    public OperationOffer(
        OperationDefinition operation,
        string riskDescription,
        string expectedResult)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentException.ThrowIfNullOrWhiteSpace(riskDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedResult);

        Operation = operation;
        RiskDescription = riskDescription;
        ExpectedResult = expectedResult;
    }
}

internal sealed class OperationChoiceSet
{
    public IReadOnlyList<OperationOffer> Offers { get; }

    public OperationChoiceSet(
        OperationOffer first,
        OperationOffer second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.Operation.Id == second.Operation.Id)
        {
            throw new ArgumentException(
                "Operation offers must be different.");
        }

        Offers = Array.AsReadOnly(new[] { first, second });
    }
}