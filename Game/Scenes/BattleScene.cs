
using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Time;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using GameProject.Combat;
using GameProject.Game.Campaign;
using GameProject.Game.Flow;
using GameProject.Game.Operations;
using GameProject.Game.Scenes.Modals;
using GameProject.Combat.Battlefield;

namespace GameProject.Game.Scenes;

internal sealed class BattleScene : IGameScene
{
    private sealed record EndTurnRequestedEvent : IEvent;
    private sealed record ReturnToHeadquartersRequestedEvent : IEvent;
    private Action<IGameScene>? _requestScene;
    private readonly CampaignState _campaign;
    private UiLabel? _leftFlankInfoLabel;
    private UiLabel? _centerInfoLabel;
    private UiLabel? _rightFlankInfoLabel;
    private GameSceneContext? _context;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _battleUiInstance;
    private EventSubscription? _endTurnSubscription;
    private BattleVictoryModal? _victoryModal;
    private BattleDefeatModal? _defeatModal;

    private bool _resultModalShown;
    private BattlefieldState? _battlefield;

    private UiLabel? _battleNameLabel;
    private UiLabel? _battleInfoLabel;

    public BattleScene(CampaignState campaign)
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

        if (_campaign.CurrentOperation is null)
        {
            throw new InvalidOperationException(
                "Battle cannot start without an active operation.");
        }

        _requestScene = requestScene;

        var ui = context.CreateUiSystem();
        _ui = ui;

        _uiActions = new UiActionRuntime(
            context.Runtime.Events,
            context.UiResources);

        _uiActions.Register(
            "EndTurn",
            () => new EndTurnRequestedEvent());

        _uiActions.Register(
    "ReturnToHeadquarters",
    () => new ReturnToHeadquartersRequestedEvent());

        var battleUiAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/BattleUI.ui"));

        _victoryModal = new BattleVictoryModal(
            context,
            ui,
            _uiActions,
            ReturnToHeadquarters);

        _defeatModal = new BattleDefeatModal(
            context,
            ui,
            _uiActions,
            ReturnToHeadquarters);

        _battleUiInstance = _uiActions.Load(battleUiAsset);

        _battleNameLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "BattleName");

        _battleInfoLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "BattleInfo");

        _leftFlankInfoLabel = FindLabel(
    battleUiAsset,
    _battleUiInstance,
    "LeftFlankInfo");

        _centerInfoLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "CenterInfo");

        _rightFlankInfoLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "RightFlankInfo");

        _battlefield = BattlefieldFactory.CreatePrototype();

        RefreshBattleUi();

        ui.Root.AddChild(_battleUiInstance.Root);

        context.Host.Resized += ui.Resize;

        Console.WriteLine("Battle scene initialized.");
    }

    public void Update(TimeSnapshot time)
    {
        _ui?.Update(time.Delta.TotalSeconds);

        RefreshBattleUi();
    }

    public void Render(double interpolationAlpha)
    {
        _ui?.Render();
    }

    public void Dispose()
    {
        _endTurnSubscription?.Dispose();
        _endTurnSubscription = null;

        if (_context is not null && _ui is not null)
        {
            _context.Host.Resized -= _ui.Resize;

            if (_battleUiInstance is not null)
            {
                _ui.Root.RemoveChild(_battleUiInstance.Root);
            }
        }


        _victoryModal?.Dispose();
        _victoryModal = null;

        _defeatModal?.Dispose();
        _defeatModal = null;
        _battleUiInstance = null;

        _uiActions?.Dispose();
        _uiActions = null;
        _leftFlankInfoLabel = null;
        _centerInfoLabel = null;
        _rightFlankInfoLabel = null;
        _battleNameLabel = null;
        _battleInfoLabel = null;
        _ui = null;
        _context = null;
        _victoryModal = null;
        _requestScene = null;
        _battlefield = null;
        Console.WriteLine("Battle scene disposed.");
    }

    
    private void ReturnToHeadquarters(CombatResult result)
    {
        var outcome = result switch
        {
            CombatResult.PlayerVictory => OperationOutcome.Victory,
            CombatResult.PlayerDefeat => OperationOutcome.Defeat,

            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result,
                "Cannot complete an operation with an unfinished combat.")
        };

        _campaign.CompleteCurrentOperation(outcome);

        _requestScene?.Invoke(
            new HeadquartersScene(_campaign));
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



    private void RefreshBattleUi()
    {
        if (_battlefield is null ||
            _battleNameLabel is null ||
            _battleInfoLabel is null ||
            _leftFlankInfoLabel is null ||
            _centerInfoLabel is null ||
            _rightFlankInfoLabel is null)
        {
            return;
        }

        var battlefield = _battlefield;

        _battleNameLabel.Text =
            $"OPERATION {_campaign.CurrentOperationNumber} / " +
            $"{_campaign.RegularOperationCount}";

        _battleInfoLabel.Text =
            $"STEP {battlefield.StepNumber} | " +
            $"CONTROL: PLAYER {battlefield.PlayerControlledDirections}/2 | " +
            $"ENEMY {battlefield.EnemyControlledDirections}/2";

        _leftFlankInfoLabel.Text =
    FormatDirection(
        BattleDirection.LeftFlank,
        battlefield.LeftFlank);

        _centerInfoLabel.Text =
            FormatDirection(
                BattleDirection.Center,
                battlefield.Center);

        _rightFlankInfoLabel.Text =
            FormatDirection(
                BattleDirection.RightFlank,
                battlefield.RightFlank);

        if (_resultModalShown)
            return;

        switch (battlefield.Result)
        {
            case BattlefieldResult.PlayerVictory:
                _resultModalShown = true;
                _victoryModal?.Show();
                break;

            case BattlefieldResult.PlayerDefeat:
                _resultModalShown = true;
                _defeatModal?.Show();
                break;
        }
    }

    private static string FormatDirection(
    BattleDirection direction,
    BattleDirectionState state)
    {
        var control = state.PlayerDetachment.IsCombatReady &&
                      !state.EnemyDetachment.IsCombatReady
            ? "PLAYER"
            : !state.PlayerDetachment.IsCombatReady &&
              state.EnemyDetachment.IsCombatReady
                ? "ENEMY"
                : "NONE";

        return
            $"{direction}: " +
            $"PLAYER {state.PlayerDetachment.Health}/{state.PlayerDetachment.MaxHealth} | " +
            $"ENEMY {state.EnemyDetachment.Health}/{state.EnemyDetachment.MaxHealth} | " +
            $"CONTROL: {control}";
    }
}
