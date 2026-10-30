
using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Core;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class MainMenuScene : IGameScene
{
    private sealed record StartGameRequestedEvent : IEvent;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;

    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _menuInstance;

    private EventSubscription? _startGameSubscription;

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
            "StartGame",
            () => new StartGameRequestedEvent());

        var menuAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/MainMenu.ui"));

        _menuInstance = _uiActions.Load(menuAsset);

        _startGameSubscription =
            context.Runtime.Events.Subscribe<StartGameRequestedEvent>(
                _ =>
                {
                    _requestScene?.Invoke(new BattleScene());
                });

        _ui.Root.AddChild(_menuInstance.Root);

        context.Host.Resized += _ui.Resize;

        Console.WriteLine("Main menu initialized.");
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
        _startGameSubscription?.Dispose();
        _startGameSubscription = null;

        if (_context is not null && _ui is not null)
        {
            _context.Host.Resized -= _ui.Resize;

            if (_menuInstance is not null)
            {
                _ui.Root.RemoveChild(_menuInstance.Root);
            }
        }

        _uiActions?.Dispose();
        _uiActions = null;

        _menuInstance = null;
        _ui = null;
        _context = null;
        _requestScene = null;

        Console.WriteLine("Main menu disposed.");
    }
}
