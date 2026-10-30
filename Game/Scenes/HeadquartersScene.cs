using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class HeadquartersScene : IGameScene
{
    private sealed record StartOperationRequestedEvent : IEvent;

    private GameSceneContext? _context;
    private Action<IGameScene>? _requestScene;
    private readonly CampaignState _campaign;

    private UiLabel? _situationDescriptionLabel;
    private UiButton? _startOperationButton;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _headquartersInstance;

    private EventSubscription? _startOperationSubscription;

    public HeadquartersScene(CampaignState campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        _campaign = campaign;
    }

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

        _situationDescriptionLabel = FindLabel(
    headquartersAsset,
    _headquartersInstance,
    "SituationDescription");

        _startOperationButton = FindButton(
            headquartersAsset,
            _headquartersInstance,
            "StartOperationButton");

        RefreshCampaignUi();

        _startOperationSubscription =
            context.Runtime.Events.Subscribe<StartOperationRequestedEvent>(
                _ =>
                {
                    if (_campaign.AreRegularOperationsCompleted ||
                        _campaign.CurrentOperation is not null)
                    {
                        return;
                    }

                    _campaign.StartNextOperation();

                    _requestScene?.Invoke(
                        new BattleScene(_campaign));
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

    private void RefreshCampaignUi()
    {
        if (_situationDescriptionLabel is null ||
            _startOperationButton is null)
        {
            return;
        }

        _situationDescriptionLabel.Text =
            $"Обычные операции: " +
            $"{_campaign.CompletedRegularOperations}/" +
            $"{_campaign.RegularOperationCount}\n" +
            $"Осталось операций: " +
            $"{_campaign.RemainingRegularOperations}";

        if (_campaign.AreRegularOperationsCompleted)
        {
            _situationDescriptionLabel.Text +=
                "\nВсе обычные операции завершены. " +
                "Финальная операция пока недоступна.";
        }

        _startOperationButton.Enabled =
            !_campaign.AreRegularOperationsCompleted;
    }

    private static UiButton FindButton(
        UiAsset asset,
        UiAssetInstance instance,
        string name)
    {
        var element = asset.Elements.SingleOrDefault(
            element => element.Name == name)
            ?? throw new InvalidDataException(
                $"UI button '{name}' was not found.");

        return instance.Get(element.Id) as UiButton
            ?? throw new InvalidDataException(
                $"UI element '{name}' is not a Button.");
    }

    private static UiLabel FindLabel(
    UiAsset asset,
    UiAssetInstance instance,
    string name)
    {
        var element = asset.Elements.SingleOrDefault(
            element => element.Name == name)
            ?? throw new InvalidDataException(
                $"UI label '{name}' was not found.");

        return instance.Get(element.Id) as UiLabel
            ?? throw new InvalidDataException(
                $"UI element '{name}' is not a Label.");
    }
}