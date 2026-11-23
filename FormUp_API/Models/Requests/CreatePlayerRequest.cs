// Defines the player details and optional retry ID accepted when creating a player.

namespace FormUp_API.Models.Requests
{
    public class CreatePlayerRequest
    {

        // Reuse this UUID for every retry of the same create request.
        public string? ClientRequestId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
    }
}
