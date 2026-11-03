
using GameProject.Combat.Battlefield;

namespace GameProject.Game.Campaign;

internal sealed class OperationPreparationState
{
    private readonly HashSet<string> _purchasedIds =
        new(StringComparer.Ordinal);

    public bool HasReconnaissance =>
        HasPurchased(OperationPreparationCatalog.ReconnaissanceId);

    public bool HasTerrainSelection =>
        HasPurchased(OperationPreparationCatalog.TerrainSelectionId);

    public bool HasMedicalSupport =>
        HasPurchased(OperationPreparationCatalog.MedicalSupportId);

    public bool HasSupplyReserve =>
        HasPurchased(OperationPreparationCatalog.SupplyReserveId);

    public BattleDirection? TerrainDirection { get; private set; }

    public string? TerrainId { get; private set; }

    public IReadOnlyCollection<string> PurchasedIds =>
        Array.AsReadOnly<string>(_purchasedIds.ToArray());

    public bool HasPurchased(string id)
    {
        return _purchasedIds.Contains(id);
    }

    internal bool CanPurchase(
        OperationPreparationDefinition preparation,
        BattleDirection? direction = null,
        string? terrainId = null)
    {
        ArgumentNullException.ThrowIfNull(preparation);

        if (HasPurchased(preparation.Id))
            return false;

        if (preparation.Kind == OperationPreparationKind.TerrainSelection)
        {
            return direction.HasValue &&
                   Enum.IsDefined(direction.Value) &&
                   !string.IsNullOrWhiteSpace(terrainId) &&
                   BattleTerrainCatalog.All.Any(
                       terrain => terrain.Id == terrainId);
        }

        return !direction.HasValue && terrainId is null;
    }

    internal void Purchase(
        OperationPreparationDefinition preparation,
        BattleDirection? direction = null,
        string? terrainId = null)
    {
        if (!CanPurchase(preparation, direction, terrainId))
        {
            throw new InvalidOperationException(
                "This preparation cannot be purchased.");
        }

        _purchasedIds.Add(preparation.Id);

        if (preparation.Kind == OperationPreparationKind.TerrainSelection)
        {
            TerrainDirection = direction;
            TerrainId = terrainId;
        }
    }

    public BattleDefinition ApplyTo(
        BattleDefinition definition,
        int battleNumber)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (battleNumber != 1 || !HasTerrainSelection)
            return definition;

        var terrain = BattleTerrainCatalog.All.First(
            item => item.Id == TerrainId);

        return new BattleDefinition(
            definition.EnemyForces,
            definition.Objective,
            TerrainDirection == BattleDirection.LeftFlank
                ? terrain
                : definition.LeftTerrain,
            TerrainDirection == BattleDirection.Center
                ? terrain
                : definition.CenterTerrain,
            TerrainDirection == BattleDirection.RightFlank
                ? terrain
                : definition.RightTerrain,
            definition.Formation);
    }

    internal OperationPreparationSaveData CreateSaveData()
    {
        return new OperationPreparationSaveData
        {
            PurchasedIds = _purchasedIds.ToList(),
            TerrainDirection = TerrainDirection,
            TerrainId = TerrainId
        };
    }

    internal void Restore(OperationPreparationSaveData? data)
    {
        if (data is null)
            return;

        foreach (var id in data.PurchasedIds)
        {
            if (OperationPreparationCatalog.FindById(id) is null ||
                !_purchasedIds.Add(id))
            {
                throw new InvalidDataException(
                    $"Invalid operation preparation ID: {id}");
            }
        }

        var hasTerrainPreparation =
            HasPurchased(OperationPreparationCatalog.TerrainSelectionId);

        if (hasTerrainPreparation)
        {
            if (!data.TerrainDirection.HasValue ||
                !Enum.IsDefined(data.TerrainDirection.Value) ||
                string.IsNullOrWhiteSpace(data.TerrainId) ||
                !BattleTerrainCatalog.All.Any(
                    terrain => terrain.Id == data.TerrainId))
            {
                throw new InvalidDataException(
                    "Invalid terrain preparation data.");
            }

            TerrainDirection = data.TerrainDirection;
            TerrainId = data.TerrainId;
        }
        else if (data.TerrainDirection.HasValue ||
                 data.TerrainId is not null)
        {
            throw new InvalidDataException(
                "Terrain data exists without terrain preparation.");
        }
    }
}
