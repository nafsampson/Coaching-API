// Defines the match details and optional retry ID accepted when creating a match.

namespace FormUp_API.Models.Requests
{
    public class CreateMatchRequest
    {

        // Reuse this UUID for every retry of the same create request.
        public string? ClientRequestId { get; set; }

        public string Opponent { get; set; } = string.Empty;
        public DateTimeOffset MatchDate { get; set; }
        public string Venue { get; set; } = string.Empty;
        public int? TeamScore { get; set; }
        public int? OpponentScore { get; set; }
    }
}
