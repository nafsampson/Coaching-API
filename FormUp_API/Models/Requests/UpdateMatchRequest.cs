// Defines optional match fields; omitted fields remain unchanged.

namespace FormUp_API.Models.Requests
{
    public class UpdateMatchRequest
    {

        public string? Opponent { get; set; }
        public DateTimeOffset? MatchDate { get; set; }
        public string? Venue { get; set; }
        public int? TeamScore { get; set; }
        public int? OpponentScore { get; set; }
    }
}
