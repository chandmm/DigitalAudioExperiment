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
using System.IO;

namespace DigitalAudioExperiment.Model
{
    public class ThematicModel
    {
        #region Constants
        // Application
        public const string DefaultThemeGuid = "BF61B3F9-2810-43EF-B710-C4C41EEB972C";

        // Theme
        public const string DefaultBackgroundColour = "DodgerBlue";
        public const string DefaultNeedleColour = "Black";
        public const string DefaultDecalColour = "Black";
        public const string DefaultOverdriveLampColour = "Red";
        public const string DefaultBottomCoverFill = "Black";
        public const string DefaultOverdriveLampOffColour = "#550000";
        public const string DefaultMeterLabelForeground = "#62e3f6";
        public const string DefaultSliderThumbGlowOverlay = "OrangeRed";
        public const string DefaultSliderThumbGripBarBackground = "#250a01";
        public const string DefaultSliderThumbPointBackground = "White";
        public const string DefaultSliderThumbBorder = "#FFACACAC";
        public const string DefaultSliderThumbForeground = "DodgerBlue";
        public const string DefaultSliderThumbMouseOverBackground = "#FFDCECFC";
        public const string DefaultSliderThumbMouseOverBorder = "#FF7Eb4EA";
        public const string DefaultSliderThumbPressedBackground = "#FFDAECFC";
        public const string DefaultSliderThumbPressedBorder = "#FF569DE5";
        public const string DefaultSliderThumbDisabledBackground = "#FFF0F0F0";
        public const string DefaultSliderThumbDisabledBorder = "#FFD9D9D9";
        public const string DefaultSliderThumbTrackBackground = "Cyan";
        public const string DefaultSliderThumbTrackBorder = "Blue";
        public const string DefaultSkipToStartButtonFill = "Black";
        public const string DefaultStopButtonFill = "Black";
        public const string DefaultPlayButtonFill = "Black";
        public const string DefaultPauseButtonFill = "Black";
        public const string DefaultSkipToEndButtonFill = "Black";
        public const string DefaultSelectButtonFill = "Black";
        public const string DefaultSwitchOnBackground = "#62e3f6";
        public const string DefaultSwitchOffBackground = "#000040";
        public const string DefaultSwitchForeground = "White";
        public const string DefaultGainSliderMidBarFill = "Red";
        public const string DefaultGainSliderTextForeground = "White";
        public const string DefaultGainSliderTickForeground = "White";
        public const string DefaultPowerButtonLightFill = "Red";
        public const string DefaultPowerButtonStrokeFill = "Black";
        public const string DefaultPowerButtonHighlight = "LimeGreen";
        public const string DefaultMonoOnFill = "#62e3f6";
        public const string DefaultMonoOffFill = "#000040";
        public const string DefaultStereoOnFill = "Red";
        public const string DefaultStereoOffFill = "#200000";
        public const string DefaultLabelForeground = "Black";
        public const string DefaultApplicationBackgroundFill = "Black";
        public const string DefaultComponentBackgroundColour = "DodgerBlue";
        public const string DefaultComponentForegroundColour = "Black";
        public const string DefaultComponentHighlightColour = "#62e3f6";
        public const string DefaultComponentBorderColour = "Black";
        public const string DefaultPlaybackIndicatorOffColour = "#00004A";
        public const string DefaultPlaybackIndicatorOnColour = "#0020FF";
        //Application Theme
        public const string DefaultApplicationBorderColour = "DodgerBlue";
        public const string DefaultListTextForegroundColour = "Black";
        public const string DefaultComponentWindowsBackgroundColour = "Silver";
        public const string DefaultApplicationForegroundColour = "White";
        public const string DefaultButtonContentForegroundColour = "Black";

        #endregion

        #region Properties
        //Thematic options
        public bool IsDefault {  get; set; }
        public string Id { get; set; }
        // Application
        public string ThematicFileName { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get => Path.GetFileName(ImagePath); }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ComponentBackgroundFill { get; set; }
        public string ComponentForegroundColour { get; set; }
        public string ComponentHighlightColour { get; set; }
        public string ComponentBorderColour { get; set; }
        public string ApplicationBackgroundFill { get; set; }
        public string ApplicationBorderColour { get; set; }
        public string ListTextForegroundColour { get; set; }
        public string ComponentWindowsDefaultBackgroundColour { get; set; }
        public string ApplicationForegroundColour { get; set; }
        public string ButtonContentForegroundColour { get; set; }

        // VU meter
        public string BackgroundColour { get; set; }
        public string NeedleColour { get; set; }
        public string DecalColour { get; set; }
        public string OverdriveLampColour { get; set; }
        public string BottomCoverFill { get; set; }
        public string OverdriveLampOffColour { get; set; }
        public string MeterLabelForeground { get; set; }
        // Seek slider
        public string SliderThumbGlowOverlay { get; set; }
        public string SliderThumbGripBarBackground { get; set; }
        public string SliderThumbPointBackground { get; set; }
        public string SliderThumbBorder { get; set; }
        public string SliderThumbForeground { get; set; }
        public string SliderThumbMouseOverBackground { get; set; }
        public string SliderThumbMouseOverBorder { get; set; }
        public string SliderThumbPressedBackground { get; set; }
        public string SliderThumbPressedBorder { get; set; }
        public string SliderThumbDisabledBackground { get; set; }
        public string SliderThumbDisabledBorder { get; set; }
        public string SliderThumbTrackBackground { get; set; }
        public string SliderThumbTrackBorder { get; set; }
        // Playback
        public string SkipToStartButtonFill { get; set; }
        public string StopButtonFill { get; set; }
        public string PlayButtonFill { get; set; }
        public string PauseButtonFill { get; set; }
        public string SkipToEndButtonFill { get; set; }
        public string SelectButtonFill { get; set; }
        public string SwitchOnBackground { get; set; }
        public string SwitchOffBackground { get; set; }
        public string SwitchForeground { get; set; }
        // Gain slider
        public string GainSliderMidBarFill { get; set; }
        public string GainSliderTextForeground { get; set; }
        public string GainSliderTickForeground { get; set; }
        // Power button
        public string PowerButtonLightFill { get; set; }
        public string PowerButtonStrokeFill { get; set; }
        public string PowerButtonHighlight { get; set; }
        // Stereo Indicator
        public string MonoOnFill { get; set; }
        public string MonoOffFill { get; set; }
        public string StereoOnFill { get; set; }
        public string StereoOffFill { get; set; }
        public string LabelForeground { get; set; }
        // Playback indicator control
        public string PlaybackIndicatorOffColour { get; set; }
        public string PlaybackIndicatorOnColour { get; set; }

        #endregion

        #region Methods

        #region Defaults

        public static ThematicModel GetDefaultSettings() => new()
        {
            BackgroundColour = DefaultBackgroundColour,
            NeedleColour = DefaultNeedleColour,
            DecalColour = DefaultDecalColour,
            OverdriveLampColour = DefaultOverdriveLampColour,
            BottomCoverFill = DefaultBottomCoverFill,
            OverdriveLampOffColour = DefaultOverdriveLampOffColour,
            MeterLabelForeground = DefaultMeterLabelForeground,
            SliderThumbGlowOverlay = DefaultSliderThumbGlowOverlay,
            SliderThumbGripBarBackground = DefaultSliderThumbGripBarBackground,
            SliderThumbPointBackground = DefaultSliderThumbPointBackground,
            SliderThumbBorder = DefaultSliderThumbBorder,
            SliderThumbForeground = DefaultSliderThumbForeground,
            SliderThumbMouseOverBackground = DefaultSliderThumbMouseOverBackground,
            SliderThumbMouseOverBorder = DefaultSliderThumbMouseOverBorder,
            SliderThumbPressedBackground = DefaultSliderThumbPressedBackground,
            SliderThumbPressedBorder = DefaultSliderThumbPressedBorder,
            SliderThumbDisabledBackground = DefaultSliderThumbDisabledBackground,
            SliderThumbDisabledBorder = DefaultSliderThumbDisabledBorder,
            SliderThumbTrackBackground = DefaultSliderThumbTrackBackground,
            SliderThumbTrackBorder = DefaultSliderThumbTrackBorder,
            SkipToStartButtonFill = DefaultSkipToStartButtonFill,
            StopButtonFill = DefaultStopButtonFill,
            PlayButtonFill = DefaultPlayButtonFill,
            PauseButtonFill = DefaultPauseButtonFill,
            SkipToEndButtonFill = DefaultSkipToEndButtonFill,
            SelectButtonFill = DefaultSelectButtonFill,
            SwitchOnBackground = DefaultSwitchOnBackground,
            SwitchOffBackground = DefaultSwitchOffBackground,
            SwitchForeground = DefaultSwitchForeground,
            GainSliderMidBarFill = DefaultGainSliderMidBarFill,
            GainSliderTextForeground = DefaultGainSliderTextForeground,
            GainSliderTickForeground = DefaultGainSliderTickForeground,
            PowerButtonLightFill = DefaultPowerButtonLightFill,
            PowerButtonStrokeFill = DefaultPowerButtonStrokeFill,
            PowerButtonHighlight = DefaultPowerButtonHighlight,
            MonoOnFill = DefaultMonoOnFill,
            MonoOffFill = DefaultMonoOffFill,
            StereoOnFill = DefaultStereoOnFill,
            StereoOffFill = DefaultStereoOffFill,
            LabelForeground = DefaultLabelForeground,
            PlaybackIndicatorOffColour = DefaultPlaybackIndicatorOffColour,
            PlaybackIndicatorOnColour = DefaultPlaybackIndicatorOnColour,
        //Application
        ApplicationBackgroundFill = DefaultApplicationBackgroundFill,
            ComponentBackgroundFill = DefaultComponentBackgroundColour,
            ComponentForegroundColour = DefaultComponentForegroundColour,
            ComponentHighlightColour = DefaultComponentHighlightColour,
            ComponentBorderColour = DefaultComponentBorderColour,
            ApplicationBorderColour = DefaultApplicationBorderColour,
            ListTextForegroundColour = DefaultListTextForegroundColour,
            ComponentWindowsDefaultBackgroundColour = DefaultComponentWindowsBackgroundColour,
            ApplicationForegroundColour = DefaultApplicationForegroundColour,
            ButtonContentForegroundColour = DefaultButtonContentForegroundColour,
            Description = "Default Theme",
            ThematicFileName = "DefaultTheme.xml",
            ImagePath = Path.Combine("Resources/Themes", "AudioPlayerFacePlateRounded.png"),
            Id = DefaultThemeGuid,
        };

        public ThematicModel Clone()
        {
            return (ThematicModel)this.MemberwiseClone();
        }

        #endregion

        #endregion
    }
}
