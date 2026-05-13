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
                    
                    deckCard.Card.BanlistTcg = updatedCard.BanlistTcg;
                    deckCard.Card.BanlistOcg = updatedCard.BanlistOcg;
                    deckCard.Card.BanlistGoat = updatedCard.BanlistGoat;
                    
                    deckCard.Card.LastUpdate = DateTime.UtcNow;
                    hasUpdates = true;
                }
            }
        }

        if (hasUpdates) await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> AddCardToDeckAsync(int deckId, DeckCardDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.Configuration) 
            .Include(d => d.DeckCards)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null) return (false, "Deck não encontrado.");

        var quantidadeParaAdicionar = dto.Quantidade > 0 ? dto.Quantidade : 1;
        
        var card = await _context.Cards.FindAsync(dto.CardId);
        if (card == null)
        {
            card = await _ygoService.GetCardByIdAsync(dto.CardId);
            if (card == null) return (false, "Carta não encontrada na API oficial.");

            _context.Cards.Add(card);
            await _context.SaveChangesAsync(); 
        }

        string formato = deck.Configuration?.Formato?.ToUpper() ?? "TCG";
        
        var isExtraDeckCard = card.Type.Contains("Fusion") || card.Type.Contains("Synchro") || 
                              card.Type.Contains("XYZ") || card.Type.Contains("Link");
        
        if (isExtraDeckCard && dto.Slot == "Main")
            return (false, "Cartas de Fusão/Synchro/XYZ/Link devem ir para o Extra Deck.");
        
        var totalNoSlot = deck.DeckCards.Where(c => c.Slot == dto.Slot).Sum(c => c.Quantidade);
        var slotValidation = ValidarLimitesDoSlot(formato, dto.Slot, totalNoSlot, quantidadeParaAdicionar);
        if (!slotValidation.Success) return slotValidation;
        
        var copiasExistentes = deck.DeckCards.Where(c => c.CardId == dto.CardId).Sum(c => c.Quantidade);
        var banlistValidation = ValidarRegrasDeBanlist(formato, card, copiasExistentes, quantidadeParaAdicionar);
        if (!banlistValidation.Success) return banlistValidation;
        
        var deckCardExistente = deck.DeckCards.FirstOrDefault(c => c.CardId == dto.CardId && c.Slot == dto.Slot);

        if (deckCardExistente != null)
            deckCardExistente.Quantidade += quantidadeParaAdicionar;
        else
            deck.DeckCards.Add(new DeckCard { DeckId = deckId, CardId = card.Id, Slot = dto.Slot, Quantidade = quantidadeParaAdicionar });

        await _context.SaveChangesAsync();
        return (true, "Carta adicionada com sucesso.");
    }

    public async Task<(bool Success, string Message)> RemoveCardFromDeckAsync(int deckId, int cardId, string slot)
    {
        var deckCard = await _context.DeckCards
            .FirstOrDefaultAsync(dc => dc.DeckId == deckId && dc.CardId == cardId && dc.Slot == slot);

        if (deckCard == null) return (false, "Carta não encontrada neste slot do deck.");

        if (deckCard.Quantidade > 1) deckCard.Quantidade--; 
        else _context.DeckCards.Remove(deckCard); 

        await _context.SaveChangesAsync();
        return (true, "Carta removida com sucesso.");
    }

    private (bool Success, string Message) ValidarLimitesDoSlot(string formato, string slot, int totalAtual, int quantidadeAdicionada)
    {
        int mainMax = 60;
        int extraMax = 15;
        int sideMax = 15;

        if (slot == "Main" && (totalAtual + quantidadeAdicionada) > mainMax) 
            return (false, $"O Main Deck atingiu o limite de {mainMax} cartas para o formato {formato}.");
            
        if (slot == "Extra" && (totalAtual + quantidadeAdicionada) > extraMax) 
            return (false, $"O Extra Deck atingiu o limite de {extraMax} cartas para o formato {formato}.");
            
        if (slot == "Side" && (totalAtual + quantidadeAdicionada) > sideMax) 
            return (false, $"O Side Deck atingiu o limite de {sideMax} cartas para o formato {formato}.");

        return (true, string.Empty);
    }

    private (bool Success, string Message) ValidarRegrasDeBanlist(string formato, Card card, int copiasAtuais, int quantidadeAdicionada)
    {
        int limiteCopias = 3; 
        string? statusBanlist = null;
        
        if (formato == "TCG") statusBanlist = card.BanlistTcg;
        else if (formato == "OCG") statusBanlist = card.BanlistOcg;
        else if (formato == "GOAT") statusBanlist = card.BanlistGoat;

        if (statusBanlist == "Banned" || statusBanlist == "Forbidden") limiteCopias = 0;
        else if (statusBanlist == "Limited") limiteCopias = 1;
        else if (statusBanlist == "Semi-Limited") limiteCopias = 2;

        if ((copiasAtuais + quantidadeAdicionada) > limiteCopias)
        {
            if (limiteCopias == 0) 
                return (false, $"Esta carta é Banida no formato {formato}.");
            else 
                return (false, $"O formato {formato} permite no máximo {limiteCopias} cópias desta carta. (Você já possui {copiasAtuais})");
        }

        return (true, string.Empty);
    }
}