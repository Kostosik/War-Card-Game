
using Engine.Hosting.Desktop;
using Engine.Networking.Replication;
using Engine.Runtime;
using Engine.Serialization.SaveLoad.Ecs;
using Engine.Serialization.SaveLoad.Worlds;
using GameProject.Game.Flow;
using GameProject.Game.Scenes;

namespace GameProject;

internal sealed class GameMain : EngineGame
{
    private readonly DesktopGameHostContext _host;
    private readonly string _startupScenePath;

    private GameSceneContext? _sceneContext;
    private GameSceneManager? _sceneManager;

    public GameMain(
        DesktopGameHostContext context,
        string startupScenePath)
        : base(
            (context ?? throw new ArgumentNullException(nameof(context)))
            .Runtime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startupScenePath);

        _host = context;
        _startupScenePath = Path.GetFullPath(startupScenePath);
    }

    protected override void ConfigureRuntime()
    {
        if (!File.Exists(_startupScenePath))
        {
            throw new FileNotFoundException(
                "Startup scene was not found.",
                _startupScenePath);
        }

        var serializers = new EcsComponentSerializerRegistry();

        serializers.RegisterEngineComponents();
        serializers.RegisterNetworkComponents();

        var saveLoad = new WorldSaveLoadService(serializers);

        saveLoad.Load(
            _startupScenePath,
            Runtime.World);
    }

    protected override void OnInitialize()
    {
        _sceneContext = new GameSceneContext(_host);

        _sceneManager = new GameSceneManager(_sceneContext);

        _sceneManager.Start(new MainMenuScene());
    }

    protected override void OnUpdate(Engine.Core.Time.TimeSnapshot time)
    {
        _sceneManager?.Update(time);
    }

    protected override void OnRender(double interpolationAlpha)
    {
        _sceneManager?.Render(interpolationAlpha);
    }

    protected override void OnShutdown()
    {
        _sceneManager?.Dispose();
        _sceneManager = null;

        _sceneContext?.Dispose();
        _sceneContext = null;
    }
}
