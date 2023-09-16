namespace KyoFileSignerCLI.Models
{
    public class SigningTask
    {
        public int UserId { get; set; }
        public string Certificate { get; set; }
        public string Description { get; set; }
        public DateTime DateStarted { get; set; }
        public DateTime DateFinished { get; set; }
        // Lacking Files Field
        public string[] Files { get; set; }
        public string OverallStatus { get; set; }
        public int Progress { get; set; }
    }
}