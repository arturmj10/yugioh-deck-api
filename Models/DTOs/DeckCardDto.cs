namespace YugiohDeck.API.Models.Dto;

public class DeckCardDto
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public string Slot { get; set; } = "Main";
    public int Quantidade { get; set; } = 1;
    
    public CardDto? Card { get; set; } 
}