// Defines optional coach profile fields; omitted fields remain unchanged.

namespace FormUp_API.Models.Requests
{
    public class UpdateCoachRequest
    {
        public string? DisplayName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public string? PreferredLanguage { get; set; }
    }
}
