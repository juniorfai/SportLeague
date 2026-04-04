using AutoMapper;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;

namespace SportsLeague.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Team mappings
            CreateMap<TeamRequestDTO, Team>();
            CreateMap<Team, TeamResponseDTO>();

            // Player mappings
            CreateMap<PlayerRequestDTO, Player>();
            CreateMap<Player, PlayerResponseDTO>()
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team.Name));

            // Referee mappings
            CreateMap<RefereeRequestDTO, Referee>();
            CreateMap<Referee, RefereeResponseDTO>();

            // ===== TOURNAMENT MAPPINGS =====
            CreateMap<TournamentRequestDTO, Tournament>();
            CreateMap<Tournament, TournamentResponseDTO>();  // ← Usa TournamentResponseDTO

            // ===== SPONSOR MAPPINGS =====
            CreateMap<SponsorRequestDTO, Sponsor>();
            CreateMap<Sponsor, SponsorResponseDTO>();

            // ===== TOURNAMENT SPONSOR MAPPINGS =====
            CreateMap<TournamentSponsorRequestDTO, TournamentSponsor>();
            CreateMap<TournamentSponsor, TournamentSponsorResponseDTO>()  // ← Usa TournamentSponsorResponseDTO
                .ForMember(dest => dest.TournamentName, opt => opt.MapFrom(src => src.Tournament.Name))
                .ForMember(dest => dest.SponsorName, opt => opt.MapFrom(src => src.Sponsor.Name));
        }
    }
}