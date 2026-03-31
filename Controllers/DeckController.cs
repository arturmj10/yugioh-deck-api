using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YugiohDeck.API.Data;
using YugiohDeck.API.Models;

namespace YugiohDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    private readonly AppDbContext _context;

    // GET: api/decks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Deck>>> GetDecks()
    {
        return await _context.Decks
            .Include(d => d.Configuration)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync();
    }

    // GET: api/decks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Deck>> GetDeck(int id)
    {
        var deck = await _context.Decks
            .Include(d => d.Configuration)
            .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card) // Traz os dados da carta também
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null) return NotFound();

        return deck;
    }

    // POST: api/decks
    [HttpPost]
    public async Task<ActionResult<Deck>> PostDeck(Deck deck)
    {
        deck.Configuration = new DeckConfiguration
        {
            Formato = "TCG",
            CorTema = "#3f51b5"
        };
        

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, deck);
    }

    // DELETE: api/decks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var deck = await _context.Decks.FindAsync(id);
        if (deck == null) return NotFound();

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}