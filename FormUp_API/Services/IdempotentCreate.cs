// Creates records with stable request IDs so interrupted creates can be retried safely.

using Google.Cloud.Firestore;
using Grpc.Core;

namespace FormUp_API.Services;

public static class IdempotentCreate
{
    public static bool IsValid(string? requestId) => requestId == null || Guid.TryParse(requestId, out _);

    public static async Task<(string Id, T Value)> SaveAsync<T>(CollectionReference collection, string? requestId, T value)
    {
        // Keep random document IDs for clients that do not supply a retry ID.
        if (requestId == null)
        {
            var added = await collection.AddAsync(value);
            return (added.Id, value);
        }

        // Normalize the UUID so equivalent formats resolve to the same document.
        var document = collection.Document("client_" + Guid.Parse(requestId).ToString("N"));
        // Create atomically instead of checking first, which would allow concurrent duplicates.
        try { await document.CreateAsync(value); }
        catch (RpcException error) when (error.StatusCode == StatusCode.AlreadyExists)
        {
            var existing = await document.GetSnapshotAsync();
            // Do not report success if the record was deleted during the retry.
            if (!existing.Exists) throw;
            return (document.Id, existing.ConvertTo<T>());
        }
        return (document.Id, value);
    }
}
