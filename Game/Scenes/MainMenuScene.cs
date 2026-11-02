
using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class MainMenuScene : IGameScene
{
    private sealed record ExitGameRequestedEvent : IEvent;
    private sealed record StartGameRequestedEvent : IEvent;
    private sealed record OpenSettingsRequestedEvent : IEvent;
    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;
    private UiButton? _continueCampaignButton;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _menuInstance;
    private EventSubscription? _exitGameSubscription;
    private EventSubscription? _startGameSubscription;
    private EventSubscription? _openSettingsSubscription;
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
            "ExitGame",
            () => new ExitGameRequestedEvent());
        _uiActions.Register(
            "StartGame",
            () => new StartGameRequestedEvent());
        _uiActions.Register(
            "OpenSettings",
            () => new OpenSettingsRequestedEvent());
        var menuAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/MainMenu.ui"));

        _menuInstance = _uiActions.Load(menuAsset);

        var startButtonElement = menuAsset.Elements.Single(
    element => element.Name == "StartButton");

        if (_menuInstance.Get(startButtonElement.Id) is UiButton startButton)
        {
            startButton.Text = "NEW CAMPAIGN";
        }

        var savedCampaign = CampaignSaveStore.Load();

        if (savedCampaign is not null &&
            savedCampaign.Stage != CampaignStage.Completed)
        {
            _continueCampaignButton = new UiButton("CONTINUE")
            {
                Width = 200,
                Height = 40,
                FontSize = 16,
                LayoutMode = UiLayoutMode.Anchor,
                Anchor = new UiAnchor(0.5f, 0.48f),
                ZIndex = 20
            };

            _continueCampaignButton.Clicked += () =>
                _requestScene?.Invoke(
                    new HeadquartersScene(savedCampaign));

            _menuInstance.Root.AddChild(_continueCampaignButton);
        }

        _exitGameSubscription =
    context.Runtime.Events.Subscribe<ExitGameRequestedEvent>(
        _ => context.Host.Close());
        _startGameSubscription =
            context.Runtime.Events.Subscribe<StartGameRequestedEvent>(
                _ =>
                {
                    _requestScene?.Invoke(new HeadquartersScene(new CampaignState()));
                });
        _openSettingsSubscription =
    context.Runtime.Events.Subscribe<OpenSettingsRequestedEvent>(
        _ =>
        {
            _requestScene?.Invoke(new SettingsScene());
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
            if (_continueCampaignButton is not null)
            {
                _menuInstance.Root.RemoveChild(_continueCampaignButton);
            }
            if (_menuInstance is not null)
            {
                _ui.Root.RemoveChild(_menuInstance.Root);
            }
        }
        _openSettingsSubscription?.Dispose();
        _openSettingsSubscription = null;
        _uiActions?.Dispose();
        _uiActions = null;
        _exitGameSubscription?.Dispose();
        _exitGameSubscription = null;
        _menuInstance = null;
        _ui = null;
        _context = null;
        _requestScene = null;
        _continueCampaignButton = null;
        Console.WriteLine("Main menu disposed.");
    }
}
