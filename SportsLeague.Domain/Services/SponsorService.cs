using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ITournamentRepository tournamentRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _tournamentRepository = tournamentRepository;
            _logger = logger;
        }

        // ========== CRUD ==========

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllWithTournamentsAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
            var sponsor = await _sponsorRepository.GetByIdWithTournamentsAsync(id);

            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);
            }

            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            // Validación: nombre duplicado
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            {
                _logger.LogWarning("Sponsor with name {Name} already exists", sponsor.Name);
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }

            // Validación: formato de email
            if (!IsValidEmail(sponsor.ContactEmail))
            {
                _logger.LogWarning("Invalid email format: {Email}", sponsor.ContactEmail);
                throw new InvalidOperationException($"El formato del email '{sponsor.ContactEmail}' no es válido");
            }

            _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existingSponsor = await _sponsorRepository.GetByIdAsync(id);
            if (existingSponsor == null)
            {
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");
            }

            // Validación: nombre duplicado (excluyendo el actual)
            if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name, id))
            {
                throw new InvalidOperationException($"Ya existe un patrocinador con el nombre '{sponsor.Name}'");
            }

            // Validación: formato de email
            if (!IsValidEmail(sponsor.ContactEmail))
            {
                throw new InvalidOperationException($"El formato del email '{sponsor.ContactEmail}' no es válido");
            }

            existingSponsor.Name = sponsor.Name;
            existingSponsor.ContactEmail = sponsor.ContactEmail;
            existingSponsor.Phone = sponsor.Phone;
            existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
            existingSponsor.Category = sponsor.Category;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existingSponsor);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.ExistsAsync(id);
            if (!exists)
            {
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {id}");
            }

            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        // ========== Vinculación ==========

        public async Task<TournamentSponsor> LinkSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            // Validación: sponsor existe
            var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
            if (sponsor == null)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", sponsorId);
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");
            }

            // Validación: tournament existe
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
            {
                _logger.LogWarning("Tournament with ID {TournamentId} not found", tournamentId);
                throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");
            }

            // Validación: ContractAmount > 0
            if (contractAmount <= 0)
            {
                _logger.LogWarning("ContractAmount must be greater than 0, received: {Amount}", contractAmount);
                throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");
            }

            // Validación: no duplicar vinculación
            if (await _tournamentSponsorRepository.ExistsByTournamentAndSponsorAsync(tournamentId, sponsorId))
            {
                _logger.LogWarning("Sponsor {SponsorName} already linked to tournament {TournamentName}", sponsor.Name, tournament.Name);
                throw new InvalidOperationException($"El patrocinador '{sponsor.Name}' ya está vinculado al torneo '{tournament.Name}'");
            }

            var tournamentSponsor = new TournamentSponsor
            {
                TournamentId = tournamentId,
                SponsorId = sponsorId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Linking sponsor {SponsorName} to tournament {TournamentName}", sponsor.Name, tournament.Name);
            return await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetSponsorTournamentsAsync(int sponsorId)
        {
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
            {
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", sponsorId);
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");
            }

            _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);
            return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
        }

        public async Task UnlinkSponsorFromTournamentAsync(int sponsorId, int tournamentId)
        {
            // Validación: sponsor existe
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
            {
                throw new KeyNotFoundException($"No se encontró el patrocinador con ID {sponsorId}");
            }

            // Validación: tournament existe
            var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
            if (!tournamentExists)
            {
                throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");
            }

            // Validación: la vinculación existe
            var tournamentSponsor = await _tournamentSponsorRepository.GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (tournamentSponsor == null)
            {
                throw new KeyNotFoundException($"El patrocinador con ID {sponsorId} no está vinculado al torneo con ID {tournamentId}");
            }

            _logger.LogInformation("Unlinking sponsor {SponsorId} from tournament {TournamentId}", sponsorId, tournamentId);
            await _tournamentSponsorRepository.DeleteAsync(tournamentSponsor.Id);
        }

        // ========== Helpers ==========
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }
    }
}
