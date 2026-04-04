using System.ComponentModel.DataAnnotations;

namespace SportsLeague.API.DTOs.Request
{
    public class TournamentSponsorRequestDTO
    {
        [Required(ErrorMessage = "El ID del torneo es requerido")]
        public int TournamentId { get; set; }

        [Required(ErrorMessage = "El monto del contrato es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto del contrato debe ser mayor a 0")]
        public decimal ContractAmount { get; set; }
    }
}