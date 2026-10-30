using GameProject.Combat.Cards;
using GameProject.Combat.Enemies;

namespace GameProject.Combat;

public sealed class CombatTurnController
{
    private readonly CombatState _combat;
    private readonly CardDeck _deck;
    private readonly CombatTurnState _turn;
    private readonly EnemyIntent _initialEnemyIntent;

    private bool _hasStarted;

    public CombatState Combat => _combat;

    public CardDeck Deck => _deck;

    public CombatTurnState Turn => _turn;

    public CombatPhase Phase =>
        !_hasStarted
            ? CombatPhase.NotStarted
            : _combat.IsFinished
                ? CombatPhase.Finished
                : CombatPhase.PlayerTurn;

    public CombatTurnController(
        CombatState combat,
        CardDeck deck,
        CombatTurnState turn,
        EnemyIntent initialEnemyIntent)
    {
        ArgumentNullException.ThrowIfNull(combat);
        ArgumentNullException.ThrowIfNull(deck);
        ArgumentNullException.ThrowIfNull(turn);
        ArgumentNullException.ThrowIfNull(initialEnemyIntent);

        _combat = combat;
        _deck = deck;
        _turn = turn;
        _initialEnemyIntent = initialEnemyIntent;
    }

    public void StartCombat()
    {
        if (_hasStarted)
        {
            throw new InvalidOperationException(
                "Combat has already started.");
        }

        if (_combat.IsFinished)
        {
            throw new InvalidOperationException(
                "Cannot start a finished combat.");
        }

        _combat.SetEnemyIntent(_initialEnemyIntent);

        _hasStarted = true;

        BeginPlayerTurn();
    }

    public bool TryPlayCard(int handIndex)
    {
        EnsureStarted();

        if (_combat.IsFinished)
        {
            return false;
        }

        var card = _deck.Hand[handIndex];

        if (!_turn.TrySpendActionPoints(card.ActionCost))
        {
            return false;
        }

        _deck.DiscardFromHand(handIndex);

        switch (card.Kind)
        {
            case CardKind.Attack:
                _combat.Enemy.ApplyDamage(card.Value);
                break;

            case CardKind.Defense:
                _turn.AddBlock(card.Value);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported card kind: {card.Kind}");
        }

        return true;
    }

    public void EndPlayerTurn()
    {
        EnsureStarted();

        if (_combat.IsFinished)
        {
            return;
        }

        _deck.DiscardHand();

        var intent = _combat.CurrentEnemyIntent
            ?? throw new InvalidOperationException(
                "Enemy intent is not available.");

        switch (intent.Kind)
        {
            case EnemyIntentKind.Attack:
                var damage = _turn.AbsorbDamage(intent.Amount);

                _combat.Player.ApplyDamage(damage);
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported enemy intent: {intent.Kind}");
        }

        if (!_combat.IsFinished)
        {
            BeginPlayerTurn();
        }
    }

    private void BeginPlayerTurn()
    {
        _turn.BeginTurn();

        _deck.DrawCards(5);
    }

    private void EnsureStarted()
    {
        if (!_hasStarted)
        {
            throw new InvalidOperationException(
                "Combat has not started.");
        }
    }
}