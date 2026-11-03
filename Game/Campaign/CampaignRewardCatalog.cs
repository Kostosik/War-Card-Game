using Engine.Core.Determinism;
using GameProject.Combat.Army;
using GameProject.Combat.Cards;

namespace GameProject.Game.Campaign;

internal enum CampaignRewardKind
{
    UnlockDetachment,
    UnlockCard,
    Resources
}

internal sealed record CampaignRewardDefinition(
    string Id,
    string Title,
    string Description,
    CampaignRewardKind Kind,
    string? TargetId = null,
    int ResourceAmount = 0);

internal static class CampaignRewardCatalog
{
    public const int OfferCount = 3;

    public static IReadOnlyList<CampaignRewardDefinition> All { get; } =
        Array.AsReadOnly(
        [
            new CampaignRewardDefinition(
                "unlock-engineers",
                "Инженерный отряд",
                "Открывает инженерное подразделение. " +
                "Выносливый отряд с умеренной атакой.",
                CampaignRewardKind.UnlockDetachment,
                "engineers"),

            new CampaignRewardDefinition(
                "unlock-assault",
                "Штурмовой отряд",
                "Открывает штурмовое подразделение. " +
                "Высокая атака, но меньше здоровья.",
                CampaignRewardKind.UnlockDetachment,
                "assault-squad"),

            new CampaignRewardDefinition(
                "unlock-heavy-artillery",
                "Тяжёлая артиллерия",
                "Добавляет в доступный набор карт " +
                "мощный артиллерийский удар.",
                CampaignRewardKind.UnlockCard,
                "heavy-artillery"),

            new CampaignRewardDefinition(
                "unlock-emergency-supply",
                "Аварийное снабжение",
                "Добавляет карту, восстанавливающую " +
                "25 единиц здоровья.",
                CampaignRewardKind.UnlockCard,
                "emergency-supply"),

            new CampaignRewardDefinition(
                "resource-cache",
                "Запасы штаба",
                "Получить дополнительно 3 ресурса.",
                CampaignRewardKind.Resources,
                ResourceAmount: 3)
        ]);

    public static CampaignRewardDefinition? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.FirstOrDefault(reward => reward.Id == id);
    }

    public static IReadOnlyList<string> CreateOffers(
        CampaignProgressionState progression,
        DeterministicRandom random)
    {
        ArgumentNullException.ThrowIfNull(progression);

        var candidates = All
            .Where(progression.CanOfferReward)
            .ToArray();

        if (candidates.Length < OfferCount)
        {
            throw new InvalidOperationException(
                "Not enough campaign rewards are available.");
        }

        var randomState = random;

        for (var i = 0; i < OfferCount; i++)
        {
            var selectedIndex = randomState.NextInt(
                i,
                candidates.Length);

            (candidates[i], candidates[selectedIndex]) =
                (candidates[selectedIndex], candidates[i]);
        }

        return Array.AsReadOnly(
            candidates
                .Take(OfferCount)
                .Select(reward => reward.Id)
                .ToArray());
    }
}