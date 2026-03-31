using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace YugiohDeck.API.Models;

public class Deck
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public virtual DeckConfiguration Configuration { get; set; }
    
    public virtual ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();
}