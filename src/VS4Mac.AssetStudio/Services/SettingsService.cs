using System.IO;
using System.Xml.Linq;
using MonoDevelop.Core;
using VS4Mac.AssetStudio.Helpers;

namespace VS4Mac.AssetStudio.Services
{
    public class SettingsService
    {
        const string ConfigName = "MonoDevelop.AssetStudio.config";
        readonly string ConfigurationPath = UserProfile.Current.ConfigDir;

        public SettingsService()
        {
            ConfigurationPath = Path.Combine(ConfigurationPath, ConfigName);
        }

        public bool SettingsExists()
        {
            return File.Exists(ConfigurationPath);
        }

        public void Save(Models.Settings settings)
        {
            XDocument doc = new XDocument();
            doc.Add(new XElement("AssetStudio"));
            doc.Root.Add(new XElement("ApiKey", settings.ApiKey));

            doc.Save(ConfigurationPath);
        }

        public Models.Settings Load()
        {
            var settings = Models.Settings.Default();

            EnsureDocumentSettingsExists(settings);
            var document = XDocument.Load(ConfigurationPath);

            if (document.Root == null)
            {
                return settings;
            }

            var apiKey = document.Root.Element("ApiKey");

            if (apiKey != null)
            {
                settings.ApiKey = apiKey.Value;
            }

            return settings;
        }

        private void EnsureDocumentSettingsExists(Models.Settings settings)
        {
            if (SettingsExists() && XDocumentHelper.CanBeLoaded(ConfigurationPath))
                return;

            this.Save(settings);
        }
    }
}