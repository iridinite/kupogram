/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using Microsoft.Xna.Framework;

namespace Kupogram {

    /// <summary>
    /// Options menu dialog box.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogOptions : DialogBase {

        private readonly TextLabel lblVolSound, lblVolMusic;
        private readonly Button btnVolSoundDown, btnVolSoundUp, btnVolMusicDown, btnVolMusicUp;

        public DialogOptions()
            : base(500, 350) {
            Title = "Options";

            // labels for volume headers and settings
            lblVolSound = new TextLabel(100, 85, String.Empty);
            lblVolMusic = new TextLabel(100, 85, String.Empty);
            AddChild(new TextLabel(15, 48, "Sound Volume:"));
            AddChild(new TextLabel(275, 48, "Music Volume:"));
            AddChild(lblVolSound);
            AddChild(lblVolMusic);

            // up/down buttons for sound and music volumes
            btnVolSoundDown = new TextButton(15, 80, 30, 30, "-");
            btnVolSoundDown.OnClick += sender => ChangeVolume(-1, 0);
            btnVolSoundUp = new TextButton(200, 80, 30, 30, "+");
            btnVolSoundUp.OnClick += sender => ChangeVolume(1, 0);
            btnVolMusicDown = new TextButton(275, 80, 30, 30, "-");
            btnVolMusicDown.OnClick += sender => ChangeVolume(0, -1);
            btnVolMusicUp = new TextButton(455, 80, 30, 30, "+");
            btnVolMusicUp.OnClick += sender => ChangeVolume(0, 1);
            AddChild(btnVolSoundUp);
            AddChild(btnVolSoundDown);
            AddChild(btnVolMusicUp);
            AddChild(btnVolMusicDown);

            // game settings
            var chkShowTitles = new CheckBox(15, 150, "Show titles of unsolved puzzles");
            chkShowTitles.CheckedChanged += val => UserConfig.ShowPuzzleNames = val;
            chkShowTitles.Checked = UserConfig.ShowPuzzleNames;
            AddChild(chkShowTitles);
            var chkShowClock = new CheckBox(15, 190, "Show in-game clock");
            chkShowClock.CheckedChanged += val => UserConfig.ShowTimer = val;
            chkShowClock.Checked = UserConfig.ShowTimer;
            AddChild(chkShowClock);
            var chkAutoCrossout = new CheckBox(15, 230, "Automatically cross out lines");
            chkAutoCrossout.CheckedChanged += val => UserConfig.AutoCrossout = val;
            chkAutoCrossout.Checked = UserConfig.AutoCrossout;
            AddChild(chkAutoCrossout);

            // close button
            var btnClose = new TextButton(335, 295, 150, 40, "Close");
            btnClose.CancelSound = true;
            btnClose.OnClick += sender => Application.Inst.PopUI();
            AddChild(btnClose);

            // set up the volume state labels with correct text
            UpdateVolumeText();
        }

        private void UpdateVolumeText() {
            lblVolSound.Text = $"{UserConfig.VolumeSound * 10}%";
            lblVolSound.X = 125 - (int)Assets.fontDefault.MeasureString(lblVolSound.Text).X / 2;
            lblVolMusic.Text = $"{UserConfig.VolumeMusic * 10}%";
            lblVolMusic.X = 380 - (int)Assets.fontDefault.MeasureString(lblVolMusic.Text).X / 2;

            btnVolSoundDown.Enabled = UserConfig.VolumeSound > 0;
            btnVolSoundUp.Enabled = UserConfig.VolumeSound < 10;
            btnVolMusicDown.Enabled = UserConfig.VolumeMusic > 0;
            btnVolMusicUp.Enabled = UserConfig.VolumeMusic < 10;
        }

        private void ChangeVolume(int sound, int music) {
            UserConfig.VolumeSound = MathHelper.Clamp(UserConfig.VolumeSound + sound, 0, 10);
            UserConfig.VolumeMusic = MathHelper.Clamp(UserConfig.VolumeMusic + music, 0, 10);
            UpdateVolumeText();
        }

    }

}
