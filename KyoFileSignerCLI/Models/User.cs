using System.ComponentModel.DataAnnotations;

namespace KyoFileSignerCLI.Models
{
    public class User : BaseModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public List<Dictionary<string, object>> toPatchList()
        {
            AddToPatchList("firstName", FirstName);
            AddToPatchList("lastName", LastName);
            AddToPatchList("status", Status);
            return PatchList;
        }
    }
}