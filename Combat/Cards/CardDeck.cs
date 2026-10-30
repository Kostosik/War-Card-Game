using System.Collections.ObjectModel;

namespace GameProject.Combat.Cards;

public sealed class CardDeck
{
    private readonly List<CardDefinition> _drawPile;

    private readonly List<CardDefinition> _hand = [];

    private readonly List<CardDefinition> _discardPile = [];

    private readonly ReadOnlyCollection<CardDefinition> _handView;

    private readonly Random _random;

    public IReadOnlyList<CardDefinition> Hand => _handView;

    public int DrawPileCount => _drawPile.Count;

    public int DiscardPileCount => _discardPile.Count;

    public CardDeck(
        IEnumerable<CardDefinition> cards,
        Random? random = null)
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

        _random = random ?? Random.Shared;
    }

    public int DrawCards(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var drawn = 0;

        while (drawn < count)
        {
            if (_drawPile.Count == 0)
            {
                RefillDrawPile();
            }

            if (_drawPile.Count == 0)
            {
                break;
            }

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
        {
            return;
        }

        _drawPile.AddRange(_discardPile);

        _discardPile.Clear();

        for (var i = _drawPile.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);

            (_drawPile[i], _drawPile[j]) =
                (_drawPile[j], _drawPile[i]);
        }
    }
}