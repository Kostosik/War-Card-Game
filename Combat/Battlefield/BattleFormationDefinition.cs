namespace GameProject.Combat.Battlefield;

public sealed class BattleFormationDefinition
{
    private readonly IReadOnlyDictionary<
        BattleDirection,
        IReadOnlyList<BattleDirection>> _neighbors;

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    public int SupportDamagePerNeighbor { get; }

    public IReadOnlyList<BattleDirectionConnection> Connections { get; }

    public BattleFormationDefinition(
        string id,
        string name,
        string description,
        IEnumerable<BattleDirectionConnection> connections,
        int supportDamagePerNeighbor = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(connections);

        ArgumentOutOfRangeException.ThrowIfNegative(
            supportDamagePerNeighbor);

        var directions = Enum.GetValues<BattleDirection>();

        var neighbors = directions.ToDictionary(
            direction => direction,
            _ => new List<BattleDirection>());

        var uniqueConnections = new HashSet<BattleDirectionConnection>();

        foreach (var connection in connections)
        {
            if (!Enum.IsDefined(connection.First) ||
                !Enum.IsDefined(connection.Second))
            {
                throw new ArgumentException(
                    "Formation contains an invalid direction.",
                    nameof(connections));
            }

            if (connection.First == connection.Second)
            {
                throw new ArgumentException(
                    "A direction cannot be connected to itself.",
                    nameof(connections));
            }

            if (!uniqueConnections.Add(connection))
            {
                throw new ArgumentException(
                    "Formation contains a duplicate connection.",
                    nameof(connections));
            }

            neighbors[connection.First].Add(connection.Second);
            neighbors[connection.Second].Add(connection.First);
        }

        Id = id;
        Name = name;
        Description = description;

        SupportDamagePerNeighbor = supportDamagePerNeighbor;

        Connections = Array.AsReadOnly(
            uniqueConnections.ToArray());

        _neighbors = neighbors.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<BattleDirection>)
                Array.AsReadOnly(pair.Value.ToArray()));
    }

    public IReadOnlyList<BattleDirection> GetNeighbors(
        BattleDirection direction)
    {
        return _neighbors.TryGetValue(direction, out var result)
            ? result
            : throw new ArgumentOutOfRangeException(nameof(direction));
    }

    public bool AreConnected(
        BattleDirection first,
        BattleDirection second)
    {
        return GetNeighbors(first).Contains(second);
    }
}