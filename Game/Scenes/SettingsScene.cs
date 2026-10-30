using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using GameProject.Game.Flow;
using GameProject.Game.Settings;

namespace GameProject.Game.Scenes;

internal sealed class SettingsScene : IGameScene
{
    private sealed record SaveSettingsRequestedEvent : IEvent;
    private sealed record BackToMenuRequestedEvent : IEvent;

    private static readonly (int Width, int Height)[] Resolutions =
    {
        (1280, 720),
        (1600, 900),
        (1920, 1080)
    };

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;
    private UiToggle? _fullscreenToggle;
    private UiToggle? _vsyncToggle;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _settingsInstance;

    private UiDropdown? _resolutionDropdown;

    private EventSubscription? _saveSubscription;
    private EventSubscription? _backSubscription;

    private GameSettings? _settings;

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;

        _settings = GameSettingsStore.Load();

        _ui = context.CreateUiSystem();

        _uiActions = new UiActionRuntime(
            context.Runtime.Events,
            context.UiResources);

        _uiActions.Register(
            "SaveSettings",
            () => new SaveSettingsRequestedEvent());

        _uiActions.Register(
            "BackToMenu",
            () => new BackToMenuRequestedEvent());

        var asset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/Settings.ui"));

        _settingsInstance = _uiActions.Load(asset);

        var dropdownElement = asset.Elements.SingleOrDefault(
            element => element.Name == "ResolutionDropdown")
            ?? throw new InvalidDataException(
                "ResolutionDropdown was not found in Settings.ui.");

        _resolutionDropdown =
            _settingsInstance.Get(dropdownElement.Id) as UiDropdown
            ?? throw new InvalidDataException(
                "ResolutionDropdown is not a Dropdown.");

        foreach (var resolution in Resolutions)
        {
            _resolutionDropdown.AddOption(
                $"{resolution.Width} x {resolution.Height}");
        }

        _fullscreenToggle = FindToggle(
    asset,
    _settingsInstance,
    "FullscreenToggle");

        _fullscreenToggle.SetValue(_settings.Fullscreen);

        _vsyncToggle = FindToggle(
            asset,
            _settingsInstance,
            "VSyncToggle");

        _vsyncToggle.SetValue(_settings.VSync);

        var selectedIndex = Array.FindIndex(
            Resolutions,
            resolution =>
                resolution.Width == _settings.WindowWidth &&
                resolution.Height == _settings.WindowHeight);

        _resolutionDropdown.SetSelectedIndex(
            Math.Max(0, selectedIndex));

        _saveSubscription =
            context.Runtime.Events.Subscribe<SaveSettingsRequestedEvent>(
                _ => SaveSettings());

        _backSubscription =
            context.Runtime.Events.Subscribe<BackToMenuRequestedEvent>(
                _ => _requestScene?.Invoke(new MainMenuScene()));

        _ui.Root.AddChild(_settingsInstance.Root);

        context.Host.Resized += _ui.Resize;

        Console.WriteLine("Settings scene initialized.");
    }

    public void Update(TimeSnapshot time)
    {
        _ui?.Update(time.Delta.TotalSeconds);
    }

    public void Render(double interpolationAlpha)
    {
        _ui?.Render();
    }

    public void Dispose()
    {
        _saveSubscription?.Dispose();
        _saveSubscription = null;

        _backSubscription?.Dispose();
        _backSubscription = null;

        if (_context is not null && _ui is not null)
        {
            _context.Host.Resized -= _ui.Resize;

            if (_settingsInstance is not null)
            {
                _ui.Root.RemoveChild(_settingsInstance.Root);
            }
        }

        _uiActions?.Dispose();
        _uiActions = null;

        _settingsInstance = null;
        _resolutionDropdown = null;
        _settings = null;
        _ui = null;
        _context = null;
        _requestScene = null;
        _fullscreenToggle = null;
        _vsyncToggle = null;
    }

    private static UiToggle FindToggle(
    UiAsset asset,
    UiAssetInstance instance,
    string name)
    {
        var element = asset.Elements.SingleOrDefault(
            element => element.Name == name)
            ?? throw new InvalidDataException(
                $"{name} was not found in Settings.ui.");

        return instance.Get(element.Id) as UiToggle
            ?? throw new InvalidDataException(
                $"{name} is not a Toggle.");
    }

    private void SaveSettings()
    {
        if (_settings is null ||
            _resolutionDropdown is null ||
            _fullscreenToggle is null ||
            _vsyncToggle is null)
        {
            return;
        }

        var resolution = Resolutions[
            _resolutionDropdown.SelectedIndex];

        _settings.WindowWidth = resolution.Width;
        _settings.WindowHeight = resolution.Height;

        _settings.Fullscreen = _fullscreenToggle.IsOn;
        _settings.VSync = _vsyncToggle.IsOn;

        _context.Host.ApplyWindowSettings(
            _settings.WindowWidth,
            _settings.WindowHeight,
            _settings.Fullscreen,
            _settings.VSync);

        GameSettingsStore.Save(_settings);

        Console.WriteLine("Settings saved.");
    }
}