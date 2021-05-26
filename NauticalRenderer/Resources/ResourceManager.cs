using System.IO;

namespace NauticalRenderer.Resources
{
    public abstract class ResourceManager
    {
        public abstract Stream GetStreamForFile(string path);
    }
}
