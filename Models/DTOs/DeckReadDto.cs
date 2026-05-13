namespace YugiohDeck.API.Models.Dto;

public class DeckReadDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
    
    public DeckConfigurationDto Configuration { get; set; } = null!;
    public ICollection<DeckCardDto> DeckCards { get; set; } = new List<DeckCardDto>();
}