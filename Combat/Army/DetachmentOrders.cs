namespace GameProject.Combat.Army;

public static class DetachmentOrders
{
    public static readonly DetachmentOrderDefinition Attack =
        new("attack", "Наступать", 20, 0, 0);

    public static readonly DetachmentOrderDefinition HoldPosition =
        new("hold-position", "Удерживать позицию", 0, 20, 0);

    public static readonly DetachmentOrderDefinition ConserveForces =
        new("conserve-forces", "Беречь силы", 0, 0, 5);

    public static IReadOnlyList<DetachmentOrderDefinition> All { get; } =
        Array.AsReadOnly(new[]
        {
            Attack,
            HoldPosition,
            ConserveForces
        });
}