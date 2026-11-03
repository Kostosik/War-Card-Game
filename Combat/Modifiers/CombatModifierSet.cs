using GameProject.Combat.Army;

namespace GameProject.Combat.Modifiers;

public sealed class CombatModifierSet
{
    private sealed record Entry(
        CombatModifierDefinition Definition,
        bool Temporary,
        long Sequence);

    private readonly List<Entry> _entries = [];
    private long _sequence;

    public void AddPermanent(CombatModifierDefinition modifier)
    {
        Add(modifier, false);
    }

    public void AddTemporary(CombatModifierDefinition modifier)
    {
        Add(modifier, true);
    }

    public void AddPermanentRange(
        IEnumerable<CombatModifierDefinition> modifiers)
    {
        ArgumentNullException.ThrowIfNull(modifiers);

        foreach (var modifier in modifiers)
            AddPermanent(modifier);
    }

    public int Apply(
        string stat,
        int baseValue,
        DetachmentState subject,
        CombatSide side,
        DetachmentState? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stat);
        ArgumentOutOfRangeException.ThrowIfNegative(baseValue);
        ArgumentNullException.ThrowIfNull(subject);

        var result = baseValue;

        foreach (var entry in _entries)
        {
            var modifier = entry.Definition;

            if (!string.Equals(
                    modifier.Stat,
                    stat,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (modifier.Side.HasValue &&
                modifier.Side.Value != side)
            {
                continue;
            }

            if (!MatchesTags(
                    modifier.RequiredSubjectTags,
                    subject))
            {
                continue;
            }

            if (!MatchesTags(
                    modifier.RequiredTargetTags,
                    target))
            {
                continue;
            }

            result = modifier.Operation switch
            {
                CombatModifierOperation.AddFlat =>
                    checked(result + modifier.Value),

                CombatModifierOperation.MultiplyPercent =>
                    Scale(result, modifier.Value),

                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return result;
    }

    public void ClearTemporary()
    {
        for (var i = _entries.Count - 1; i >= 0; i--)
        {
            if (_entries[i].Temporary)
                _entries.RemoveAt(i);
        }
    }

    private void Add(
        CombatModifierDefinition modifier,
        bool temporary)
    {
        ArgumentNullException.ThrowIfNull(modifier);

        var entry = new Entry(
            modifier,
            temporary,
            _sequence++);

        _entries.Add(entry);

        _entries.Sort(static (a, b) =>
        {
            var priority = a.Definition.Priority.CompareTo(
                b.Definition.Priority);

            return priority != 0
                ? priority
                : a.Sequence.CompareTo(b.Sequence);
        });
    }

    private static bool MatchesTags(
        IReadOnlyList<string> requiredTags,
        DetachmentState? subject)
    {
        if (requiredTags.Count == 0)
            return true;

        if (subject is null)
            return false;

        foreach (var tag in requiredTags)
        {
            if (!subject.Definition.HasTag(tag))
                return false;
        }

        return true;
    }

    private static int Scale(int value, int percent)
    {
        return (int)(((long)value * percent + 50) / 100);
    }
}