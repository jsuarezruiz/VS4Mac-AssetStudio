using System;
using VS4Mac.AssetStudio.Services;
using Xwt;

namespace VS4Mac.AssetStudio.Controls
{
    public class OptimizeImageSettingsWidget : VBox
    {
        HBox _linkBox;
        Label _apiKeyDescriptionLabel;
        LinkLabel _createApiKeyLink;
        Label _apiKeyLabel;
        TextEntry _apiKeyEntry;

        SettingsService _settingsService;

        public OptimizeImageSettingsWidget()
        {
            Init();
            BuildGui(); 
            LoadData();
        }

        void Init()
        {
            _settingsService = new SettingsService();

            _linkBox = new HBox();

            _apiKeyDescriptionLabel = new Label("To optimize JPEG and PNG images on the fly it is necessary to have an API Key of TinyPNG.")
            {
                Font = Font.WithSize(10)
            };

            _createApiKeyLink = new LinkLabel("Create API Key")
            {
                Font = Font.WithSize(10),
                Uri = new Uri("https://tinypng.com/developers")
            };

            _apiKeyLabel = new Label("API Key");
            _apiKeyEntry = new TextEntry();
        }

        void BuildGui()
        {
            _linkBox.PackStart(_apiKeyDescriptionLabel);
            _linkBox.PackStart(_createApiKeyLink);

            PackStart(_linkBox);
            PackStart(_apiKeyLabel);
            PackStart(_apiKeyEntry);
        }

        void LoadData()
        {
            var settings = _settingsService.Load();
            _apiKeyEntry.Text = settings.ApiKey;
        }

        public void ApplyChanges()
        {
            Models.Settings settings = new Models.Settings
            {
                ApiKey = _apiKeyEntry.Text
            };

            _settingsService.Save(settings);
        }
    }
}