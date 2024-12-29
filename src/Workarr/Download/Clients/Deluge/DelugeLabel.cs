using Newtonsoft.Json;

namespace Workarr.Download.Clients.Deluge
{
    public class DelugeLabel
    {
        [JsonProperty(PropertyName = "apply_move_completed")]
        public bool ApplyMoveCompleted { get; set; }

        [JsonProperty(PropertyName = "move_completed")]
        public bool MoveCompleted { get; set; }

        [JsonProperty(PropertyName = "move_completed_path")]
        public string MoveCompletedPath { get; set; }
    }
}
