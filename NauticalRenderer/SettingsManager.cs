using System;
using System.Collections.Generic;
using System.Text;

namespace NauticalRenderer
{
    public abstract class SettingsManager
    {
        public abstract object GetSettingsValue(string key);

        public abstract void SetSettingsValue(string key, object value);

        public abstract event EventHandler<string> SettingChanged;
    }
}
