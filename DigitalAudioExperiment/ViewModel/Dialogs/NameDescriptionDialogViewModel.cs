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
using System.Windows.Forms;

namespace DigitalAudioExperiment.ViewModel.Dialogs
{
    public class NameDescriptionDialogViewModel : BaseViewModel
    {
        #region Properties

        public string Name { get; set; }
        public string Description { get; set; }
        public string NameFieldLabel { get; private set; }
        public string DescriptionFieldLabel { get; private set; }
        public string Title { get; private set; }
        public bool IsUserAccepted { get; private set; }
        public Action Close {  get; set; }

        #endregion

        #region Commands

        public RelayCommand OkCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }

        #endregion

        public NameDescriptionDialogViewModel(string nameFieldLabel, string descriptionLabel, string title)
        {
            NameFieldLabel = nameFieldLabel;
            DescriptionFieldLabel = descriptionLabel;
            Title = title;

            OkCommand = new RelayCommand(UserAccepted, () => true);
            CancelCommand = new RelayCommand(UserCancelled, () => true);

            UpdateChangedNotifications();
        }

        private void UpdateChangedNotifications()
        {
            OnPropertyChanged(nameof(Name)
                , nameof(Description)
                , nameof(NameFieldLabel)
                , nameof(DescriptionFieldLabel)
                , nameof(Title));
        }

        #region Methods

        private void UserCancelled()
        {
            IsUserAccepted = false;

            Close?.Invoke();
        }

        private void UserAccepted()
        {
            if (!Validate())
            {
                MessageBox.Show($"Both the \"{NameFieldLabel}\" and \"{DescriptionFieldLabel}\" Fields are required.");

                return;
            }

            IsUserAccepted = true;

            Close?.Invoke();
        }

        private bool Validate()
            => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Description);

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            // Nothing to dispose.
        }

        #endregion
    }
}
