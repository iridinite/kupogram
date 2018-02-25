/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kupogram {
    
    /// <summary>
    /// A collection of methods for dealing with sound output.
    /// </summary>
    internal static class Audio {

        private static FMOD.Studio.System fmodsys;
        private static List<FMOD.Studio.Bank> banks;
        private static FMOD.Studio.EventInstance playingMusic;

        private static void ErrCheck(FMOD.RESULT r) {
            if (r == FMOD.RESULT.OK) return;

            Debug.Fail("FMOD error: " + FMOD.Error.String(r));
        }

        /// <summary>
        /// Initializes the FMOD backend and loads all the game's sound banks.
        /// </summary>
        public static void Initialize() {
            // create and initialize system object
            ErrCheck(FMOD.Studio.System.create(out fmodsys));
#if DEBUG
            ErrCheck(fmodsys.initialize(512, FMOD.Studio.INITFLAGS.LIVEUPDATE, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
#else
            ErrCheck(fmodsys.initialize(512, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
#endif

            // load banks
            banks = new List<FMOD.Studio.Bank>();
            LoadBank("Master.bank");
            LoadBank("Master.strings.bank");
            LoadBank("Music.bank");
        }

        /// <summary>
        /// Unloads the FMOD system and all sound banks.
        /// </summary>
        public static void Shutdown() {
            if (fmodsys == null) return;

            // unload banks
            foreach (var bank in banks)
                ErrCheck(bank.unload());
            banks.Clear();

            // release the system object
            ErrCheck(fmodsys.release());
            fmodsys = null;
        }

        /// <summary>
        /// Perform frame updates for FMOD.
        /// </summary>
        public static void Update() {
            // backend update
            ErrCheck(fmodsys.update());

            // set music volume
            if (playingMusic != null)
                ErrCheck(playingMusic.setVolume(UserConfig.VolumeMusic / 10f));
        }

        /// <summary>
        /// Load a sound bank compiled by FMOD Studio into memory.
        /// </summary>
        /// <param name="file">The name of the bank file, excluding path.</param>
        private static void LoadBank(string file) {
            ErrCheck(fmodsys.loadBankFile("Content/Audio/" + file, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out var bank));
            banks.Add(bank);
        }

        /// <summary>
        /// Plays the specified event as a fire-and-forget oneshot.
        /// </summary>
        /// <param name="path">The Studio path to the event.</param>
        public static void PlaySound(string path) {
            ErrCheck(fmodsys.getEvent(path, out var eventdesc));
            ErrCheck(eventdesc.createInstance(out var eventinst));
            ErrCheck(eventinst.setVolume(UserConfig.VolumeSound / 10f));
            ErrCheck(eventinst.start());
            ErrCheck(eventinst.release());
        }

        /// <summary>
        /// Stops any previously playing song, and starts the specified one.
        /// </summary>
        /// <param name="path">The Studio path to the event.</param>
        public static void PlayMusic(string path) {
            if (playingMusic != null) {
                ErrCheck(playingMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT));
                ErrCheck(playingMusic.release());
                playingMusic = null;
            }

            ErrCheck(fmodsys.getEvent(path, out var eventdesc));
            ErrCheck(eventdesc.createInstance(out playingMusic));
            ErrCheck(playingMusic.setVolume(UserConfig.VolumeMusic / 10f));
            ErrCheck(playingMusic.start());
        }

    }

}
