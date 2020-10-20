using System.IO;

namespace NauticalRenderer
{
    public abstract class ResourceManager
    {
        public abstract Stream GetStreamForFile(string path);
    }
}
