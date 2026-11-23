// Defines a player’s attendance values submitted when saving an event.

namespace FormUp_API.Models.Requests
{
    public class AttendanceEntryRequest
    {
        public string PlayerId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
