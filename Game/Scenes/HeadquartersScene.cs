using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Core;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class HeadquartersScene : IGameScene
{
    private sealed record StartOperationRequestedEvent : IEvent;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _headquartersInstance;

    private EventSubscription? _startOperationSubscription;

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;
        _requestScene = requestScene;

        _ui = context.CreateUiSystem();

        _uiActions = new UiActionRuntime(
            context.Runtime.Events,
            context.UiResources);

        _uiActions.Register(
            "StartOperation",
            () => new StartOperationRequestedEvent());

        var headquartersAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/Headquarters.ui"));

        _headquartersInstance = _uiActions.Load(headquartersAsset);

        _startOperationSubscription =
            context.Runtime.Events.Subscribe<StartOperationRequestedEvent>(
                _ =>
                {
                    _requestScene?.Invoke(new BattleScene());
                });

        _ui.Root.AddChild(_headquartersInstance.Root);

        context.Host.Resized += _ui.Resize;

        Console.WriteLine("Headquarters initialized.");
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
        _startOperationSubscription?.Dispose();
        _startOperationSubscription = null;

        if (_context is not null && _ui is not null)
        {
            _context.Host.Resized -= _ui.Resize;

            if (_headquartersInstance is not null)
            {
                _ui.Root.RemoveChild(_headquartersInstance.Root);
            }
        }

        _uiActions?.Dispose();
        _uiActions = null;

        _headquartersInstance = null;
        _ui = null;
        _context = null;
        _requestScene = null;

        Console.WriteLine("Headquarters disposed.");
    }
}