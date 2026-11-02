using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using Engine.Core.Math;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;
using GameProject.Game.Operations;

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
    private UiCanvas? _choiceCanvas;
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
        _ui.Root.AddChild(_headquartersInstance.Root);
        RefreshCampaignUi();
        CampaignSaveStore.Save(_campaign);
        _startOperationSubscription =
            context.Runtime.Events.Subscribe<StartOperationRequestedEvent>(
                _ =>
                {
                    if (_campaign.Stage == CampaignStage.FinalOperationAvailable)
                    {
                        var finalOperation = FinalOperationCatalog.Create(
                            _campaign.WarPosition);

                        _requestScene?.Invoke(
                            new ArmyDeploymentScene(
                                _campaign,
                                finalOperation));

                        return;
                    }

                    if (_campaign.AreRegularOperationsCompleted)
                        return;

                    var operation = _campaign.CurrentOperation?.Definition;

                    if (operation is null)
                    {
                        if (_campaign.CompletedRegularOperations != 0)
                            return;

                        operation = OperationCatalog.FirstOperation;
                    }

                    _requestScene?.Invoke(
                        new ArmyDeploymentScene(_campaign, operation));
                });



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

        if (_ui is not null && _choiceCanvas is not null)
        {
            _ui.Root.RemoveChild(_choiceCanvas);
        }

        _choiceCanvas = null;

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

        EnsureOperationChoices();

        _situationDescriptionLabel.Text =
            $"Обычные операции: " +
            $"{_campaign.CompletedRegularOperations}/" +
            $"{_campaign.RegularOperationCount}\n" +
            $"Осталось операций: " +
            $"{_campaign.RemainingRegularOperations}";
        _situationDescriptionLabel.Text +=
    $"\nПоложение на войне: {_campaign.WarPositionDescription}";
        var choices = _campaign.PendingOperationChoices;

        if (_campaign.Stage == CampaignStage.FinalOperationAvailable)
        {
            _situationDescriptionLabel.Text +=
                "\n\nФИНАЛЬНАЯ ОПЕРАЦИЯ ДОСТУПНА." +
                "\nКомандование приказывает захватить штаб противника." +
                $"\nПоложение на войне: {_campaign.WarPositionDescription}";

            _startOperationButton.Enabled = true;

            return;
        }

        if (_campaign.LatestEvent is { } campaignEvent)
        {
            _situationDescriptionLabel.Text +=
                $"\n\nСОБЫТИЯ НА ДРУГИХ ФРОНТАХ\n" +
                $"{campaignEvent.Title}\n" +
                $"{campaignEvent.Description}";
        }

        if (choices is not null)
        {
            _situationDescriptionLabel.Text +=
                "\nШтаб подготовил два приказа. Выберите следующий.";

            _startOperationButton.Enabled = false;

            BuildOperationChoiceUi(choices);

            return;
        }

        _startOperationButton.Enabled =
            _campaign.CurrentOperation is not null ||
            _campaign.CompletedRegularOperations == 0;

        if (_campaign.AreRegularOperationsCompleted)
        {
            _situationDescriptionLabel.Text +=
                "\nВсе обычные операции завершены.";
        }
    }

    private void EnsureOperationChoices()
    {
        if (_campaign.PendingOperationChoices is not null ||
            _campaign.CurrentOperation is not null)
        {
            return;
        }

        var choices = _campaign.CompletedRegularOperations switch
        {
            1 => OperationChoiceCatalog.AfterFirstOperation,
            2 => OperationChoiceCatalog.AfterSecondOperation,
            _ => null
        };

        if (choices is not null)
        {
            _campaign.SetNextOperationChoices(choices);
        }
    }

    private void BuildOperationChoiceUi(OperationChoiceSet choices)
    {
        _choiceCanvas = new UiCanvas();

        var title = new UiLabel("ВЫБЕРИТЕ СЛЕДУЮЩИЙ ПРИКАЗ")
        {
            Width = 800,
            Height = 45,
            FontSize = 24,
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = UiAnchor.TopCenter,
            Offset = new Vector2(0, 180)
        };

        _choiceCanvas.AddChild(title);

        for (var i = 0; i < choices.Offers.Count; i++)
        {
            var offer = choices.Offers[i];
            var offsetX = i == 0 ? -260 : 260;

            var description = new UiLabel(
                $"{offer.Operation.Name}\n\n" +
                $"РИСК: {offer.RiskDescription}\n" +
                $"ОЖИДАЕМЫЙ РЕЗУЛЬТАТ: {offer.ExpectedResult}")
            {
                Width = 480,
                Height = 140,
                FontSize = 15,
                LayoutMode = UiLayoutMode.Anchor,
                Anchor = UiAnchor.Center,
                Offset = new Vector2(offsetX, 0)
            };

            _choiceCanvas.AddChild(description);

            var button = new UiButton("ВЫБРАТЬ ПРИКАЗ")
            {
                Width = 280,
                Height = 52,
                FontSize = 16,
                LayoutMode = UiLayoutMode.Anchor,
                Anchor = UiAnchor.Center,
                Offset = new Vector2(offsetX, 125)
            };

            button.Clicked += () => SelectOperation(offer);

            _choiceCanvas.AddChild(button);
        }

        _ui!.Root.AddChild(_choiceCanvas);
    }

    private void SelectOperation(OperationOffer offer)
    {
        if (_campaign.PendingOperationChoices is null ||
            _campaign.CurrentOperation is not null)
        {
            return;
        }

        var operation = _campaign.SelectNextOperation(
            offer.Operation.Id);
        CampaignSaveStore.Save(_campaign);
        _requestScene?.Invoke(
            new ArmyDeploymentScene(
                _campaign,
                operation.Definition));
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