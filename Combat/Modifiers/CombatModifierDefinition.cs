namespace GameProject.Combat.Modifiers;

public enum CombatModifierOperation
{
    AddFlat,
    MultiplyPercent
}

public enum CombatSide
{
    Player,
    Enemy
}

public sealed class CombatModifierDefinition
{
    public string Id { get; }
    public string Stat { get; }
    public CombatModifierOperation Operation { get; }
    public int Value { get; }
    public CombatSide? Side { get; }
    public int Priority { get; }

    public IReadOnlyList<string> RequiredSubjectTags { get; }
    public IReadOnlyList<string> RequiredTargetTags { get; }

    public CombatModifierDefinition(
        string id,
        string stat,
        CombatModifierOperation operation,
        int value,
        CombatSide? side = null,
        IEnumerable<string>? requiredSubjectTags = null,
        IEnumerable<string>? requiredTargetTags = null,
        int priority = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(stat);

        ArgumentOutOfRangeException.ThrowIfNegative(value);

        if (!Enum.IsDefined(operation))
            throw new ArgumentOutOfRangeException(nameof(operation));

        if (side.HasValue && !Enum.IsDefined(side.Value))
            throw new ArgumentOutOfRangeException(nameof(side));

        Id = id;
        Stat = stat;
        Operation = operation;
        Value = value;
        Side = side;
        Priority = priority;

        RequiredSubjectTags = CreateTags(requiredSubjectTags);
        RequiredTargetTags = CreateTags(requiredTargetTags);
    }

    private static IReadOnlyList<string> CreateTags(
        IEnumerable<string>? tags)
    {
        if (tags is null)
            return Array.Empty<string>();

        var result = new List<string>();
        var unique = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tag in tags)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var normalized = tag.Trim();

            if (!unique.Add(normalized))
                throw new ArgumentException(
                    $"Duplicate modifier tag: {normalized}");

            result.Add(normalized);
        }

        return Array.AsReadOnly(result.ToArray());
    }
}