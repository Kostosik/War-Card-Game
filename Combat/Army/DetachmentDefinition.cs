namespace GameProject.Combat.Army;

public sealed class DetachmentDefinition
{
    private readonly HashSet<string> _tagSet;

    public string Id { get; }
    public string Name { get; }
    public int MaxHealth { get; }
    public int Attack { get; }

    public IReadOnlyCollection<string> Tags { get; }

    // Оставлено для совместимости со старым кодом.
    // Новые игровые правила должны использовать HasTag().
    public DetachmentType Type { get; }

    public DetachmentDefinition(
        string id,
        string name,
        int maxHealth,
        int attack,
        DetachmentType type = DetachmentType.Infantry)
        : this(id, name, maxHealth, attack, GetTagsForType(type))
    {
    }

    public DetachmentDefinition(
        string id,
        string name,
        int maxHealth,
        int attack,
        IEnumerable<string> tags)
        : this(id, name, maxHealth, attack, CreateTagArray(tags))
    {
    }

    private DetachmentDefinition(
        string id,
        string name,
        int maxHealth,
        int attack,
        string[] tags)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHealth);
        ArgumentOutOfRangeException.ThrowIfNegative(attack);

        Id = id;
        Name = name;
        MaxHealth = maxHealth;
        Attack = attack;

        Tags = Array.AsReadOnly(tags);
        _tagSet = new HashSet<string>(
            tags,
            StringComparer.Ordinal);

        // Представление для старого API.
        Type = GetLegacyType(_tagSet);
    }

    public bool HasTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return _tagSet.Contains(tag);
    }

    private static string[] GetTagsForType(DetachmentType type)
    {
        return type switch
        {
            DetachmentType.Infantry =>
                [DetachmentTags.Infantry],

            DetachmentType.Mechanized =>
                [DetachmentTags.Mechanized],

            DetachmentType.Artillery =>
                [DetachmentTags.Artillery],

            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static string[] CreateTagArray(
        IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var result = new List<string>();
        var unique = new HashSet<string>(StringComparer.Ordinal);

        foreach (var tag in tags)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(tag);

            var normalized = tag.Trim();

            if (!unique.Add(normalized))
            {
                throw new ArgumentException(
                    $"Duplicate detachment tag: {normalized}",
                    nameof(tags));
            }

            result.Add(normalized);
        }

        return result.ToArray();
    }

    private static DetachmentType GetLegacyType(
        HashSet<string> tags)
    {
        if (tags.Contains(DetachmentTags.Mechanized))
            return DetachmentType.Mechanized;

        if (tags.Contains(DetachmentTags.Artillery))
            return DetachmentType.Artillery;

        return DetachmentType.Infantry;
    }
}