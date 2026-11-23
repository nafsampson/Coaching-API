// Defines optional player fields; omitted fields remain unchanged.

namespace FormUp_API.Models.Requests
{
    public class UpdatePlayerRequest
    {

        public string? Name { get; set; }

        public string? Position { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
}
