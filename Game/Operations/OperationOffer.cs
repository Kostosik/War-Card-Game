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

    public OperationChoiceSet(params OperationOffer[] offers)
    {
        ArgumentNullException.ThrowIfNull(offers);

        if (offers.Length < 2)
        {
            throw new ArgumentException(
                "At least two operation offers are required.",
                nameof(offers));
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var offer in offers)
        {
            ArgumentNullException.ThrowIfNull(offer);

            if (!ids.Add(offer.Operation.Id))
            {
                throw new ArgumentException(
                    "Operation offers must be different.",
                    nameof(offers));
            }
        }

        Offers = Array.AsReadOnly(offers.ToArray());
    }
}