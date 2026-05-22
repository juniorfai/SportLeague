using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;

namespace SportsLeague.Domain.Interfaces.Services
{
    public interface ITournamentService
    {
        Task<IEnumerable<Tournament>> GetAllAsync();
        Task<Tournament?> GetByIdAsync(int id);
        Task<Tournament> CreateAsync(Tournament tournament);
        Task UpdateAsync(int id, Tournament tournament);
        Task DeleteAsync(int id);
        Task UpdateStatusAsync(int id, TournamentStatus newStatus); //Método para cambiar el status de un torneo
        Task RegisterTeamAsync(int tournamentId, int teamId); // para registrar un equipo en un torneo
        Task<IEnumerable<Team>> GetTeamsByTournamentAsync(int tournamentId); // para obtener los equipos registrados en un torneo
    }
}
