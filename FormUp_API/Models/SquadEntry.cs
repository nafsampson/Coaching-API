// Defines a player’s match availability, squad role and assigned pitch position.

using Google.Cloud.Firestore;

namespace FormUp_API.Models
{
    [FirestoreData]
    public class SquadEntry
    {
        [FirestoreProperty("status")]
        public string Status { get; set; } = string.Empty;

        [FirestoreProperty("selection")]
        public string Selection { get; set; } = string.Empty;

        [FirestoreProperty("assignedPosition")]
        public string AssignedPosition { get; set; } = string.Empty;

        [FirestoreProperty("pitchX")]
        public double? PitchX { get; set; }

        [FirestoreProperty("pitchY")]
        public double? PitchY { get; set; }

        public SquadEntry()
        {
        }
    }
}
