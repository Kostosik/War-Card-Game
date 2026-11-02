using GameProject.Combat.Army;

namespace GameProject.Combat.Battlefield;

public sealed class ArmyDeploymentState
{
    private readonly IReadOnlyList<DetachmentDefinition> _available;

    private DetachmentDefinition? _leftFlank;
    private DetachmentDefinition? _center;
    private DetachmentDefinition? _rightFlank;
    private readonly Dictionary<string, DetachmentState> _detachmentStates;
    private readonly Dictionary<BattleDirection, DetachmentOrderDefinition> _orders =
    new()
    {
        [BattleDirection.LeftFlank] = DetachmentOrders.Attack,
        [BattleDirection.Center] = DetachmentOrders.Attack,
        [BattleDirection.RightFlank] = DetachmentOrders.Attack
    };

    public IReadOnlyList<DetachmentDefinition> AvailableDetachments =>
        _available;

    public bool IsReadyForBattle =>
        _leftFlank is not null &&
        _center is not null &&
        _rightFlank is not null;

    public ArmyDeploymentState(
        IEnumerable<DetachmentDefinition> available)
    {
        ArgumentNullException.ThrowIfNull(available);

        var definitions = available.ToArray();

        if (definitions.Length == 0)
        {
            throw new ArgumentException(
                "At least one detachment is required.",
                nameof(available));
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var definition in definitions)
        {
            ArgumentNullException.ThrowIfNull(definition);

            if (!ids.Add(definition.Id))
            {
                throw new ArgumentException(
                    $"Duplicate detachment ID: {definition.Id}",
                    nameof(available));
            }
        }

        _available = Array.AsReadOnly(definitions);
        _detachmentStates = definitions.ToDictionary(
    definition => definition.Id,
    definition => new DetachmentState(definition),
    StringComparer.Ordinal);
    }

    public bool TryAssignOrder(
    BattleDirection direction,
    string orderId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);

        var order = DetachmentOrders.All
            .FirstOrDefault(item => item.Id == orderId);

        if (order is null || !_orders.ContainsKey(direction))
            return false;

        _orders[direction] = order;
        return true;
    }

    public DetachmentState GetAssignedState(BattleDirection direction)
    {
        var definition = GetAssigned(direction)
            ?? throw new InvalidOperationException(
                $"No detachment assigned to {direction}.");

        return _detachmentStates[definition.Id];
    }

    public void RecoverBetweenBattles(int missingHealthPercent)
    {
        if (missingHealthPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(missingHealthPercent));
        }

        RecoverAssigned(_leftFlank, missingHealthPercent);
        RecoverAssigned(_center, missingHealthPercent);
        RecoverAssigned(_rightFlank, missingHealthPercent);
    }



    public DetachmentState GetDetachmentState(string detachmentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detachmentId);

        return _detachmentStates.TryGetValue(detachmentId, out var state)
            ? state
            : throw new ArgumentException(
                $"Unknown detachment ID: {detachmentId}",
                nameof(detachmentId));
    }

    private void RecoverAssigned(
        DetachmentDefinition? definition,
        int missingHealthPercent)
    {
        if (definition is null)
            return;

        _detachmentStates[definition.Id]
            .RecoverBetweenBattles(missingHealthPercent);
    }

    public bool TryAssign(
        BattleDirection direction,
        string detachmentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detachmentId);

        var definition = _available.FirstOrDefault(
            item => item.Id == detachmentId);

        if (definition is null)
        {
            return false;
        }

        if (IsAssignedElsewhere(direction, detachmentId))
        {
            return false;
        }

        switch (direction)
        {
            case BattleDirection.LeftFlank:
                _leftFlank = definition;
                break;

            case BattleDirection.Center:
                _center = definition;
                break;

            case BattleDirection.RightFlank:
                _rightFlank = definition;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction));
        }

        return true;
    }

    public DetachmentOrderDefinition GetAssignedOrder(
    BattleDirection direction)
    {
        return _orders.TryGetValue(direction, out var order)
            ? order
            : throw new ArgumentOutOfRangeException(nameof(direction));
    }

    public DetachmentDefinition? GetAssigned(
        BattleDirection direction)
    {
        return direction switch
        {
            BattleDirection.LeftFlank => _leftFlank,
            BattleDirection.Center => _center,
            BattleDirection.RightFlank => _rightFlank,

            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };
    }

    public void RemoveAssignment(BattleDirection direction)
    {
        switch (direction)
        {
            case BattleDirection.LeftFlank:
                _leftFlank = null;
                break;

            case BattleDirection.Center:
                _center = null;
                break;

            case BattleDirection.RightFlank:
                _rightFlank = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction));
        }
    }

    private bool IsAssignedElsewhere(
        BattleDirection direction,
        string detachmentId)
    {
        return
            (direction != BattleDirection.LeftFlank &&
             _leftFlank?.Id == detachmentId) ||

            (direction != BattleDirection.Center &&
             _center?.Id == detachmentId) ||

            (direction != BattleDirection.RightFlank &&
             _rightFlank?.Id == detachmentId);
    }
}