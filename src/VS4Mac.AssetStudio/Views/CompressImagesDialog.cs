using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using VS4Mac.AssetStudio.Helpers;
using VS4Mac.AssetStudio.Services;
using Xwt;

namespace VS4Mac.AssetStudio.Views
{
    public class CompressImageInfo
    {
        public int ImageId { get; set; }
        public string Path { get; set; }
        public string Weight { get; set; }
    }

    public class CompressImagesDialog : Dialog
    {
        VBox _mainBox;
        ListView _filesView;
        DataField<bool> _isCheckedField;
        DataField<string> _nameField;
        DataField<string> _weightField;
        DataField<string> _compressWeightField;
        DataField<string> _percentage;
        ListStore _fileStore;
        HBox _buttonBox;
        Button _closeButton;
        Button _compressButton;

        ProjectFolder _projectFolder;

        TinifyService _tinifyService;
        SettingsService _settingsService;

        public CompressImagesDialog(ProjectFolder projectFolder)
        {
            Init(projectFolder);
            BuildGui();
            AttachEvents();
            LoadSettings();
            LoadImages();
        }

        void Init(ProjectFolder projectFolder)
        {
            _projectFolder = projectFolder;

            _settingsService = new SettingsService();
            _tinifyService = new TinifyService();

            Title = "Compress Images";

            _mainBox = new VBox
            {
                HeightRequest = 300,
                WidthRequest = 600
            };

            _filesView = new ListView
            {
                GridLinesVisible = GridLines.Both
            };

            _isCheckedField = new DataField<bool>();
            _nameField = new DataField<string>();
            _weightField = new DataField<string>();
            _compressWeightField = new DataField<string>();
            _percentage = new DataField<string>();

            _fileStore = new ListStore(_isCheckedField, _nameField, _weightField, _compressWeightField, _percentage);

            _buttonBox = new HBox();

            _closeButton = new Button("Close");

            _compressButton = new Button("Compress")
            {
                BackgroundColor = Styles.BaseSelectionBackgroundColor,
                LabelColor = Styles.BaseSelectionTextColor
            };
        }

        void BuildGui()
        {
            var checkView = new CheckBoxCellView(_isCheckedField)
            {
                Editable = true
            };

            _filesView.Columns.Add("Images", checkView, new TextCellView(_nameField));
            _filesView.Columns.Add("Weight", new TextCellView(_weightField));
            _filesView.Columns.Add("Compress Weight", new TextCellView(_compressWeightField));
            _filesView.Columns.Add("Percentage", new TextCellView(_percentage));

            _filesView.DataSource = _fileStore;

            _buttonBox.PackEnd(_compressButton);
            _buttonBox.PackEnd(_closeButton);

            _mainBox.PackStart(_filesView, true);
            _mainBox.PackEnd(_buttonBox, false);

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

        void LoadImages()
        {
            _fileStore.Clear();

            var images = FileHelper.GetFolderImages(_projectFolder.Path);

            foreach (var image in images)
            {
                var metadata = ImageHelper.GetImageMetadata(_projectFolder.Path + image);

                if (metadata != null)
                {
                    var row = _fileStore.AddRow();
                    _fileStore.SetValue(row, _isCheckedField, true);
                    _fileStore.SetValue(row, _nameField, image);
                    _fileStore.SetValue(row, _weightField, ImageHelper.GetImageSize(metadata.FileSize));
                }
            }
        }

        void Loading(bool isLoading)
        {
            _compressButton.Sensitive = !isLoading;
            _closeButton.Sensitive = !isLoading;
        }

        async void OnCompressButtonClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_tinifyService.APIKey))
            {
                MessageService.ShowWarning("It is necessary to add an API Key of TinyPNG in the Preferences of Visual Studio.");
                return;
            }

            var progressMonitor = IdeApp.Workbench.ProgressMonitors.GetStatusProgressMonitor("Compressing images...", Stock.StatusSolutionOperation, false, true, false);

            Loading(true);

            var selectedImages = new List<CompressImageInfo>();

            for (int i = 0; i < _fileStore.RowCount; i++)
            {
                var isChecked = _fileStore.GetValue(i, _isCheckedField);

                if (isChecked)
                {
                    var compressImageInfo = new CompressImageInfo
                    {
                        ImageId = i,
                        Path = _projectFolder.Path + _fileStore.GetValue(i, _nameField),
                        Weight = _fileStore.GetValue(i, _weightField)
                    };

                    selectedImages.Add(compressImageInfo);
                }
            }

            bool success = true;

            foreach (var selectedImage in selectedImages)
            {
                try
                {
                    progressMonitor.Log.WriteLine($"Compressing {Path.GetFileName(selectedImage.Path)}...");

                    var originalSize = new FileInfo(selectedImage.Path).Length;

                    var source = await _tinifyService.CompressAsync(selectedImage.Path);
                    _tinifyService.DownloadImage(source.Output.Url, selectedImage.Path);
                    var finalSize = source.Output.Size;

                    _fileStore.SetValue(selectedImage.ImageId, _compressWeightField, ImageHelper.GetImageSize(finalSize));
                    var percentChange = Math.Round((finalSize - originalSize) * 100.0 / originalSize, 2);
                    _fileStore.SetValue(selectedImage.ImageId, _percentage, $"{percentChange} %");

                    progressMonitor.Log.WriteLine($"Compression complete. {Path.GetFileName(selectedImage.Path)} was {ImageHelper.GetImageSize(originalSize)}, now {ImageHelper.GetImageSize(finalSize)}");
                }
                catch (Exception ex)
                {
                    progressMonitor.Log.WriteLine($"An error occurred compressing {Path.GetFileName(selectedImage.Path)}: {ex.Message}");

                    _fileStore.SetValue(selectedImage.ImageId, _compressWeightField, selectedImage.Weight);
                    _fileStore.SetValue(selectedImage.ImageId, _percentage, "0 %");

                    success = false;
                }
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
    }
}