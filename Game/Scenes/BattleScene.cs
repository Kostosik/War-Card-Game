
using Engine.Core.Assets;
using Engine.Core.Events;
using Engine.Core.Math;
using Engine.Core.Time;
using Engine.Graphics.Resources;
using Engine.UI.Actions;
using Engine.UI.Assets;
using Engine.UI.Controls;
using Engine.UI.Core;
using Engine.UI.Layout;
using GameProject.Combat;
using GameProject.Combat.Cards;
using GameProject.Combat.Combatants;
using GameProject.Combat.Enemies;
using GameProject.Combat.UI;
using GameProject.Game.Flow;

namespace GameProject.Game.Scenes;

internal sealed class BattleScene : IGameScene
{
    private sealed record EndTurnRequestedEvent : IEvent;

    private GameSceneContext? _context;
    private UiSystem? _ui;
    private UiActionRuntime? _uiActions;
    private UiAssetInstance? _battleUiInstance;
    private EventSubscription? _endTurnSubscription;

    private CombatTurnController? _combatController;

    private UiLabel? _battleNameLabel;
    private UiLabel? _battleInfoLabel;
    private UiLabel? _battleResultLabel;

    private CombatPlayAreaDropTarget? _playAreaDropTarget;
    private readonly List<CardView> _handCardViews = new();

    private float _handCardWidth;

    private const float HandCardGap = 18.0f;
    private const float HandCardY = 500.0f;

    public void Initialize(
        GameSceneContext context,
        Action<IGameScene> requestScene)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requestScene);

        _context = context;

        var ui = context.CreateUiSystem();
        _ui = ui;

        _uiActions = new UiActionRuntime(
            context.Runtime.Events,
            context.UiResources);

        _uiActions.Register(
            "EndTurn",
            () => new EndTurnRequestedEvent());

        var battleUiAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("UI/BattleUI.ui"));

        _battleUiInstance = _uiActions.Load(battleUiAsset);

        _battleNameLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "BattleName");

        _battleInfoLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "BattleInfo");

        _battleResultLabel = FindLabel(
            battleUiAsset,
            _battleUiInstance,
            "BattleResult");

        _combatController = CreateInitialCombat();

        CreatePlayAreaDropTarget(battleUiAsset);

        var cardAsset = context.Host.Content.Load<UiAsset>(
            new AssetPath("Cards/Card.ui"));

        CreateHandViews(cardAsset);

        _endTurnSubscription =
            context.Runtime.Events.Subscribe<EndTurnRequestedEvent>(
                _ =>
                {
                    _combatController?.EndPlayerTurn();

                    RefreshBattleUi();
                    RefreshHandUi();
                });

        RefreshBattleUi();

        ui.Root.AddChild(_battleUiInstance.Root);

        context.Host.Resized += ui.Resize;

        Console.WriteLine("Battle scene initialized.");
    }

    public void Update(TimeSnapshot time)
    {
        _ui?.Update(time.Delta.TotalSeconds);

        RefreshBattleUi();
        RefreshHandUi();
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

        _handCardViews.Clear();

        _playAreaDropTarget = null;
        _battleUiInstance = null;

        _uiActions?.Dispose();
        _uiActions = null;

        _combatController = null;
        _battleNameLabel = null;
        _battleInfoLabel = null;
        _battleResultLabel = null;
        _ui = null;
        _context = null;

        Console.WriteLine("Battle scene disposed.");
    }

    private void CreatePlayAreaDropTarget(UiAsset asset)
    {
        var element = asset.Elements.SingleOrDefault(
            element => element.Name == "PlayArea")
            ?? throw new InvalidDataException(
                "PlayArea was not found in BattleUI.");

        var parent = _battleUiInstance!.Get(
            element.ParentId) as UiContainer
            ?? throw new InvalidDataException(
                "PlayArea parent is not a UI container.");

        var target = new CombatPlayAreaDropTarget(TryPlayCard)
        {
            LayoutMode = UiLayoutMode.Anchor,
            Anchor = element.Layout.Anchor,
            Offset = element.Layout.Offset,
            Width = element.Layout.Size.X,
            Height = element.Layout.Size.Y,
            ZIndex = element.ZIndex + 1
        };

        parent.AddChild(target);

        _playAreaDropTarget = target;
    }

    private bool TryPlayCard(CardDefinition definition)
    {
        if (_combatController is null)
        {
            return false;
        }

        var hand = _combatController.Deck.Hand;

        for (var i = 0; i < hand.Count; i++)
        {
            if (hand[i].Id != definition.Id)
            {
                continue;
            }

            if (!_combatController.TryPlayCard(i))
            {
                return false;
            }

            RefreshBattleUi();
            RefreshHandUi();

            return true;
        }

        return false;
    }

    private void CreateHandViews(UiAsset cardAsset)
    {
        var hand = _combatController!.Deck.Hand;

        _handCardWidth = cardAsset.CanvasSize.X;

        var totalWidth =
            hand.Count * _handCardWidth +
            Math.Max(0, hand.Count - 1) * HandCardGap;

        var canvasWidth =
            _battleUiInstance!.Root.Width ?? 1280.0f;

        var startX = (canvasWidth - totalWidth) * 0.5f;

        for (var i = 0; i < hand.Count; i++)
        {
            var instance = _uiActions!.Load(cardAsset);

            var cardView = new CardView(
                cardAsset,
                instance,
                hand[i]);

            _battleUiInstance.Root.AddChild(cardView);

            _battleUiInstance.Root.SetPosition(
                cardView,
                new Vector2(
                    startX + i * (_handCardWidth + HandCardGap),
                    HandCardY));

            _handCardViews.Add(cardView);
        }
    }

    private void RefreshHandUi()
    {
        if (_combatController is null ||
            _battleUiInstance is null)
        {
            return;
        }

        var hand = _combatController.Deck.Hand;

        var totalWidth =
            hand.Count * _handCardWidth +
            Math.Max(0, hand.Count - 1) * HandCardGap;

        var canvasWidth =
            _battleUiInstance.Root.Width ?? 1280.0f;

        var startX = (canvasWidth - totalWidth) * 0.5f;

        for (var i = 0; i < _handCardViews.Count; i++)
        {
            var cardView = _handCardViews[i];

            if (i >= hand.Count)
            {
                cardView.Visible = false;
                continue;
            }

            cardView.Visible = true;
            cardView.SetDefinition(hand[i]);

            _battleUiInstance.Root.SetPosition(
                cardView,
                new Vector2(
                    startX + i * (_handCardWidth + HandCardGap),
                    HandCardY));
        }
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
        deck.ShuffleDrawPile();

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
            _battleInfoLabel is null ||
            _battleResultLabel is null)
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

        _battleResultLabel.Text = combat.Result switch
        {
            CombatResult.PlayerVictory => "VICTORY",
            CombatResult.PlayerDefeat => "DEFEAT",
            _ => string.Empty
        };
    }
}
