using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using WindowsRenderer.Properties;
using NauticalRenderer;

namespace WindowsRenderer
{
    class WindowsSettingsManager : SettingsManager
    {
        /// <inheritdoc />
        public override event EventHandler<string> SettingChanged;

        /// <inheritdoc />
        public override object GetSettingsValue(string key)
        {
            return Settings.Default[key];
        }

        /// <inheritdoc />
        public override void SetSettingsValue(string key, object value)
        {
            Settings.Default[key] = value;
            Settings.Default.Save();
            SettingChanged?.Invoke(this, key);
        }
    }
}
