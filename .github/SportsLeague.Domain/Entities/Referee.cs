namespace SportsLeague.Domain.Entities
{
    public class Referee : AuditBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}
