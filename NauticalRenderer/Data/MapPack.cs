using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Zip;

namespace NauticalRenderer.Data
{
    public class MapPack
    {
        private string zipPath;

        /// <inheritdoc />
        public MapPack(string zipPath)
        {
            this.zipPath = zipPath;
        }

        public Stream OpenFile(string mapName)
        {
            Stream tmp = Globals.ResourceManager.GetStreamForFile(zipPath);
            Stream output = new MemoryStream();
            using (ZipFile zip = ZipFile.Read(tmp))
            {
                ZipEntry entry = zip[mapName];
                entry.Extract(output);
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }
    }
}
