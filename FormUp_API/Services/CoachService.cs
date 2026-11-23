// Loads the signed-in coach’s profile or creates its initial values from token claims.

using FormUp_API.Models;
using System.Security.Claims;

namespace FormUp_API.Services
{
    public class CoachService
    {
        private readonly FirestoreService _firestoreService;
        public CoachService(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        public async Task<Coach>GetOrCreateCoachAsync(ClaimsPrincipal user)
        {
            string uid = user.GetCoachUid();
            var documentReference = _firestoreService.CoachDoc(uid);
            var snapshot = await documentReference.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Coach>();
            }

            string email = user.FindFirst("email")?.Value ?? user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

            string? nameClaim = user.FindFirst("name")?.Value ?? user.FindFirst(ClaimTypes.Name)?.Value;

            // Use the verified name claim, then the email prefix, before falling back to a generic label.
            string displayName = !string.IsNullOrWhiteSpace(nameClaim) ? nameClaim : (!string.IsNullOrEmpty(email) && email.Contains('@') ? email.Split('@')[0] : "Coach");

            string? profilePhoto = user.FindFirst("picture")?.Value;

            var newCoach = new Coach
            {
                Uid = uid,
                Email = email,
                DisplayName = displayName,
                ProfilePhotoUrl = profilePhoto,
                PreferredLanguage = "en",
                TeamName = null,
                AgeGroup = null,
                CreatedAt = DateTime.UtcNow
            };
            await documentReference.SetAsync(newCoach);
            return newCoach;
        }
    }
}
