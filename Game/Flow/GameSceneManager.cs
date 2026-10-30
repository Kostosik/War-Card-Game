using Engine.Core.Time;

namespace GameProject.Game.Flow;

internal sealed class GameSceneManager : IDisposable
{
    private readonly GameSceneContext _context;

    private IGameScene? _activeScene;
    private IGameScene? _pendingScene;
    private bool _disposed;

    public GameSceneManager(GameSceneContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public void Start(IGameScene initialScene)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_activeScene is not null)
        {
            throw new InvalidOperationException(
                "A scene is already active.");
        }

        Activate(initialScene);
    }

    public void RequestScene(IGameScene scene)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(scene);

        if (_pendingScene is not null)
        {
            throw new InvalidOperationException(
                "A scene transition is already pending.");
        }

        _pendingScene = scene;
    }

    public void Update(TimeSnapshot time)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _activeScene?.Update(time);

        if (_pendingScene is null)
        {
            return;
        }

        var nextScene = _pendingScene;
        _pendingScene = null;

        Activate(nextScene);
    }

    public void Render(double interpolationAlpha)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _activeScene?.Render(interpolationAlpha);
    }

    private void Activate(IGameScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        _activeScene?.Dispose();

        _activeScene = scene;

        scene.Initialize(
            _context,
            RequestScene);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _pendingScene?.Dispose();
        _pendingScene = null;

        _activeScene?.Dispose();
        _activeScene = null;
    }
}