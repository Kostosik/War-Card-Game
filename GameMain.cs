using Engine.Core.Assets;
using Engine.Core.Time;
using Engine.Hosting.Desktop;
using Engine.Graphics.Resources;
using Engine.Runtime;
using Engine.Serialization.SaveLoad.Ecs;
using Engine.Serialization.SaveLoad.Worlds;
using Engine.Networking.Replication;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Core;

namespace GameProject;

internal sealed class GameMain : EngineGame
{
    private readonly DesktopGameHostContext _context;
    private readonly string _startupScenePath;

    private TextureResourceManager? _uiResources;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _battleUiInstance;

    public GameMain(
        DesktopGameHostContext context,
        string startupScenePath)
        : base(
            (context ?? throw new ArgumentNullException(nameof(context)))
            .Runtime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            startupScenePath);

        _context = context;

        _startupScenePath =
            Path.GetFullPath(
                startupScenePath);
    }

    protected override void ConfigureRuntime()
    {
        if (!File.Exists(_startupScenePath))
        {
            throw new FileNotFoundException(
                "Startup scene was not found.",
                _startupScenePath);
        }

        var serializers =
            new EcsComponentSerializerRegistry();

        serializers.RegisterEngineComponents();
        serializers.RegisterNetworkComponents();

        var saveLoad =
            new WorldSaveLoadService(serializers);

        saveLoad.Load(
            _startupScenePath,
            Runtime.World);
    }

    protected override void OnInitialize()
    {
        Console.WriteLine(
            $"Startup scene loaded: {_startupScenePath}");

        var resources =
            new TextureResourceManager(
                _context.Content,
                _context.Graphics.Textures);

        _uiResources = resources;

        try
        {
            var ui =
                new UiSystem(
                    _context.Graphics,
                    1280,
                    720,
                    _context.PointerInput,
                    _context.TextInput,
                    _context.Cursor);

            _ui = ui;

            _uiActions =
                new UiActionRuntime(
                    Runtime.Events,
                    resources);

            var battleUiAsset =
                _context.Content.Load<UiAsset>(
                    new AssetPath("UI/BattleUI.ui"));

            _battleUiInstance =
                _uiActions.Load(battleUiAsset);

            ui.Root.AddChild(
                _battleUiInstance.Root);

            _context.Resized += ui.Resize;

            Console.WriteLine(
                $"Battle UI loaded: {battleUiAsset.Name}");
        }
        catch
        {
            _uiActions?.Dispose();
            _uiActions = null;

            resources.Dispose();
            _uiResources = null;

            _ui = null;

            throw;
        }

        Console.WriteLine(
            "VibeEngine2 game initialized.");
    }

    protected override void OnUpdate(TimeSnapshot time)
    {
        _ui?.Update(
            time.Delta.TotalSeconds);
    }

    protected override void OnRender(
        double interpolationAlpha)
    {
        _ui?.Render();
    }

    protected override void OnShutdown()
    {
        if (_ui is not null)
        {
            _context.Resized -= _ui.Resize;

            if (_battleUiInstance is not null)
            {
                _ui.Root.RemoveChild(
                    _battleUiInstance.Root);
            }
        }

        _battleUiInstance = null;
        _ui = null;

        _uiActions?.Dispose();
        _uiActions = null;

        _uiResources?.Dispose();
        _uiResources = null;

        Console.WriteLine(
            "VibeEngine2 game stopped.");
    }
}