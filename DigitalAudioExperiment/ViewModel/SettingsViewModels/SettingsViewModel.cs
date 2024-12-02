/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using DigitalAudioExperiment.Extensions;
using DigitalAudioExperiment.Infrastructure;
using DigitalAudioExperiment.Model;
using DigitalAudioExperiment.Pages;
using DigitalAudioExperiment.View;
using DigitalAudioExperiment.View.Components;
using DigitalAudioExperiment.View.Dialogs;
using DigitalAudioExperiment.ViewModel.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace DigitalAudioExperiment.ViewModel.SettingsViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Constant Fields

        public const string DefaultThemeImage = "AudioPlayerFacePlateRounded.png";
        public const string ThemePath = "Resources/Themes";
        public const string DefaultThematicFileName = "DefaultTheme.xml";

        private static SettingsViewModel Instance = null;

        #endregion

        #region Fields

        private ReceiverViewModel _receiver;
        private FilterSettingsViewModel _filterSettingsViewModel;
        private Func<string, string[]> _getFiles;
        private Action _windowCloseFunction;

        #endregion

        #region Properties

        public bool IsReset { get; private set; }

        private bool _isShowThemeSetting;
        public bool IsShowThemeSetting
        {
            get => _isShowThemeSetting;
            set
            {
                _isShowThemeSetting = value;

                OnPropertyChanged();
            }
        }

        private ObservableCollection<ThematicModel> _thematicList;
        public ObservableCollection<ThematicModel> ThematicList
        {
            get => _thematicList;
            set
            {
                _thematicList = value;

                OnPropertyChanged();
            }
        }

        private ThematicModel _thematic;
        public ThematicModel Thematic
        {
            get => _thematic;
            set
            {
                _thematic = value;

                OnPropertyChanged();
            }
        }

        public bool HasChanges { get; private set; }

        #endregion

        #region Commands

        public RelayCommand ResetToDefaultCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand ThemesCommand { get; private set; }
        public RelayCommand AddImageCommand { get; private set; }
        public RelayCommand ApplyThemeCommand { get; private set; }
        public RelayCommand DeleteThemeCommand { get; private set; }
        public RelayCommand CreateNewThemeCommand { get; private set; }
        public RelayCommand ResetToFactoryDefaultCommand { get; private set; }

        #endregion

        // TODO: Make folders relative. I.e instead of full path, use 'Resources/Themes'
        // TODO: Have some kind of active theme that is autosaved. Load this up each time. So changes whether saved or unsaved is retained.
        private SettingsViewModel(ReceiverViewModel receiver, FilterSettingsViewModel filterSettingsViewModel, Action windowCloseFunction)
        {
            _receiver = receiver;
            _filterSettingsViewModel = filterSettingsViewModel;
            _windowCloseFunction = windowCloseFunction;

            ResetToDefaultCommand = new RelayCommand(() => ResetToDefault(null), () => true);
            CloseCommand = new RelayCommand(() =>
                {
                    _windowCloseFunction?.Invoke();
                }, () => true);
            ThemesCommand = new RelayCommand(() => IsShowThemeSetting = !IsShowThemeSetting, () => true);
            AddImageCommand = new RelayCommand(AddImageToTheme, () => true);
            ApplyThemeCommand = new RelayCommand(ApplyTheme, () => true);
            DeleteThemeCommand = new RelayCommand(DeleteTheme, () => true);
            CreateNewThemeCommand = new RelayCommand(CreateNewTheme, () => true);
            ResetToFactoryDefaultCommand = new RelayCommand(ResetToFactoryDefault, () => true);

            PropertyChanged += OnNotifiedPropertyChanged;

            Initialise();
        }

        public static SettingsViewModel GetSettingsInstance(ReceiverViewModel receiver, FilterSettingsViewModel filterSettingsViewModel, Action windowCloseFunction)
        {
            if (Instance == null)
            {
                Instance =  new SettingsViewModel(receiver, filterSettingsViewModel, windowCloseFunction);
            }
            else
            {
                Instance.UpdateReceiever(receiver);
                Instance.UpdateWindowsCloseFunction(windowCloseFunction);
            }

            return Instance;
        }

        private void Initialise()
        {
            try
            {
                // Always create default settings to preserve sanity incase of external file changes.
                ThematicList = LoadThemeListFromApplicationFolder();
                HasChanges = false;
                CreateDefaultSettingsIfNotExists();
            }
            catch
            {
                // Ignore. May be out of space or no storage read/write access.
            }

            Thematic = GetDefaultTheme();
            Save();
            ApplyTheme();
        }

        private void CreateDefaultSettingsIfNotExists()
        {
            if (ThematicList.Any(x => x.Id == ThematicModel.DefaultThemeGuid))
            {
                return;
            }

            ThematicList.Add(ThematicModel.GetDefaultSettings());

            ThematicList.Refresh();
        }

        private void UpdateReceiever(ReceiverViewModel receiver)
            => _receiver = _receiver ?? receiver;

        private void UpdateWindowsCloseFunction(Action windowCloseFunction)
            => _windowCloseFunction = windowCloseFunction;

        private ThematicModel GetDefaultTheme()
        {
            var thematic = ThematicList.FirstOrDefault(x => x.IsDefault);

            if (thematic == null)
            {
                try
                {
                    thematic = ThematicList.FirstOrDefault(x => x.IsDefault);

                    if (thematic == null
                        && ThematicList.Any())
                    {
                        thematic = ThematicList.First();
                        thematic.IsDefault = true;

                        return thematic;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"Default theme could not be found. Internal defaults will be loaded. Error details: {exception.Message}");

                    return ThematicModel.GetDefaultSettings();
                }
            }

            return thematic;
        }

        #region Manage Theme

        private static ImageSource LoadImage(string filePath)
        {
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Ensure it's fully loaded into memory
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Make it thread-safe and immutable
                    return bitmap;
                }
            }
            catch
            {
                return CreateSolidColourImage((Color)ColorConverter.ConvertFromString(ThematicModel.DefaultComponentWindowsBackgroundColour), 1, 1);
            }
        }

        private static ImageSource CreateSolidColourImage(Color color, int width, int height)
        {
            var pixelData = new byte[4 * width * height]; // 4 bytes per pixel: ARGB
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                pixelData[i] = color.B;
                pixelData[i + 1] = color.G;
                pixelData[i + 2] = color.R;
                pixelData[i + 3] = color.A;
            }

            var bitmap = BitmapSource.Create(
                width, height, 96, 96, PixelFormats.Pbgra32, null, pixelData, 4 * width);
            bitmap.Freeze(); // Make it immutable and thread-safe
            return bitmap;
        }

        private void DeleteTheme()
        {
            var result = MessageBox.Show("This will permenantly delete this theme and load application default. Are you sure?"
                , "Reset Settings"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var currentThematic = Thematic;

            var defaultTheme = ThematicList.FirstOrDefault(x => x.ThematicFileName.Contains(DefaultThematicFileName));

            if (defaultTheme == null)
            {
                defaultTheme = ThematicModel.GetDefaultSettings();
                ThematicList.Add(defaultTheme);
            }

            if (File.Exists(currentThematic.ImagePath))
            {

                File.Delete(currentThematic.ImagePath);
            }

            var thematicPath = Path.Combine(ThemePath, currentThematic.ThematicFileName);

            if (File.Exists(thematicPath))
            {

                File.Delete(thematicPath);
            }

            ApplyTheme();
            ThematicList = LoadThemeListFromApplicationFolder();
            Thematic = ThematicList.FirstOrDefault(x => x.ThematicFileName.Contains(defaultTheme.ThematicFileName));
        }

        public void SetOpenFileDialog(Func<string, string[]> getFiles)
            => _getFiles = getFiles;


        private void AddImageToTheme()
        {
            var files = _getFiles?.Invoke("PNG file | *.png");

            if (files == null)
            {
                return;
            }

            var filePath = files.First();

            var fileName = Path.GetFileName(filePath);

            if (!File.Exists(Path.Combine(ThemePath, fileName)))
            {
                File.Copy(filePath, Path.Combine(ThemePath, fileName));
            }

            Thematic.ImagePath = Path.Combine(ThemePath, fileName);

            OnPropertyChanged(nameof(Thematic));
        }

        private ObservableCollection<ThematicModel> LoadThemeListFromApplicationFolder()
        {
            var collection = new ObservableCollection<ThematicModel>();

            foreach (var path in Directory.GetFiles(ThemePath, "*.xml"))
            {
                var theme = Load(path);

                if (!string.IsNullOrEmpty(theme.Id))
                {
                    collection.Add(theme);
                }
            }

            return collection;
        }

        private bool ResetToDefault(string? message = null)
        {
            var result = MessageBox.Show(message ?? "This will reset player settings to default. You will lose your current player settings. Are you sure?"
                , "Reset Settings"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Warning);

            IsReset = result == MessageBoxResult.Yes;

            if (result == MessageBoxResult.No)
            {
                return false;
            }

            if (File.Exists(Settings.SettingsFilePath))
            {
                File.Delete(Settings.SettingsFilePath);
            }

            Settings.CreateIfNotExists(_receiver, _filterSettingsViewModel);

            return true;
        }

        private void ResetToFactoryDefault()
        {
            if (!ResetToDefault("This will reset player settings and application theme to defaults. Some updates may only occur after settings is closed. Are you sure?"))
            {
                return;
            }


            var defaultThemePath = Path.Combine(ThemePath, DefaultThematicFileName);

            if (File.Exists(defaultThemePath))
            {
                File.Delete(defaultThemePath);
            }

            ThematicList.Clear();

            CreateDefaultSettingsIfNotExists();
            Thematic = ThematicList.First();
            Save();
            ThematicList = LoadThemeListFromApplicationFolder();
            Thematic = ThematicList.FirstOrDefault(x => x.Id.Equals(ThematicModel.DefaultThemeGuid));
            ApplyTheme();
        }

        #endregion

        #region Save and Load Theme

        private void Save()
        {
            if (!HasChanges)
            {
                return;
            }

            foreach (var theme in ThematicList)
            {
                var filePath = Path.Combine(ThemePath, Path.GetFileName(theme.ThematicFileName));
                var serialiser = new XmlSerializer(typeof(ThematicModel));

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    serialiser.Serialize(fs, theme);
                }
            }
        }

        private ThematicModel Load(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ThematicModel));

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return (ThematicModel)serializer.Deserialize(fs);

            }

        }

        private void ApplyTheme()
        {
            if (Instance == null)
            {
                return;
            }

            Application.Current.Resources["ReceiverFacePlateImageSource"] = LoadImage(Thematic?.ImagePath);

            if (PlaylistPage.Instance != null)
            {
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ApplicationBackgroundFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBackgroundFill));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBackgroundFill));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentForegroundColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentHighlightColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentHighlightColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBorderColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ApplicationBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBorderColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ListTextForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ListTextForegroundColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentWindowsDefaultBackgroundColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ButtonContentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ButtonContentForegroundColour));
                PlaylistPage.Instance.Resources.MergedDictionaries.First()["ApplicationForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
            }

            if (FacePlateControlView.Instance != null)
            {
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ApplicationBackgroundFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBackgroundFill));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBackgroundFill));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentForegroundColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentHighlightColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentHighlightColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBorderColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ApplicationBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBorderColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ListTextForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ListTextForegroundColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentWindowsDefaultBackgroundColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ButtonContentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ButtonContentForegroundColour));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["ApplicationForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
            }

            if (FilterSettingsView.Instance != null)
            {
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ApplicationBackgroundFill"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBackgroundFill));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBackgroundFill));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentForegroundColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentHighlightColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentHighlightColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentBorderColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ApplicationBorderColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationBorderColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ListTextForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ListTextForegroundColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ComponentWindowsDefaultBackgroundColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ComponentWindowsDefaultBackgroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ButtonContentForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ButtonContentForegroundColour));
                FilterSettingsView.Instance.Resources.MergedDictionaries.First()["ApplicationForegroundColour"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.ApplicationForegroundColour));
            }

            if (FacePlateControlView.Instance != null)
            {
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["PanelBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.PanelBackground));
                FacePlateControlView.Instance.Resources.MergedDictionaries.First()["PanelForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Thematic.PanelForeground));
            }

            if (_receiver == null)
            {
                return;
            }

            // VU
            _receiver.BackgroundColour = Thematic.BackgroundColour;
            _receiver.NeedleColour = Thematic.NeedleColour;
            _receiver.DecalColour = Thematic.DecalColour;
            _receiver.OverdriveLampColour = Thematic.OverdriveLampColour;
            _receiver.BottomCoverFill = Thematic.BottomCoverFill;
            _receiver.OverdriveLampOffColour = Thematic.OverdriveLampOffColour;
            _receiver.MeterLabelForeground = Thematic.MeterLabelForeground;
            // Seek slider
            _receiver.SliderThumbGlowOverlay = Thematic.SliderThumbGlowOverlay;
            _receiver.SliderThumbGripBarBackground = Thematic.SliderThumbGripBarBackground;
            _receiver.SliderThumbPointBackground = Thematic.SliderThumbPointBackground;
            _receiver.SliderThumbBorder = Thematic.SliderThumbBorder;
            _receiver.SliderThumbForeground = Thematic.SliderThumbForeground;
            _receiver.SliderThumbMouseOverBackground = Thematic.SliderThumbMouseOverBackground;
            _receiver.SliderThumbMouseOverBorder = Thematic.SliderThumbMouseOverBorder;
            _receiver.SliderThumbPressedBackground = Thematic.SliderThumbPressedBackground;
            _receiver.SliderThumbPressedBorder = Thematic.SliderThumbPressedBorder;
            _receiver.SliderThumbDisabledBackground = Thematic.SliderThumbDisabledBackground;
            _receiver.SliderThumbDisabledBorder = Thematic.SliderThumbDisabledBorder;
            _receiver.SliderThumbTrackBackground = Thematic.SliderThumbTrackBackground;
            _receiver.SliderThumbTrackBorder = Thematic.SliderThumbTrackBorder;
            // Playback controls
            _receiver.SkipToStartButtonFill = Thematic.SkipToStartButtonFill;
            _receiver.StopButtonFill = Thematic.StopButtonFill;
            _receiver.PlayButtonFill = Thematic.PlayButtonFill;
            _receiver.PauseButtonFill = Thematic.PauseButtonFill;
            _receiver.SkipToEndButtonFill = Thematic.SkipToEndButtonFill;
            _receiver.SelectButtonFill = Thematic.SelectButtonFill;
            _receiver.SwitchOnBackground = Thematic.SwitchOnBackground;
            _receiver.SwitchOffBackground = Thematic.SwitchOffBackground;
            _receiver.SwitchForeground = Thematic.SwitchForeground;
            // Volume, bass, treble controls
            _receiver.GainSliderMidBarFill = Thematic.GainSliderMidBarFill;
            _receiver.GainSliderTextForeground = Thematic.GainSliderTextForeground;
            _receiver.GainSliderTickForeground = Thematic.GainSliderTickForeground;
            // Power button Control
            _receiver.PowerButtonLightFill = Thematic.PowerButtonLightFill;
            _receiver.PowerButtonStrokeFill = Thematic.PowerButtonStrokeFill;
            _receiver.PowerButtonHighlight = Thematic.PowerButtonHighlight;
            // Stereo indicator control
            _receiver.MonoOnFill = Thematic.MonoOnFill;
            _receiver.MonoOffFill = Thematic.MonoOffFill;
            _receiver.StereoOnFill = Thematic.StereoOnFill;
            _receiver.StereoOffFill = Thematic.StereoOffFill;
            _receiver.LabelForeground = Thematic.LabelForeground;
            // Playback indicator lamp colour
            _receiver.PlaybackIndicatorOffLamp = Thematic.PlaybackIndicatorOffColour;
            _receiver.PlaybackIndicatorOnLamp = Thematic.PlaybackIndicatorOnColour;

            UpdateDefaultTheme(Thematic.Id);
            OnPropertyChanged(nameof(Thematic));
            Save();
        }

        public void ApplyCurrentTheme()
        {
            ApplyTheme();
        }

        private void UpdateDefaultTheme(string id)
        {
            foreach (var thematic in ThematicList)
            {
                thematic.IsDefault = false;

                if (thematic.Id.Equals(id))
                {
                    thematic.IsDefault = true;
                }
            }
        }

        private void CreateNewTheme()
        {
            var dialog = new NameDescriptionDialog();
            var viewModel = new NameDescriptionDialogViewModel(nameFieldLabel: "Theme file name"
                , descriptionLabel: "Theme description"
                , title: "Enter your theme file name and description");

            dialog.DataContext = viewModel;
            dialog.ShowDialog();
            
            if (viewModel.IsUserAccepted)
            {
                var theme = Thematic.Clone();

                theme.Id = Guid.NewGuid().ToString();
                theme.ThematicFileName = $"{viewModel.Name}.xml";
                theme.Description = viewModel.Description;

                ThematicList.Add(theme);
                UpdateDefaultTheme(theme.Id);
                Save();

                ThematicList = LoadThemeListFromApplicationFolder();

                Thematic = ThematicList.FirstOrDefault(x => x.Id.Equals(theme.Id));

                ApplyTheme();
            }
        }

        #endregion

        #region Events

        private void OnNotifiedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            HasChanges = true;
        }

        #endregion

        #region Dispose

        public override void ExplicitDispose()
        {
            _receiver = null;
            _filterSettingsViewModel = null;
        }

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    // Dispose on close
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
