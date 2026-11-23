// Defines the session date, notes and optional retry ID accepted when creating training.

namespace FormUp_API.Models.Requests
{
    public class CreateTrainingSessionRequest
    {

        // Reuse this UUID for every retry of the same create request.
        public string? ClientRequestId { get; set; }

        public DateTimeOffset SessionDate { get; set; }
        public string? Notes { get; set; }
    }
}
