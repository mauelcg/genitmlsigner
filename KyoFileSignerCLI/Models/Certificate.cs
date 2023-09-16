using System.ComponentModel.DataAnnotations;

namespace KyoFileSignerCLI.Models
{
    public class Certificate : BaseModel
    {
        public static string TOKEN = "";
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Thumbprint { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime Expiry { get; set; }
        public bool IsDefault { get; set; }
        [Required]
        public string Status { get; set; }
        public List<Dictionary<string, object>> toPatchList()
        {
            AddToPatchList("name", Name);
            AddToPatchList("thumbprint", Thumbprint);
            AddToPatchList("password", Password);
            AddToPatchList("expiry", IsDefault);
            AddToPatchList("status", Status);
            return PatchList;
        }
    }
}