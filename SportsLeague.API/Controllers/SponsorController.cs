using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController : ControllerBase
    {
        private readonly ISponsorService _sponsorService;
        private readonly IMapper _mapper;

        public SponsorController(ISponsorService sponsorService, IMapper mapper)
        {
            _sponsorService = sponsorService;
            _mapper = mapper;
        }

        // GET: api/Sponsor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
        {
            var sponsors = await _sponsorService.GetAllAsync();
            var sponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);
            return Ok(sponsorsDto);
        }

        // GET: api/Sponsor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
        {
            var sponsor = await _sponsorService.GetByIdAsync(id);

            if (sponsor == null)
                return NotFound(new { message = $"Patrocinador con ID {id} no encontrado" });

            var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
            return Ok(sponsorDto);
        }

        // POST: api/Sponsor
        [HttpPost]
        public async Task<ActionResult<SponsorResponseDTO>> Create([FromBody] SponsorRequestDTO dto)
        {
            try
            {
                var sponsor = _mapper.Map<Sponsor>(dto);
                var createdSponsor = await _sponsorService.CreateAsync(sponsor);
                var responseDto = _mapper.Map<SponsorResponseDTO>(createdSponsor);

                return CreatedAtAction(nameof(GetById), new { id = responseDto.Id }, responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/Sponsor/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] SponsorRequestDTO dto)
        {
            try
            {
                var sponsor = _mapper.Map<Sponsor>(dto);
                await _sponsorService.UpdateAsync(id, sponsor);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: api/Sponsor/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _sponsorService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/Sponsor/{id}/tournaments
        [HttpGet("{id}/tournaments")]
        public async Task<ActionResult<IEnumerable<TournamentSponsorResponseDTO>>> GetSponsorTournaments(int id)
        {
            try
            {
                var tournamentSponsors = await _sponsorService.GetSponsorTournamentsAsync(id);
                var responseDto = _mapper.Map<IEnumerable<TournamentResponseDTO>>(tournamentSponsors);
                return Ok(responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Sponsor/{id}/tournaments
        [HttpPost("{id}/tournaments")]
        public async Task<ActionResult<TournamentSponsorResponseDTO>> LinkToTournament(
            int id,
            [FromBody] TournamentSponsorRequestDTO dto)
        {
            try
            {
                var tournamentSponsor = await _sponsorService.LinkSponsorToTournamentAsync(
                    id, dto.TournamentId, dto.ContractAmount);

                // Obtener el TournamentSponsor completo con navegación
                var responseDto = _mapper.Map<TournamentSponsorResponseDTO>(tournamentSponsor);

                return CreatedAtAction(nameof(GetSponsorTournaments), new { id = responseDto.SponsorId }, responseDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: api/Sponsor/{id}/tournaments/{tournamentId}
        [HttpDelete("{id}/tournaments/{tournamentId}")]
        public async Task<ActionResult> UnlinkFromTournament(int id, int tournamentId)
        {
            try
            {
                await _sponsorService.UnlinkSponsorFromTournamentAsync(id, tournamentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}