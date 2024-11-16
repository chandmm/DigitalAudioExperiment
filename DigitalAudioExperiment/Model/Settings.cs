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
using DigitalAudioExperiment.Filters;
using DigitalAudioExperiment.ViewModel;
using System.IO;
using System.Text.Json;

namespace DigitalAudioExperiment.Model
{

    public static class Settings
    {
        private static string SettingsFilePath = "settings.json";
        private static bool _isLoading;

        public static void SaveReceiverSettings(ReceiverViewModel receiver)
        {
            if (receiver is null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }

            if (!IsSettingsExist())
            {
                File.WriteAllText(SettingsFilePath, "");
            }

            var settingsData = new SettingsData()
            {
                Bass = receiver.Bass,
                Treble = receiver.Treble,
                IsAutoPlayChecked = receiver.IsAutoPlayChecked,
                IsLoopPlayChecked = receiver.IsLoopPlayChecked,
                LastPlayedFile = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel)?.GetCurrentSelected(),
                IsDocked = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsDocked,
                IsShowing = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsShowing,
                FilterType = receiver.FilterSettingsViewModel.FilterTypeSet.FilterTypeValue,
                CutoffFrequency = receiver.FilterSettingsViewModel.CutoffFrequency,
                Bandwidth = receiver.FilterSettingsViewModel.Bandwidth,
                FilterOrder = receiver.FilterSettingsViewModel.FilterOrder,
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            var jsonString = JsonSerializer.Serialize(settingsData, options);
            File.WriteAllText(SettingsFilePath, jsonString);
        }

        public static bool IsSettingsExist()
            => File.Exists(SettingsFilePath);

        public static void CreateIfNotExists(ReceiverViewModel receiver, FilterSettingsViewModel filterViewModel)
        {
            if (IsSettingsExist())
            {
                return;
            }

            var settingsData = new SettingsData()
            {
                Bass = receiver.Bass,
                Treble = receiver.Treble,
                IsAutoPlayChecked = receiver.IsAutoPlayChecked, 
                IsLoopPlayChecked = receiver.IsLoopPlayChecked,
                LastPlayedFile = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel)?.GetCurrentSelected(),
                IsDocked = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsDocked,
                IsShowing = (receiver.PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsShowing,
                FilterType = filterViewModel.FilterTypeSet.FilterTypeValue,
                CutoffFrequency = filterViewModel.CutoffFrequency,
                Bandwidth = filterViewModel.Bandwidth,
                FilterOrder = filterViewModel.FilterOrder,
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settingsData, options));
        }

        public static void SaveSettings(object viewModel)
        {
            if (_isLoading)
            {
                return;
            }

            var settingsData = GetSettingsFromFile();

            if (viewModel is FilterSettingsViewModel)
            {
                settingsData.FilterType = ((FilterSettingsViewModel)viewModel).FilterTypeSet.FilterTypeValue;
                settingsData.CutoffFrequency = ((FilterSettingsViewModel)viewModel).CutoffFrequency;
                settingsData.Bandwidth = ((FilterSettingsViewModel)viewModel).Bandwidth;
                settingsData.FilterOrder = ((FilterSettingsViewModel)viewModel).FilterOrder;
            }

            if (viewModel is ReceiverViewModel)
            {
                settingsData.Bass = ((ReceiverViewModel)viewModel).Bass;
                settingsData.Treble = ((ReceiverViewModel)viewModel).Treble;
                settingsData.IsAutoPlayChecked = ((ReceiverViewModel)viewModel).IsAutoPlayChecked;
                settingsData.IsLoopPlayChecked = ((ReceiverViewModel)viewModel).IsLoopPlayChecked;
                settingsData.PlayListFile = ((PlaylistPageViewModel)((ReceiverViewModel)viewModel).PlaylistPageViewInstance.DataContext).GetCurrentlyLoadedPlaylistFilePath();
                settingsData.LastPlayedFile = ((PlaylistPageViewModel)((ReceiverViewModel)viewModel).PlaylistPageViewInstance.DataContext).GetCurrentSelected();
                settingsData.IsDocked = (((ReceiverViewModel)viewModel).PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsDocked;
                settingsData.IsShowing = (((ReceiverViewModel)viewModel).PlaylistPageViewInstance.DataContext as PlaylistPageViewModel).IsShowing;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settingsData, options));
        }

        public static SettingsData LoadSettings(ReceiverViewModel receiver, FilterSettingsViewModel filterViewModel)
        {
            _isLoading = true;

            var settingsData = GetSettingsFromFile();

            receiver.Bass = settingsData.Bass;
            receiver.Treble = settingsData.Treble;
            receiver.IsAutoPlayChecked = settingsData.IsAutoPlayChecked;
            receiver.IsLoopPlayChecked = settingsData.IsLoopPlayChecked;
            filterViewModel.FilterTypeSet = filterViewModel.FilterTypeLookup[settingsData.FilterType];
            filterViewModel.CutoffFrequency = settingsData.CutoffFrequency;
            filterViewModel.Bandwidth = settingsData.Bandwidth;
            filterViewModel.FilterOrder = settingsData.FilterOrder;

            _isLoading = false;

            return settingsData;
        }

        private static SettingsData? GetSettingsFromFile()
        {
            if (!File.Exists(SettingsFilePath))
            {
                var settingsData = GetDefaultDto();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };

                File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settingsData, options));
            }

            return JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SettingsFilePath));
        }

        private static SettingsData GetDefaultDto()
        {
            return new SettingsData()
            {
                Bass = 0,
                Treble = 0,
                FilterType = FilterType.Bandpass,
                CutoffFrequency = 1040,
                Bandwidth = 2020,
                FilterOrder = 2,
            };
        }


    }
}
