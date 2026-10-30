using Engine.Core.Assets;
using Engine.Core.Time;
using Engine.Graphics.Resources;
using Engine.Hosting.Desktop;
using Engine.Networking.Replication;
using Engine.Runtime;
using Engine.Serialization.SaveLoad.Ecs;
using Engine.Serialization.SaveLoad.Worlds;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using GameProject.Combat;
using GameProject.Combat.Cards;
using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;

namespace GameProject;

internal sealed class GameMain : EngineGame
{
    private readonly DesktopGameHostContext _context;
    private readonly string _startupScenePath;

    private TextureResourceManager? _uiResources;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _battleUiInstance;
    private CombatTurnController? _combatController;
    private UiLabel? _battleNameLabel;
    private UiLabel? _battleInfoLabel;
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

            _battleNameLabel = FindLabel(
                battleUiAsset,
                _battleUiInstance,
                "BattleName");

            _battleInfoLabel = FindLabel(
                battleUiAsset,
                _battleUiInstance,
                "BattleInfo");

            _combatController = CreateInitialCombat();

            RefreshBattleUi();

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
        RefreshBattleUi();
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

    private static CombatTurnController CreateInitialCombat()
    {
        var cards =
            Enumerable.Range(0, 5)
                .Select(i => new CardDefinition(
                    $"defense_{i}",
                    CardKind.Defense,
                    1,
                    5))
                .Concat(
                    Enumerable.Range(0, 5)
                        .Select(i => new CardDefinition(
                            $"attack_{i}",
                            CardKind.Attack,
                            1,
                            6)))
                .ToArray();

        var deck = new CardDeck(cards);

        var combat = new CombatState(
            new CombatantState(60),
            new CombatantState(42));

        var turn = new CombatTurnState(3);

        var controller = new CombatTurnController(
            combat,
            deck,
            turn,
            new EnemyIntent(EnemyIntentKind.Attack, 12));

        controller.StartCombat();

        return controller;
    }

    private void RefreshBattleUi()
    {
        if (_combatController is null ||
            _battleNameLabel is null ||
            _battleInfoLabel is null)
        {
            return;
        }

        var combat = _combatController.Combat;
        var turn = _combatController.Turn;
        var intent = combat.CurrentEnemyIntent;

        _battleNameLabel.Text = "FIRST ENGAGEMENT";

        _battleInfoLabel.Text =
            $"TURN {turn.TurnNumber} | " +
            $"AP {turn.AvailableActionPoints}/{turn.MaxActionPoints} | " +
            $"PLAYER {combat.Player.Health}/{combat.Player.MaxHealth} | " +
            $"ENEMY {combat.Enemy.Health}/{combat.Enemy.MaxHealth} | " +
            $"BLOCK {turn.Block} | " +
            $"INTENT {intent?.Kind} {intent?.Amount}";
    }
}