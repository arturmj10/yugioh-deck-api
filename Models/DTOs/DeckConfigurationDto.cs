namespace YugiohDeck.API.Models.Dto;

public class DeckConfigurationDto
{
    public string Formato { get; set; } = "TCG";
    public string CorTema { get; set; } = "#3f51b5";
    public int? CapaCardId { get; set; }
}