using System.Collections.ObjectModel;
using Engine.Core.Determinism;

namespace GameProject.Combat.Cards;

public sealed class CardDeck
{
    private readonly List<CardDefinition> _drawPile;
    private readonly List<CardDefinition> _hand = [];
    private readonly List<CardDefinition> _discardPile = [];

    private readonly ReadOnlyCollection<CardDefinition> _handView;

    private readonly Random? _random;

    private DeterministicRandom _deterministicRandom;
    private readonly bool _useDeterministicRandom;

    public IReadOnlyList<CardDefinition> Hand => _handView;

    public int DrawPileCount => _drawPile.Count;

    public int DiscardPileCount => _discardPile.Count;

    // Старый вариант сохраняем.
    public CardDeck(
        IEnumerable<CardDefinition> cards,
        Random? random = null)
        : this(
            cards,
            random ?? Random.Shared,
            default,
            false)
    {
    }

    // Новый вариант с генератором движка.
    public CardDeck(
        IEnumerable<CardDefinition> cards,
        DeterministicRandom random)
        : this(
            cards,
            null,
            random,
            true)
    {
    }

    private CardDeck(
        IEnumerable<CardDefinition> cards,
        Random? random,
        DeterministicRandom deterministicRandom,
        bool useDeterministicRandom)
    {
        ArgumentNullException.ThrowIfNull(cards);

        _drawPile = new List<CardDefinition>(cards);

        if (_drawPile.Any(card => card is null))
        {
            throw new ArgumentException(
                "Card collection cannot contain null values.",
                nameof(cards));
        }

        _handView = _hand.AsReadOnly();

        _random = random;
        _deterministicRandom = deterministicRandom;
        _useDeterministicRandom = useDeterministicRandom;
    }

    public void ShuffleDrawPile()
    {
        Shuffle(_drawPile);
    }

    public int DrawCards(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var drawn = 0;

        while (drawn < count)
        {
            if (_drawPile.Count == 0)
                RefillDrawPile();

            if (_drawPile.Count == 0)
                break;

            var index = _drawPile.Count - 1;

            var card = _drawPile[index];

            _drawPile.RemoveAt(index);
            _hand.Add(card);

            drawn++;
        }

        return drawn;
    }

    public CardDefinition DiscardFromHand(int handIndex)
    {
        if ((uint)handIndex >= (uint)_hand.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(handIndex));
        }

        var card = _hand[handIndex];

        _hand.RemoveAt(handIndex);
        _discardPile.Add(card);

        return card;
    }

    public void DiscardHand()
    {
        _discardPile.AddRange(_hand);
        _hand.Clear();
    }

    private void RefillDrawPile()
    {
        if (_discardPile.Count == 0)
            return;

        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();

        Shuffle(_drawPile);
    }

    private void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = NextInt(i + 1);

            (items[i], items[j]) =
                (items[j], items[i]);
        }
    }

    private int NextInt(int maxExclusive)
    {
        return _useDeterministicRandom
            ? _deterministicRandom.NextInt(0, maxExclusive)
            : _random!.Next(maxExclusive);
    }
}