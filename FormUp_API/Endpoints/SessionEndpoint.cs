// Creates, loads, updates and deletes training sessions belonging to the signed-in coach.

using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class SessionEndpoint
    {
        public static WebApplication MapSessionEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/session");

            group.MapGet("/", async (FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var snapshot = await fireService.SessionsCollection(uid).GetSnapshotAsync();

                var sessions = snapshot.Documents.Select(doc => doc.ConvertTo<TrainingSession>()).OrderBy(p => p.SessionDate).Select(m => new { id = m.Id, sessionDate = m.SessionDate, notes = m.Notes }).ToList();

                return Results.Ok(sessions);
            });

            group.MapGet("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }
                return Results.Ok(snapshot.ConvertTo<TrainingSession>());
            });

            group.MapPost("/", async (CreateTrainingSessionRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                if (!IdempotentCreate.IsValid(request.ClientRequestId))
                    return Results.BadRequest(new { message = "ClientRequestId must be a UUID." });
                var session = new TrainingSession
                {
                    SessionDate = request.SessionDate.UtcDateTime,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    Attendance = new()

                };

                var created = await IdempotentCreate.SaveAsync(fireService.SessionsCollection(uid), request.ClientRequestId, session);
                session = created.Value;
                session.Id = created.Id;
                return Results.Created($"/api/session/{session.Id}", session);

            });

            group.MapPatch("/{id}", async (string id, UpdateTrainingSessionRequest request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }

                var updates = new Dictionary<string, object>();

                if (request.SessionDate.HasValue)
                {
                    updates["sessionDate"] = request.SessionDate.Value.UtcDateTime;
                }

                if (request.Notes != null)
                {
                    updates["notes"] = request.Notes;
                }

                if (updates.Count == 0)
                {
                    return Results.Ok(snapshot.ConvertTo<TrainingSession>());
                }

                await documentReference.UpdateAsync(updates);
                var updatedSnapshot = await documentReference.GetSnapshotAsync();
                return Results.Ok(updatedSnapshot.ConvertTo<TrainingSession>());
            });

            group.MapDelete("/{id}", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Training Session not found");
                }

                await documentReference.DeleteAsync();
                return Results.NoContent();
            });
            return app;
        }
    }
}
