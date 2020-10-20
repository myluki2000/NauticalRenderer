using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NauticalRenderer;

namespace AndroidRenderer
{
    class AndroidResourceManager : ResourceManager
    {
        private readonly AssetManager assetManager;

        public AndroidResourceManager(AssetManager assetManager)
        {
            this.assetManager = assetManager;
        }
        /// <inheritdoc />
        public override Stream GetStreamForFile(string path)
        {
            return assetManager.Open(path);
        }
    }
}