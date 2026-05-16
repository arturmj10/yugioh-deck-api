using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YugiohDeck.API.Data;
using YugiohDeck.API.Models;
using YugiohDeck.API.Models.Dto;
using YugiohDeck.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace YugiohDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DecksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly DeckService _deckService;

    public DecksController(AppDbContext context, IMapper mapper, DeckService deckService)
    {
        _context = context;
        _mapper = mapper;
        _deckService = deckService;
    }

    private string? ObterUsuarioLogadoId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeckReadDto>>> GetDecks()
    {
        var userId = ObterUsuarioLogadoId();

        var decks = await _context.Decks
            .Where(d => d.UserId == userId) // FILTRO: Traz apenas os decks do usuário logado
            .Include(d => d.Configuration)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<DeckReadDto>>(decks));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeckReadDto>> GetDeck(int id)
    {
        var userId = ObterUsuarioLogadoId();

        var deck = await _context.Decks
            .Include(d => d.Configuration)
            .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId); // GARANTIA: O deck tem que ser do usuário

        if (deck == null) return NotFound("Deck não encontrado ou você não tem permissão para acessá-lo.");

        await _deckService.UpdateDeckCardsIfNeededAsync(deck);

        return Ok(_mapper.Map<DeckReadDto>(deck));
    }

    [HttpPost]
    public async Task<ActionResult<DeckReadDto>> PostDeck([FromBody] DeckSaveDto deckDto)
    {
        var userId = ObterUsuarioLogadoId();
        if (userId == null) return Unauthorized();

        var deck = _mapper.Map<Deck>(deckDto);
        deck.UserId = userId; // VÍNCULO: Associa o usuário ao deck na hora de salvar

        if (deck.Configuration == null)
        {
            deck.Configuration = new DeckConfiguration();
        }

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        var deckRead = _mapper.Map<DeckReadDto>(deck);
        return CreatedAtAction(nameof(GetDeck), new { id = deck.Id }, deckRead);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutDeck(int id, [FromBody] DeckSaveDto deckDto)
    {
        var userId = ObterUsuarioLogadoId();

        var deckExistente = await _context.Decks
            .Include(d => d.Configuration)
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId); // SEGURANÇA: Impede alterar deck dos outros

        if (deckExistente == null) return NotFound("Deck não encontrado ou acesso negado.");

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var userId = ObterUsuarioLogadoId();

        var deck = await _context.Decks.FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        if (deck == null) return NotFound("Deck não encontrado ou acesso negado.");

        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/cards")]
    public async Task<IActionResult> AddCardToDeck(int id, [FromBody] DeckCardDto dto)
    {
        var userId = ObterUsuarioLogadoId();
        
        bool ehDono = await _context.Decks.AnyAsync(d => d.Id == id && d.UserId == userId);
        if (!ehDono) return NotFound("Deck não encontrado ou acesso negado.");

        var result = await _deckService.AddCardToDeckAsync(id, dto);

        if (!result.Success) return BadRequest(result.Message);

        return Ok(new { message = result.Message });
    }

    [HttpDelete("{id}/cards/{cardId}")]
    public async Task<IActionResult> RemoveCardFromDeck(int id, int cardId, [FromQuery] string slot = "Main")
    {
        var userId = ObterUsuarioLogadoId();
        
        bool ehDono = await _context.Decks.AnyAsync(d => d.Id == id && d.UserId == userId);
        if (!ehDono) return NotFound("Deck não encontrado ou acesso negado.");

        var result = await _deckService.RemoveCardFromDeckAsync(id, cardId, slot);

        if (!result.Success) return NotFound(result.Message);

        return NoContent();
    }
}