namespace KyoFileSignerCLI.Models
{
    public class CreateTask
    {
        public string[] FilesToSign { get; set; }
        public int CertificateId { get; set; }
        public string Description { get; set; }
    }
}