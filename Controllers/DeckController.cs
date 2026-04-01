using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YugiohDeck.API.Data;
using YugiohDeck.API.Models;
using YugiohDeck.API.Models.Dto;

namespace YugiohDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DecksController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/decks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeckReadDto>>> GetDecks()
    {
        var decks = await _context.Decks
            .Include(d => d.Configuration)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync();

        // Mapeia a lista de entidades para uma lista de DTOs
        return Ok(_mapper.Map<IEnumerable<DeckReadDto>>(decks));
    }

    // GET: api/decks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DeckReadDto>> GetDeck(int id)
    {
        var deck = await _context.Decks
            .Include(d => d.Configuration)
            .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null) return NotFound();

        return Ok(_mapper.Map<DeckReadDto>(deck));
    }

    // POST: api/decks
    [HttpPost]
    public async Task<ActionResult<DeckReadDto>> PostDeck([FromBody] DeckSaveDto deckDto)
    {
        // Converte o DTO para a Entidade
        var deck = _mapper.Map<Deck>(deckDto);

        // A configuração padrão já pode vir do DTO ou ser setada aqui
        if (deck.Configuration == null)
        {
            deck.Configuration = new DeckConfiguration(); // Usa os padrões do construtor
        }

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Mapeia de volta para o DTO de leitura para retornar ao usuário
        var deckRead = _mapper.Map<DeckReadDto>(deck);
        return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, deckRead);
    }

    // PUT: api/decks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDeck(int id, [FromBody] DeckSaveDto deckDto)
    {
        // 1. Busca o deck incluindo a configuração
        var deckExistente = await _context.Decks
            .Include(d => d.Configuration)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deckExistente == null) return NotFound();

        // 2. A MÁGICA: Atualiza o deckExistente com os dados do DTO
        _mapper.Map(deckDto, deckExistente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatusCode(500, "Erro ao atualizar o banco.");
        }

        return NoContent();
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