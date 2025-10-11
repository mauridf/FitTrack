namespace FitTrack.Core.DTOs
{
    public class UpdateSessionStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "planned", "completed", "skipped"
    }
}
