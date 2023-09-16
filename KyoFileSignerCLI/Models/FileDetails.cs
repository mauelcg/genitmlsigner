namespace KyoFileSignerCLI.Models
{
    public class FileDetails
    {
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public FileType FileType { get; set; }
    }
}