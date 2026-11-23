// Defines a formation template and its pitch slots for create and update requests.

namespace FormUp_API.Models.Requests
{
    public class SaveFormationRequest
    {

        // Reuse this UUID for every retry of the same create request.
        public string? ClientRequestId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public List<FormationSlotRequest> Positions { get; set; } = new();
    }

    public class FormationSlotRequest
    {
        public string SlotId { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public double X { get; set; }

        public double Y { get; set; }
    }
}
