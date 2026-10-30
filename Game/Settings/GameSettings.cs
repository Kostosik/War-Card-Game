using System.Text.Json;

namespace GameProject.Game.Settings;

internal sealed class GameSettings
{
    public int WindowWidth { get; set; } = 1280;

    public int WindowHeight { get; set; } = 720;

    public bool Fullscreen { get; set; }

    public bool VSync { get; set; }
}

internal static class GameSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string SettingsPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "WarCardGame",
            "settings.json");

    public static GameSettings Load()
    {
        if (!File.Exists(SettingsPath))
            return new GameSettings();

        try
        {
            return JsonSerializer.Deserialize<GameSettings>(
                File.ReadAllText(SettingsPath))
                ?? new GameSettings();
        }
        catch (JsonException)
        {
            return new GameSettings();
        }
    }

    public static void Save(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(SettingsPath)!;

        Directory.CreateDirectory(directory);

        File.WriteAllText(
            SettingsPath,
            JsonSerializer.Serialize(settings, JsonOptions));
    }
}