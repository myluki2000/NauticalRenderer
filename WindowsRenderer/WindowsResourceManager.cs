using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NauticalRenderer;

namespace WindowsRenderer
{
    class WindowsResourceManager : ResourceManager
    {
        /// <inheritdoc />
        public override Stream GetStreamForFile(string path)
        {
            return File.OpenRead(path);
        }
    }
}

