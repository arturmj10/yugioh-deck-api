namespace YugiohDeck.API.Models.Dto;

public class DeckSaveDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    
    public DeckConfigurationDto Configuration { get; set; } = new();
}