// Reads the coach’s Firebase UID from validated authentication claims.

using System.Security.Claims;

namespace FormUp_API.Services
{
    public static class CoachExtension
    {

        public static string GetCoachUid(this ClaimsPrincipal user)
        {
            var uid = user.FindFirst("sub")?.Value ?? user.FindFirst("user_id")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Reject requests that have no account identifier.
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new UnauthorizedAccessException("Coach UID is missing from the token");
            }
            return uid;
        }
    }
}
