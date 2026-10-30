using GameProject.Combat.Cards;

namespace WarCardGame.Tests.Combat.Cards;

public sealed class CardDeckTests
{
    [Fact]
    public void DrawCards_MovesCardsIntoHand()
    {
        var cards = CreateCards();

        var deck = new CardDeck(cards);

        var drawn = deck.DrawCards(2);

        Assert.Equal(2, drawn);
        Assert.Equal(2, deck.Hand.Count);
        Assert.Equal(0, deck.DrawPileCount);
        Assert.Equal(0, deck.DiscardPileCount);
    }

    [Fact]
    public void DiscardFromHand_MovesCardToDiscardPile()
    {
        var deck = new CardDeck(CreateCards());

        deck.DrawCards(2);

        var card = deck.DiscardFromHand(0);

        Assert.Single(deck.Hand);
        Assert.Equal(1, deck.DiscardPileCount);
        Assert.Contains(card, CreateCards());
    }

    [Fact]
    public void DiscardHand_MovesAllCardsToDiscardPile()
    {
        var deck = new CardDeck(CreateCards());

        deck.DrawCards(2);

        deck.DiscardHand();

        Assert.Empty(deck.Hand);
        Assert.Equal(2, deck.DiscardPileCount);
    }

    [Fact]
    public void DrawCards_WhenDrawPileIsEmpty_ReshufflesDiscard()
    {
        var deck = new CardDeck(
            CreateCards(),
            new Random(123));

        deck.DrawCards(2);

        deck.DiscardHand();

        var drawn = deck.DrawCards(2);

        Assert.Equal(2, drawn);
        Assert.Equal(2, deck.Hand.Count);
        Assert.Equal(0, deck.DiscardPileCount);
    }

    [Fact]
    public void DrawCards_WithNegativeCount_Throws()
    {
        var deck = new CardDeck(CreateCards());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => deck.DrawCards(-1));
    }

    [Fact]
    public void DiscardFromHand_WithInvalidIndex_Throws()
    {
        var deck = new CardDeck(CreateCards());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => deck.DiscardFromHand(0));
    }

    private static CardDefinition[] CreateCards()
    {
        return
        [
            new CardDefinition(
                "attack",
                CardKind.Attack,
                1,
                6),

            new CardDefinition(
                "defense",
                CardKind.Defense,
                1,
                5)
        ];
    }
}