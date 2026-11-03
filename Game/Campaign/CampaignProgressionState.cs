using GameProject.Combat.Army;
using GameProject.Combat.Cards;

namespace GameProject.Game.Campaign;

internal sealed class CampaignProgressionState
{
    public const int StartingResources = 4;

    private readonly HashSet<string> _unlockedDetachmentIds =
        new(StringComparer.Ordinal);

    private readonly HashSet<string> _unlockedCardIds =
        new(StringComparer.Ordinal);

    private readonly List<string> _pendingRewardIds = [];

    public int Resources { get; private set; } = StartingResources;

    public IReadOnlyList<string> PendingRewardIds =>
        _pendingRewardIds;

    public bool HasPendingReward =>
        _pendingRewardIds.Count > 0;

    public IReadOnlyList<DetachmentDefinition> GetAvailableDetachments()
    {
        return Array.AsReadOnly(
            DetachmentCatalog.Player.All
                .Concat(
                    DetachmentCatalog.Player.Unlockable.Where(
                        item => _unlockedDetachmentIds.Contains(item.Id)))
                .ToArray());
    }

    public IReadOnlyList<CardDefinition> GetAvailableCards()
    {
        return Array.AsReadOnly(
            TacticalCardCatalog.All
                .Concat(
                    TacticalCardCatalog.Unlockable.Where(
                        item => _unlockedCardIds.Contains(item.Id)))
                .ToArray());
    }

    public bool CanAfford(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return Resources >= amount;
    }

    public bool TrySpendResources(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (!CanAfford(amount))
            return false;

        Resources -= amount;
        return true;
    }

    public void AddResources(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        Resources = checked(Resources + amount);
    }

    internal bool CanOfferReward(CampaignRewardDefinition reward)
    {
        ArgumentNullException.ThrowIfNull(reward);

        return reward.Kind switch
        {
            CampaignRewardKind.UnlockDetachment =>
                reward.TargetId is not null &&
                !_unlockedDetachmentIds.Contains(reward.TargetId) &&
                DetachmentCatalog.Player.Unlockable.Any(
                    item => item.Id == reward.TargetId),

            CampaignRewardKind.UnlockCard =>
                reward.TargetId is not null &&
                !_unlockedCardIds.Contains(reward.TargetId) &&
                TacticalCardCatalog.Unlockable.Any(
                    item => item.Id == reward.TargetId),

            CampaignRewardKind.Resources =>
                reward.ResourceAmount > 0,

            _ => false
        };
    }

    internal void SetPendingRewards(IEnumerable<string> rewardIds)
    {
        ArgumentNullException.ThrowIfNull(rewardIds);

        if (HasPendingReward)
        {
            throw new InvalidOperationException(
                "Reward choices are already pending.");
        }

        var ids = rewardIds.ToArray();

        if (ids.Length != CampaignRewardCatalog.OfferCount)
        {
            throw new ArgumentException(
                $"Exactly {CampaignRewardCatalog.OfferCount} rewards are required.",
                nameof(rewardIds));
        }

        if (ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
        {
            throw new ArgumentException(
                "Reward choices must be unique.",
                nameof(rewardIds));
        }

        foreach (var id in ids)
        {
            var reward = CampaignRewardCatalog.FindById(id);

            if (reward is null || !CanOfferReward(reward))
            {
                throw new ArgumentException(
                    $"Reward '{id}' is not available.",
                    nameof(rewardIds));
            }
        }

        _pendingRewardIds.AddRange(ids);
    }

    public void ClaimReward(string rewardId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rewardId);

        var pendingIndex = _pendingRewardIds.FindIndex(
            id => id == rewardId);

        if (pendingIndex < 0)
        {
            throw new InvalidOperationException(
                "This reward is not among the available choices.");
        }

        var reward = CampaignRewardCatalog.FindById(rewardId)
            ?? throw new InvalidDataException(
                $"Unknown campaign reward: {rewardId}");

        if (!CanOfferReward(reward))
        {
            throw new InvalidOperationException(
                "This reward can no longer be claimed.");
        }

        switch (reward.Kind)
        {
            case CampaignRewardKind.UnlockDetachment:
                _unlockedDetachmentIds.Add(reward.TargetId!);
                break;

            case CampaignRewardKind.UnlockCard:
                _unlockedCardIds.Add(reward.TargetId!);
                break;

            case CampaignRewardKind.Resources:
                AddResources(reward.ResourceAmount);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        _pendingRewardIds.Clear();
    }

    internal CampaignProgressionSaveData CreateSaveData()
    {
        return new CampaignProgressionSaveData
        {
            Resources = Resources,

            UnlockedDetachmentIds = _unlockedDetachmentIds
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList(),

            UnlockedCardIds = _unlockedCardIds
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList(),

            PendingRewardIds = _pendingRewardIds.ToList()
        };
    }

    internal static CampaignProgressionState Restore(
        CampaignProgressionSaveData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Resources < 0)
        {
            throw new InvalidDataException(
                "Campaign resources cannot be negative.");
        }

        var state = new CampaignProgressionState
        {
            Resources = data.Resources
        };

        foreach (var id in data.UnlockedDetachmentIds)
        {
            if (!DetachmentCatalog.Player.Unlockable.Any(
                    item => item.Id == id) ||
                !state._unlockedDetachmentIds.Add(id))
            {
                throw new InvalidDataException(
                    $"Invalid unlocked detachment ID: {id}");
            }
        }

        foreach (var id in data.UnlockedCardIds)
        {
            if (!TacticalCardCatalog.Unlockable.Any(
                    item => item.Id == id) ||
                !state._unlockedCardIds.Add(id))
            {
                throw new InvalidDataException(
                    $"Invalid unlocked card ID: {id}");
            }
        }

        if (data.PendingRewardIds.Count > 0)
        {
            state.SetPendingRewards(data.PendingRewardIds);
        }

        return state;
    }
}

internal sealed class CampaignProgressionSaveData
{
    public int Resources { get; set; } =
        CampaignProgressionState.StartingResources;

    public List<string> UnlockedDetachmentIds { get; set; } = [];

    public List<string> UnlockedCardIds { get; set; } = [];

    public List<string> PendingRewardIds { get; set; } = [];
}