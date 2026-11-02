namespace GameProject.Game.Operations;

internal static class OperationCatalog
{
    public static OperationDefinition FirstOperation { get; } =
        new(
            "oil-node",
            "Захват нефтяного узла",
            "Прорваться к нефтяному узлу и захватить объект.",
            "Минимальные потери пехоты.",
            "Пехота, механизированные части и артиллерия.",
            "Три боя на марше, затем решающее сражение " +
            "с усиленной обороной объекта.");
}