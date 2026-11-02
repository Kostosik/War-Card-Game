namespace GameProject.Game.Campaign;

internal sealed record CampaignEvent(
    string Title,
    string Description,
    int WarPositionChange);

internal static class CampaignEventCatalog
{
    private static readonly CampaignEvent[] Events =
    [
        new(
            "Успех союзников на севере",
            "Союзные войска захватили важный железнодорожный узел. " +
            "Снабжение наших частей улучшается.",
            1),

        new(
            "Прорыв противника на западе",
            "Противник сосредоточил дополнительные бронетанковые " +
            "соединения и продвинулся на западном направлении.",
            -1),

        new(
            "Воздушная поддержка",
            "Союзное командование выделило дополнительные " +
            "авиационные силы для поддержки наступления.",
            1)
    ];

    public static CampaignEvent? GetAfterOperation(int operationNumber)
    {
        return operationNumber <= Events.Length
            ? Events[operationNumber - 1]
            : null;
    }
}