// Loads and replaces attendance for the signed-in coach’s matches and training sessions.

using FormUp_API.Models;
using FormUp_API.Models.Requests;
using FormUp_API.Services;
using Google.Cloud.Firestore;
using System.Security.Claims;

namespace FormUp_API.Endpoints
{
    public static class AttendanceEndpoint
    {

        public static WebApplication MapAttendanceEndpoints(this WebApplication app)
        {

            var sessionGroup = app.MapGroup("/api/session");

            sessionGroup.MapGet("/{id}/attendance", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Session not found");
                }
                return await ReadAttendanceAsync(uid, snapshot, fireService);
            });

            sessionGroup.MapPut("{id}/attendance", async (string id, List<AttendanceEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.SessionDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Session not found");
                }
                return await SaveAttendanceAsync(documentReference, request);
            });

            var matchGroup = app.MapGroup("/api/match");
            matchGroup.MapGet("/{id}/attendance", async (string id, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }
                return await ReadAttendanceAsync(uid, snapshot, fireService);
            });

            matchGroup.MapPut("/{id}/attendance", async (string id, List<AttendanceEntryRequest> request, FirestoreService fireService, ClaimsPrincipal user) =>
            {
                string uid = user.GetCoachUid();
                var documentReference = fireService.MatchDoc(uid, id);
                var snapshot = await documentReference.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return Results.Problem(statusCode: 404, title: "Match not found");
                }
                return await SaveAttendanceAsync(documentReference, request);
            });
            return app;
        }

        private static async Task<IResult> SaveAttendanceAsync(DocumentReference documentReference, List<AttendanceEntryRequest> request)
        {
            var attendanceDictionary = new Dictionary<string, AttendanceEntry>();

            foreach (var item in request)
            {
                if (string.IsNullOrWhiteSpace(item.PlayerId))
                {
                    continue;
                }
                attendanceDictionary[item.PlayerId] = new AttendanceEntry
                {
                    Status = item.Status,
                    Reason = item.Reason
                };
            }
            // Replace the full attendance set; callers must send all records they want to keep.
            await documentReference.UpdateAsync(new Dictionary<string, object>
            {
                ["attendance"] = attendanceDictionary
            });
            return Results.Ok(request);
        }

        private static async Task<IResult> ReadAttendanceAsync(string uid, DocumentSnapshot snapshot, FirestoreService fireService)
        {

            Dictionary<string, AttendanceEntry> attendanceMap;

            bool hasAttendanceData = snapshot.TryGetValue("attendance", out attendanceMap);

            if (hasAttendanceData == false || attendanceMap == null)
            {

                attendanceMap = new Dictionary<string, AttendanceEntry>();
            }

            // Include every current player, even when no attendance has been recorded.
            var playersSnapshot = await fireService.PlayersCollection(uid).GetSnapshotAsync();
            var players = playersSnapshot.Documents.Select(doc => doc.ConvertTo<Player>()).OrderBy(p => p.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

            var result = players.Select(player =>
            {
                bool hasEntry = attendanceMap.TryGetValue(player.Id, out var entry);
                return new
                {
                    playerId = player.Id,
                    name = player.Name,
                    position = player.Position,
                    status = hasEntry ? entry.Status : "NotRecorded",
                    reason = hasEntry ? entry.Reason : null
                };
            }).ToList();

            return Results.Ok(result);
        }
    }
}
