namespace YugiohDeck.API.Models;

public class DeckCard
{
    public int Id { get; set; }

    public int DeckId { get; set; }
    public virtual Deck Deck { get; set; } = null!;

    public int CardId { get; set; }
    public virtual Card Card { get; set; } = null!;

    public string Slot { get; set; } = "Main";

    public int Quantidade { get; set; } = 1;
}