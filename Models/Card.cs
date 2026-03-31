using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YugiohDeck.API.Models;

public class Card
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)] 
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? FrameType { get; set; }

    public string? Desc { get; set; }

    public string? Race { get; set; }

    public string? Attribute { get; set; }

    public int? Atk { get; set; }

    public int? Def { get; set; }

    public int? Level { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();
}