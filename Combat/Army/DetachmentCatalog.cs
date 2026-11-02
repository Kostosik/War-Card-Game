using System.Collections.ObjectModel;

namespace GameProject.Combat.Army;

public static class DetachmentCatalog
{
    public static ReadOnlyCollection<DetachmentDefinition> CreateInitialRoster()
    {
        return Array.AsReadOnly(
            new[]
            {
                new DetachmentDefinition(
                    "infantry",
                    "Пехота",
                    100,
                    24),

                new DetachmentDefinition(
                    "mechanized",
                    "Механизированные войска",
                    120,
                    20),

                new DetachmentDefinition(
                    "artillery",
                    "Артиллерия",
                    90,
                    28)
            });
    }
}