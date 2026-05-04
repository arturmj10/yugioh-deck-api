using Microsoft.EntityFrameworkCore;
using YugiohDeck.API.Data;
using YugiohDeck.API.Models;
using YugiohDeck.API.Models.Dto;

namespace YugiohDeck.API.Services;

public class DeckService
{
    private readonly AppDbContext _context;
    private readonly YgoApiService _ygoService;

    public DeckService(AppDbContext context, YgoApiService ygoService)
    {
        _context = context;
        _ygoService = ygoService;
    }

    /// <summary>
    /// Verifica se as cartas do deck estão desatualizadas (mais de 14 dias) e busca na API externa.
    /// </summary>
    public async Task UpdateDeckCardsIfNeededAsync(Deck deck)
    {
        bool hasUpdates = false;
        var dataLimite = DateTime.UtcNow.AddDays(-14);

        foreach (var deckCard in deck.DeckCards)
        {
            if (deckCard.Card.LastUpdate < dataLimite)
            {
                var updatedCard = await _ygoService.GetCardByIdAsync(deckCard.CardId);
                if (updatedCard != null)
                {
                    deckCard.Card.Name = updatedCard.Name;
                    deckCard.Card.Desc = updatedCard.Desc;
                    deckCard.Card.Type = updatedCard.Type;
                    deckCard.Card.Atk = updatedCard.Atk;
                    deckCard.Card.Def = updatedCard.Def;
                    deckCard.Card.LastUpdate = DateTime.UtcNow; // Renova a flag
                    hasUpdates = true;
                }
            }
        }

        if (hasUpdates) await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Adiciona cartas ao deck usando Lazy Loading e validando limites.
    /// </summary>
    public async Task<(bool Success, string Message)> AddCardToDeckAsync(int deckId, DeckCardDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.DeckCards)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null) return (false, "Deck não encontrado.");

        var quantidadeParaAdicionar = dto.Quantidade > 0 ? dto.Quantidade : 1;

        // LAZY LOADING
        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
        {
            card = await _ygoService.GetCardByIdAsync(dto.CardId);
            if (card == null) return (false, "Carta não encontrada na API oficial.");

            _context.Cards.Add(card);
            await _context.SaveChangesAsync(); 
        }

        // VALIDAÇÕES YU-GI-OH
        var isExtraDeckCard = card.Type.Contains("Fusion") || card.Type.Contains("Synchro") || 
                              card.Type.Contains("XYZ") || card.Type.Contains("Link");
        
        if (isExtraDeckCard && dto.Slot == "Main")
            return (false, "Cartas de Fusão/Synchro/XYZ/Link devem ir para o Extra Deck.");

        var totalNoSlot = deck.DeckCards.Where(c => c.Slot == dto.Slot).Sum(c => c.Quantidade);
        if (dto.Slot == "Main" && (totalNoSlot + quantidadeParaAdicionar) > 60) return (false, "O Main Deck ultrapassará o limite de 60 cartas.");
        if (dto.Slot == "Extra" && (totalNoSlot + quantidadeParaAdicionar) > 15) return (false, "O Extra Deck ultrapassará o limite de 15 cartas.");
        if (dto.Slot == "Side" && (totalNoSlot + quantidadeParaAdicionar) > 15) return (false, "O Side Deck ultrapassará o limite de 15 cartas.");

        var copiasExistentes = deck.DeckCards.Where(c => c.CardId == dto.CardId).Sum(c => c.Quantidade);
        if ((copiasExistentes + quantidadeParaAdicionar) > 3) return (false, "Você não pode ter mais que 3 cópias desta carta no deck.");

        // INSERE OU ATUALIZA A QUANTIDADE EXISTENTE
        var deckCardExistente = deck.DeckCards.FirstOrDefault(c => c.CardId == dto.CardId && c.Slot == dto.Slot);

        if (deckCardExistente != null)
        {
            deckCardExistente.Quantidade += quantidadeParaAdicionar;
        }
        else
        {
            deck.DeckCards.Add(new DeckCard
            {
                DeckId = deckId,
                CardId = card.Id,
                Slot = dto.Slot,
                Quantidade = quantidadeParaAdicionar
            });
        }

        await _context.SaveChangesAsync();
        return (true, "Carta adicionada com sucesso.");
    }

    /// <summary>
    /// Remove uma cópia da carta do deck. Se chegar a 0, remove o vínculo.
    /// </summary>
    public async Task<(bool Success, string Message)> RemoveCardFromDeckAsync(int deckId, int cardId, string slot)
    {
        var deckCard = await _context.DeckCards
            .FirstOrDefaultAsync(dc => dc.DeckId == deckId && dc.CardId == cardId && dc.Slot == slot);

        if (deckCard == null) return (false, "Carta não encontrada neste slot do deck.");

        if (deckCard.Quantidade > 1)
        {
            deckCard.Quantidade--; // Reduz a quantidade
        }
        else
        {
            _context.DeckCards.Remove(deckCard); // Remove o registro
        }

        await _context.SaveChangesAsync();
        return (true, "Carta removida com sucesso.");
    }
}