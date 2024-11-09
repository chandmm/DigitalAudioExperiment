/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

    This class is a data stream intercepter so that samples being played
    can be used for calculating db values. This class returns the calculated RMS
    component part.

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
using DigitalAudioExperiment.Infrastructure;
using DigitalAudioExperiment.Model;

namespace DigitalAudioExperiment.ViewModel
{
    public class FilterSettingsViewModel : BaseViewModel
    {
        #region Fields
        private bool _isDisposed;
        private Action _exitSettingsCallback;

        public delegate void ApplySettingsEvent(FilterSettingsViewModel filterSettingsViewModel);
        public event ApplySettingsEvent OnSettingsApplied;

        #endregion

        #region Properties

        private int _cutoffFrequency;
        public int CutoffFrequency
        {
            get => _cutoffFrequency;
            set
            {
                _cutoffFrequency = value;

                OnPropertyChanged();
                InvokeApplySettingsEvent();
            }
        }

        private int _bandwidth;
        public int Bandwidth
        {
            get => _bandwidth;
            set
            {
                _bandwidth = value;

                OnPropertyChanged();
                InvokeApplySettingsEvent();
            }
        }

        private int _filterOrder;
        public int FilterOrder
        {
            get => _filterOrder;
            set
            {
                _filterOrder = value;

                OnPropertyChanged();
                InvokeApplySettingsEvent();
            }
        }

        private List<FilterTypeDescriptionModel> _filterTypes;
        public List<FilterTypeDescriptionModel> FilterTypes
        {
            get => _filterTypes;
            set
            {
                _filterTypes = value;
                OnPropertyChanged();
            }
        }

        private FilterTypeDescriptionModel _filterTypeSet;
        public FilterTypeDescriptionModel FilterTypeSet
        {
            get => _filterTypeSet;
            set
            {
                _filterTypeSet = value;
                OnPropertyChanged();
                InvokeApplySettingsEvent();
            }
        }

        private bool _isFilterOutput;
        public bool IsFilterOutput
        {
            get => _isFilterOutput;
            set
            {
                _isFilterOutput = value;
                OnPropertyChanged();
                InvokeApplySettingsEvent();
            }
        }

        #endregion

        #region Commands

        public RelayCommand ExitCommand { get; set; }
        public RelayCommand DefaultCommand { get; set; }

        #endregion

        #region Initialisation

        public FilterSettingsViewModel()
        {
            Initialisation();
            ExitCommand = new RelayCommand(ExitFilterSettings, () => true);
            DefaultCommand = new RelayCommand(ResetToDefaultSettings, () => true);
        }

        private void Initialisation()
        {
            FilterTypes = new List<FilterTypeDescriptionModel>
            {
                new FilterTypeDescriptionModel() {FilterTypeValue = FilterType.Lowpass, Description = "Lowpass" },
                new FilterTypeDescriptionModel() {FilterTypeValue = FilterType.Highpass, Description = "Highpass" },
                new FilterTypeDescriptionModel() {FilterTypeValue = FilterType.Bandpass, Description = "Bandpass" },
                new FilterTypeDescriptionModel() {FilterTypeValue = FilterType.ButterworthBandpass, Description = "Butterworth bandpass" },
            };

            FilterTypeSet = FilterTypes.First(x => x.FilterTypeValue == FilterType.Bandpass);

            ResetToDefaultSettings();
        }

        #endregion

        #region Application Logic

        private void ResetToDefaultSettings()
        {
            switch(FilterTypeSet.FilterTypeValue)
            {
                case FilterType.Lowpass:
                    CutoffFrequency = 1000;
                    Bandwidth = 0;
                    break;
                case FilterType.Highpass:
                    CutoffFrequency = 20;
                    Bandwidth = 0;
                    break;
                case FilterType.Bandpass:
                    CutoffFrequency = 1040;
                    Bandwidth = 2020;
                    FilterOrder = 2;
                    break;
                case FilterType.ButterworthBandpass:
                    CutoffFrequency = 250;
                    Bandwidth = 100;
                    FilterOrder = 2;
                    break;
            }
        }

        private void ExitFilterSettings()
        {
            _exitSettingsCallback?.Invoke();
        }

        #endregion

        #region Event Handlers

        private void InvokeApplySettingsEvent()
        {
            OnSettingsApplied?.Invoke(this);
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    // Dispose managed resources.
                }

                _isDisposed = true;
            }
        }

        public void SetExitCallback(Action exitSettings)
        {
            _exitSettingsCallback = exitSettings;
        }

        #endregion
    }
}
