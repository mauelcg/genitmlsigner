using System.Collections.ObjectModel;

namespace KyoFileSignerCLI.Models
{
    public class StreamCollection : Collection<Stream>, IDisposable
    {
        public void Dispose()
        {
            foreach (var stream in this)
            {
                stream.Dispose();
            }
        }
    }
}