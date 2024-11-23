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
using DigitalAudioExperiment.Infrastructure;
using DigitalAudioExperiment.Model;
using System.Collections.ObjectModel;
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

        #endregion

        #region Fields

        private ReceiverViewModel _receiver;
        private FilterSettingsViewModel _filterSettingsViewModel;
        private Func<string, string[]> _getFiles;

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

        #endregion

        #region Commands

        public RelayCommand ResetToDefaultCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }
        public RelayCommand ThemesCommand { get; private set; }
        public RelayCommand AddThemeCommand { get; private set; }
        public RelayCommand ApplyThemeCommand { get; private set; }
        public RelayCommand DeleteThemeCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }

        #endregion

        // TODO: Make folders relative. I.e instead of full path, use 'Resources/Themes'
        public SettingsViewModel(ReceiverViewModel receiver, FilterSettingsViewModel filterSettingsViewModel, Action windowCloseFunction)
        {
            _receiver = receiver;
            _filterSettingsViewModel = filterSettingsViewModel;

            ResetToDefaultCommand = new RelayCommand(ResetToDefault, () => true);
            CloseCommand = new RelayCommand(() => windowCloseFunction?.Invoke(), () => true);
            ThemesCommand = new RelayCommand(() => IsShowThemeSetting = !IsShowThemeSetting, () => true);
            AddThemeCommand = new RelayCommand(AddThemeFileToList, () => true);
            ApplyThemeCommand = new RelayCommand(ApplyTheme, () => true);
            DeleteThemeCommand = new RelayCommand(DeleteTheme, () => true);
            SaveCommand = new RelayCommand(Save, () => true);

            Initialise();
        }

        private void Initialise()
        {
            // Always create default settings to preserve sanity incase of external file changes.
            CreateDefaultSettings();

            //ThematicList = BuildImageListFromApplicationFolder();

            //Thematic = GetCurrentTheme();
            Thematic = Load(Path.Combine(ThemePath, DefaultThematicFileName));

            ApplyTheme();
        }

        private void CreateDefaultSettings()
        {
            Save(ThematicModel.GetDefaultSettings(), DefaultThematicFileName);
        }

        #region Manage Theme

        private ThematicModel GetCurrentTheme()
        {
            try
            {
                var imageName = Path.GetFileName(((BitmapImage)Application.Current.Resources["ReceiverFacePlateImageSource"]).UriSource.OriginalString);

                return ThematicList.FirstOrDefault(x => x.ImagePath.Contains(imageName));
            }
            catch
            {
                return null;
            }
        }

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
                return CreateSolidColourImage(Colors.Black, 1, 1);
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
            var result = MessageBox.Show("This will permenantly delete this theme. Are you sure?"
                , "Reset Settings"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var currentThematic = Thematic;

            Thematic = ThematicList.FirstOrDefault(x => x.ImagePath.Contains(DefaultThemeImage));
            ApplyTheme();

            if (File.Exists(currentThematic.ImagePath)
                && !currentThematic.ImagePath.Contains("AudioPlayerFacePlateRounded.png"))
            {

                File.Delete(currentThematic.ImagePath);
            }

            if (currentThematic.ImagePath.Contains(DefaultThemeImage))
            {
                MessageBox.Show("You cannot delete the default application theme");
                return;
            }

            ThematicList = BuildImageListFromApplicationFolder();
        }

        private void ApplyTheme()
        {
            Application.Current.Resources["ReceiverFacePlateImageSource"] = LoadImage(Thematic.ImagePath);
        }

        public void SetOpenFileDialog(Func<string, string[]> getFiles)
            => _getFiles = getFiles;

        private void AddThemeFileToList()
        {
            var currentThematic = Thematic;

            if (_getFiles == null)
            {
                return;
            }

            var files = _getFiles?.Invoke("PNG file| *.png");

            if (files == null)
            {
                return;
            }

            var resourceFolder = Path.Combine(Directory.GetCurrentDirectory(), ThemePath);

            foreach (var file in files)
            {
                if (File.Exists(Path.Combine(resourceFolder, Path.GetFileName(file))))
                {
                    continue;
                }

                File.Copy(file, Path.Combine(resourceFolder, Path.GetFileName(file)));
            }

            ThematicList = BuildImageListFromApplicationFolder();

            Thematic = ThematicList.FirstOrDefault(x => x.ImagePath.Equals(currentThematic.ImagePath));
        }

        private ObservableCollection<ThematicModel> BuildImageListFromApplicationFolder()
        {
            var collection = new ObservableCollection<ThematicModel>();

            //foreach (var path in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), ThemePath)
            //     , "*.png"))
            //{
            //    collection.Add(new ThematicModel()
            //    {
            //        ImagePath = path,
            //        Description = Path.GetFileNameWithoutExtension(path)
            //    });
            //}

            foreach (var path in Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), ThemePath), "*.xml"))
            {
                collection.Add(Load(path));
            }

            return collection;
        }

        private void ResetToDefault()
        {
            var result = MessageBox.Show("This will reset player settings to default. You will lose your saved player settings. Are you sure?"
                , "Reset Settings"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Warning);

            IsReset = result == MessageBoxResult.Yes;

            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (File.Exists(Settings.SettingsFilePath))
            {
                File.Delete(Settings.SettingsFilePath);
            }

            Settings.CreateIfNotExists(_receiver, _filterSettingsViewModel);
        }

        #endregion

        #region Save and Load Theme

        private void Save()
        {
            // Save button saves Thematic and user provided theme name.
            // Save(Thematic, userFileName);
        }

        private void Save(ThematicModel model, string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), ThemePath, fileName);
            var serialiser = new XmlSerializer(typeof(ThematicModel));

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                serialiser.Serialize(fs, model);
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

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    _receiver = null;
                    _filterSettingsViewModel = null;
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
