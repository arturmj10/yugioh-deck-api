using Microsoft.AspNetCore.Mvc;
using YugiohDeck.API.Models.Dto;
using YugiohDeck.API.Services;

namespace YugiohDeck.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly YgoApiService _ygoApiService;

    public CardsController(YgoApiService ygoApiService)
    {
        _ygoApiService = ygoApiService;
    }

    /// <summary>
    /// Retorna resultados da API externa baseados na string de busca.
    /// Exemplo: GET /api/cards/search?q=Mago Negro
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CardDto>>> SearchCards([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("O termo de busca não pode ser vazio.");

        var cards = await _ygoApiService.SearchCardsAsync(q);

        if (!cards.Any())
            return NotFound("Nenhuma carta encontrada para o termo informado.");

        return Ok(cards);
    }
}