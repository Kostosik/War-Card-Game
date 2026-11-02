using System.Text.Json;

namespace GameProject.Game.Campaign;

internal static class CampaignSaveStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "WarCardGame",
        "campaign.json");

    public static bool HasSave => File.Exists(SavePath);

    public static CampaignState? Load()
    {
        if (!File.Exists(SavePath))
            return null;

        try
        {
            var data = JsonSerializer.Deserialize<CampaignSaveData>(
                File.ReadAllText(SavePath));

            if (data is null ||
                data.Version != CampaignSaveData.CurrentVersion)
            {
                return null;
            }

            return CampaignState.Restore(data);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static void Save(CampaignState campaign)
    {
        var directory = Path.GetDirectoryName(SavePath)!;

        Directory.CreateDirectory(directory);

        File.WriteAllText(
            SavePath,
            JsonSerializer.Serialize(
                campaign.CreateSaveData(),
                JsonOptions));
    }
}