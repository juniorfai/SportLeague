using SportsLeague.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SportsLeague.API.DTOs.Request
{
    public class SponsorRequestDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email de contacto es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string ContactEmail { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Url(ErrorMessage = "El formato de la URL no es válido")]
        public string? WebsiteUrl { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        public SponsorCategory Category { get; set; }
    }
}