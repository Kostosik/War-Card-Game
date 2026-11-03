
namespace GameProject.Game.Campaign;

internal enum OperationPreparationKind
{
    Reconnaissance,
    TerrainSelection,
    MedicalSupport,
    SupplyReserve
}

internal sealed record OperationPreparationDefinition(
    string Id,
    string Title,
    string Description,
    int Cost,
    OperationPreparationKind Kind);

internal static class OperationPreparationCatalog
{
    public const string ReconnaissanceId = "reconnaissance";
    public const string TerrainSelectionId = "terrain-selection";
    public const string MedicalSupportId = "medical-support";
    public const string SupplyReserveId = "supply-reserve";

    public static IReadOnlyList<OperationPreparationDefinition> All { get; } =
        Array.AsReadOnly<OperationPreparationDefinition>(
            [
            new(
                ReconnaissanceId,
                "Разведка",
                "Раскрывает расположение и состав сил противника во всех боях операции.",
                2,
                OperationPreparationKind.Reconnaissance),

            new(
                TerrainSelectionId,
                "Инженерная подготовка",
                "Позволяет выбрать местность одного направления в первом бою.",
                3,
                OperationPreparationKind.TerrainSelection),

            new(
                MedicalSupportId,
                "Медицинская группа",
                "Увеличивает восстановление отрядов между боями с 50% до 75%.",
                2,
                OperationPreparationKind.MedicalSupport),

            new(
                SupplyReserveId,
                "Резерв снабжения",
                "Добавляет одну дополнительную карту «Снабжение» в каждый бой операции.",
                2,
                OperationPreparationKind.SupplyReserve)
        ]);

    public static OperationPreparationDefinition? FindById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        return All.FirstOrDefault(item => item.Id == id);
    }
}
