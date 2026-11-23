// Loads the coach profile and updates profile details, device tokens and reminder preferences.

using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{

    public static class CoachEndpoints
    {

        public static WebApplication MapCoachEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/coach");

            group.MapGet("/", async (CoachService coachService, ClaimsPrincipal user) =>
            {
                var coach = await coachService.GetOrCreateCoachAsync(user);
                return Results.Ok(coach);
            });

            group.MapPatch("/", async (UpdateCoachRequest request, CoachService coachService, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                var coach = await coachService.GetOrCreateCoachAsync(user);
                string uid = user.GetCoachUid();

                var updates = new Dictionary<string, object>();
                if (request.DisplayName != null)
                {
                    updates["displayName"] = request.DisplayName;
                }

                if (request.ProfilePhotoUrl != null)
                {
                    updates["profilePhotoUrl"] = request.ProfilePhotoUrl;
                }

                if (request.PreferredLanguage != null)
                {
                    updates["preferredLanguage"] = request.PreferredLanguage;
                }

                // Skip empty updates because Firestore requires at least one field.
                if (updates.Count == 0)
                {
                    return Results.Ok(coach);
                }

                await fireService.CoachDoc(uid).UpdateAsync(updates);

                var updatedSnapshot = await fireService.CoachDoc(uid).GetSnapshotAsync();
                var updatedCoach = updatedSnapshot.ConvertTo<Coach>();
                return Results.Ok(updatedCoach);
            });

            group.MapPost("/device-token", async (DeviceTokenRequest request, ReminderService reminders, ClaimsPrincipal user) =>
            {
                if (string.IsNullOrWhiteSpace(request.Token) || request.Token.Length > 4096)
                    return Results.BadRequest();
                await reminders.RegisterTokenAsync(user.GetCoachUid(), request.Token);
                return Results.NoContent();
            });

            group.MapPut("/reminders", async (ReminderPreference request, ReminderService reminders, ClaimsPrincipal user) =>
            {
                await reminders.SetEnabledAsync(user.GetCoachUid(), request.Enabled);
                return Results.NoContent();
            });

            return app;
        }
    }
}

namespace FormUp_API.Endpoints
{
    public record DeviceTokenRequest(string Token);
    public record ReminderPreference(bool Enabled);
}
