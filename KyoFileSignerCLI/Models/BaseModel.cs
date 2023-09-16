namespace KyoFileSignerCLI.Models
{
    public class BaseModel
    {
        protected List<Dictionary<string, object>> PatchList = new();
        public Dictionary<string, object> Updates { get; set; }
        protected void AddToPatchList(string key, object value)
        {
            if (Updates.ContainsKey(key) && Updates[key] != value)
            {
                Dictionary<string, object> patch = new(){
                    { "op", "replace" },
                    { "path", key },
                    { "value", Updates[key] }
                };

                PatchList.Add(patch);
            }
        }
    }
}