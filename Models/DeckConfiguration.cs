using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YugiohDeck.API.Models;

public class DeckConfiguration
{
    public DeckConfiguration()
    {
        Formato = "TCG";
        CorTema = "#3f51b5";
    }
    
    [Key, ForeignKey("Deck")]
    public int DeckId { get; set; }
    public virtual Deck Deck { get; set; } = null!;

    public string Formato { get; set; }

    public string CorTema { get; set; }

    public int? CapaCardId { get; set; }
}