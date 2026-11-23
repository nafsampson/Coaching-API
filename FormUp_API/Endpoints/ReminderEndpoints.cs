// Runs due reminders only when the scheduler supplies the configured shared secret.

using FormUp_API.Services;
using System.Security.Cryptography;
using System.Text;

namespace FormUp_API.Endpoints;

public static class ReminderEndpoints
{
    public static WebApplication MapReminderEndpoints(this WebApplication app)
    {

        app.MapPost("/api/reminders/run", async (HttpContext context, IConfiguration config, ReminderService reminders) =>
        {
            var secret = config["Reminder:Secret"];
            var supplied = context.Request.Headers["X-Reminder-Secret"].ToString();
            // This route uses the scheduler secret instead of a coach token; compare its hashes in constant time.
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(supplied) ||
                !CryptographicOperations.FixedTimeEquals(
                    SHA256.HashData(Encoding.UTF8.GetBytes(secret)),
                    SHA256.HashData(Encoding.UTF8.GetBytes(supplied))))
                return Results.Unauthorized();

            var sent = await reminders.SendDueAsync();
            return Results.Ok(new { sent, checkedAtUtc = DateTime.UtcNow });
        }).AllowAnonymous();
        return app;
    }
}
