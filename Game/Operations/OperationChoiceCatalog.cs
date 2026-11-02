namespace GameProject.Game.Operations;

internal static class OperationChoiceCatalog
{
    // Предложения после первой операции.
    public static OperationChoiceSet AfterFirstOperation { get; } =
        new(
            new OperationOffer(
                OperationCatalog.IndustrialFacilityOperation,
                "Промышленный объект хорошо укреплён.",
                "Получение преимущества в снабжении."),

            new OperationOffer(
                OperationCatalog.RiverCrossingOperation,
                "Противник готовит наступление у переправы.",
                "Сохранение позиций на фронте."));

    // Предложения после второй операции.
    public static OperationChoiceSet AfterSecondOperation { get; } =
        new(
            new OperationOffer(
                OperationCatalog.ArtilleryHubOperation,
                "Артиллерийский узел прикрыт тяжёлой бронёй.",
                "Ослабление огневой поддержки противника."),

            new OperationOffer(
                OperationCatalog.SupplyRouteOperation,
                "Линия снабжения находится под угрозой засад.",
                "Улучшение снабжения перед финальной операцией."));
}