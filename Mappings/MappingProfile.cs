using AutoMapper;
using YugiohDeck.API.Models;
using YugiohDeck.API.Models.Dto;

namespace YugiohDeck.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Card, CardDto>().ReverseMap();

        CreateMap<DeckConfiguration, DeckConfigurationDto>().ReverseMap();

        CreateMap<DeckCard, DeckCardDto>().ReverseMap();
        
        CreateMap<Deck, DeckReadDto>();
        
        CreateMap<DeckSaveDto, Deck>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DataCriacao, opt => opt.Ignore())
            .ForMember(dest => dest.DeckCards, opt => opt.Ignore()); 
    }
}