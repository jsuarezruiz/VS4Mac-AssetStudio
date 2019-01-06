using System;
using System.IO;
using MonoDevelop.Components;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using VS4Mac.AssetStudio.Helpers;
using VS4Mac.AssetStudio.Services;
using Xwt;
using Xwt.Drawing;

namespace VS4Mac.AssetStudio.Views
{
    public class CompressImageDialog : Xwt.Dialog
    {
        VBox _mainBox;
        HBox _contentBox;
        VBox _imageBox;
        Xwt.ImageView _previewImage;
        Label _previewName;
        Label _previewSize;
        VBox _resizeBox;
        Label _heightLabel;
        TextEntry _heightEntry;
        Label _widthLabel;
        TextEntry _widthEntry;
        HBox _buttonBox;
        VBox _loadingBox;
        MDSpinner _spinner;
        VBox _resultBox;
        Label _resultTitleLabel;
        Label _resultContentLabel;
        Label _resultPercentageLabel;
        Button _closeButton;
        Button _compressButton;

        ProjectFile _projectFile;

        TinifyService _tinifyService;
        SettingsService _settingsService;

        public CompressImageDialog(ProjectFile projectFile)
        {
            Init(projectFile);
            BuildGui(); 
            AttachEvents();
            LoadImage();
            LoadSettings();
        }

        public bool Compressed { get; set; }

        void Init(ProjectFile projectFile)
        {
            _projectFile = projectFile;

            _settingsService = new SettingsService();
            _tinifyService = new TinifyService();

            Title = "Compress Image";

            _mainBox = new VBox
            {
                HeightRequest = 250,
                WidthRequest = 500
            };

            _contentBox = new HBox();

            _imageBox = new VBox();

            _resizeBox = new VBox();

            _previewImage = new Xwt.ImageView
            {
                HeightRequest = 150,
                WidthRequest = 150,
                HorizontalPlacement = WidgetPlacement.Start,
                VerticalPlacement = WidgetPlacement.Start
            };

            _previewName = new Label();
            _previewSize = new Label();

            _heightLabel = new Label("Height");

            _heightEntry = new TextEntry
            {
                ReadOnly = true
            };

            _widthLabel = new Label("Width");

            _widthEntry = new TextEntry
            {
                ReadOnly = true
            };

            _loadingBox = new VBox();

            _spinner = new MDSpinner
            {
                Animate = true,
                HorizontalPlacement = WidgetPlacement.Center,
                VerticalPlacement = WidgetPlacement.Center
            };

            _resultBox = new VBox();

            _resultTitleLabel = new Label
            {
                HorizontalPlacement = WidgetPlacement.Center,
                Font = Font.SystemFont.WithSize(22),
                Margin = new WidgetSpacing(0, 60, 0, 0)
            };

            _resultContentLabel = new Label
            {
                HorizontalPlacement = WidgetPlacement.Center,
                Margin = new WidgetSpacing(18)
            };

            _resultPercentageLabel = new Label
            {
                HorizontalPlacement = WidgetPlacement.Center,
                Font = Font.SystemFont.WithSize(18),
                Margin = new WidgetSpacing(0, 0, 0, 60)
            };

            _buttonBox = new HBox();

            _closeButton = new Button("Close");

            _compressButton = new Button("Compress")
            {
                BackgroundColor = MonoDevelop.Ide.Gui.Styles.BaseSelectionBackgroundColor,
                LabelColor = MonoDevelop.Ide.Gui.Styles.BaseSelectionTextColor
            };
        }

        void BuildGui()
        {
            _imageBox.PackStart(_previewImage);
            _imageBox.PackStart(_previewName);
            _imageBox.PackStart(_previewSize);

            _resizeBox.PackStart(_heightLabel);
            _resizeBox.PackStart(_heightEntry);

            _resizeBox.PackStart(_widthLabel);
            _resizeBox.PackStart(_widthEntry);

            _loadingBox.PackStart(_spinner, true, WidgetPlacement.Center);

            _buttonBox.PackEnd(_compressButton);
            _buttonBox.PackEnd(_closeButton);

            _contentBox.PackStart(_imageBox, false);
            _contentBox.PackStart(_resizeBox, true);

            _resultBox.PackStart(_resultTitleLabel);
            _resultBox.PackStart(_resultContentLabel);
            _resultBox.PackStart(_resultPercentageLabel);

            _mainBox.PackStart(_contentBox, true);
            _mainBox.PackStart(_loadingBox, true);
            _mainBox.PackStart(_resultBox, true);
            _mainBox.PackEnd(_buttonBox);

            Content = _mainBox;
            Resizable = false;
        }

        void AttachEvents()
        {
            _closeButton.Clicked += (sender, e) => Respond(Command.Close);
            _compressButton.Clicked += OnCompressButtonClicked;
        }

        void LoadSettings()
        {
            var settings = _settingsService.Load();
            _tinifyService.Init(settings.ApiKey);
        }

        void LoadImage()
        {
            Loading(true);

            var metadata = ImageHelper.GetImageMetadata(_projectFile.FilePath);

            _previewImage.Image = Image.FromFile(_projectFile.FilePath).WithSize(150, 150);
            _previewName.Text = _projectFile.FilePath.FileName;

            if (metadata != null)
            {
                _previewSize.Text = ImageHelper.GetImageSize(metadata);
                _heightEntry.Text = metadata.Height.ToString();
                _widthEntry.Text = metadata.Width.ToString();
            }

            Loading(false);
        }

        void Loading(bool isLoading)
        {
            _contentBox.Visible = !isLoading && !Compressed;
            _loadingBox.Visible = isLoading;
            _resultBox.Visible = !isLoading && Compressed;
            _compressButton.Sensitive = !isLoading && !Compressed;
            _closeButton.Sensitive = !isLoading;
        }

        async void OnCompressButtonClicked(object sender, EventArgs e)
        {
            var filePath = _projectFile.FilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if(string.IsNullOrEmpty(_tinifyService.APIKey))
            {
                MessageService.ShowWarning("It is necessary to add an API Key of TinyPNG in the Preferences of Visual Studio.");
                return;
            }

            var progressMonitor = IdeApp.Workbench.ProgressMonitors.GetStatusProgressMonitor("Compressing images...", Stock.StatusSolutionOperation, false, true, false);

            Loading(true);

            bool success = true;

            try
            {
                progressMonitor.Log.WriteLine($"Compressing {Path.GetFileName(filePath)}...");

                var originalSize = new FileInfo(filePath).Length;

                var source = await _tinifyService.CompressAsync(filePath);
                _tinifyService.DownloadImage(source.Output.Url, filePath);
                var finalSize = source.Output.Size;

                progressMonitor.Log.WriteLine($"Compression complete. {Path.GetFileName(filePath)} was {ImageHelper.GetImageSize(originalSize)}, now {ImageHelper.GetImageSize(finalSize)}");

                LoadResults(filePath, originalSize, finalSize);
                Compressed = true;
            }
            catch (Exception ex)
            {
                progressMonitor.Log.WriteLine($"An error occurred compressing {Path.GetFileName(filePath)}: {ex.Message}");
                LoadErrors(filePath);
                success = false;
            }

            Loading(false);

            progressMonitor.EndTask();

            if (success)
            {
                progressMonitor.ReportSuccess("Images compressed successfully.");
            }
            else
            {
                progressMonitor.ReportWarning("Process completed.");
            }
        }

        void LoadResults(string filePath, long originalSize, long finalSize)
        {
            _resultTitleLabel.Text = "Completed";
            _resultContentLabel.Text = $"{Path.GetFileName(filePath)} was {ImageHelper.GetImageSize(originalSize)}, now {ImageHelper.GetImageSize(finalSize)}";
            var percentChange = Math.Round((finalSize - originalSize) * 100.0 / originalSize, 2);
            _resultPercentageLabel.Text = $"{percentChange} %";
        }

        void LoadErrors(string filePath)
        {
            _resultTitleLabel.Text = "Oops, an error ocurred.";
            _resultContentLabel.Text = $"An error occurred compressing {Path.GetFileName(filePath)}";
        }
    }
}