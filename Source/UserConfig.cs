/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System.IO;
using System.Text;

namespace Kupogram
{

    /// <summary>
    /// Manages user-specified game options and handles their persistence and serialization.
    /// </summary>
    internal static class UserConfig {

        private const byte ConfigVersion = 1;
        private static readonly string OptionsFile = Util.ConfigDir.FullName + Path.DirectorySeparatorChar + "options.bin";

        /// <summary>
        /// SFX volume level, 0-10 inclusive
        /// </summary>
        public static int VolumeSound { get; set; } = 10;

        /// <summary>
        /// Music volume level, 0-10 inclusive
        /// </summary>
        public static int VolumeMusic { get; set; } = 9;

        /// <summary>
        /// Whether to show the editor intro popup
        /// </summary>
        public static bool ShowEditorIntro { get; set; } = true;

        /// <summary>
        /// Whether to draw the puzzle timer on the game board
        /// </summary>
        public static bool ShowTimer { get; set; } = true;

        /// <summary>
        /// Whether to show the names of unsolved puzzles
        /// </summary>
        public static bool ShowPuzzleNames { get; set; } = false;

        /// <summary>
        /// Whether to mark cells as CrossoutAuto when lines are completed
        /// </summary>
        public static bool AutoCrossout { get; set; } = true;

        /// <summary>
        /// Background index to use
        /// </summary>
        public static int Background { get; set; } = 0;

        /// <summary>
        /// Load the user config from persistent storage.
        /// </summary>
        public static void Load() {
            if (!File.Exists(OptionsFile)) return;

            try {
                using (StreamReader stream = new StreamReader(OptionsFile)) {
                    using (BinaryReader reader = new BinaryReader(stream.BaseStream, Encoding.UTF8, true)) {
                        if (reader.ReadByte() != ConfigVersion) return;

                        VolumeMusic = reader.ReadByte();
                        VolumeSound = reader.ReadByte();
                        ShowEditorIntro = reader.ReadBoolean();
                        ShowTimer = reader.ReadBoolean();
                        ShowPuzzleNames = reader.ReadBoolean();
                        AutoCrossout = reader.ReadBoolean();
                        Background = reader.ReadInt32();
                    }
                }
            } catch (IOException ex) {
                
            }
        }

        /// <summary>
        /// Save the user configuration to persistent storage.
        /// </summary>
        public static void Save() {
            try {
                using (StreamWriter stream = new StreamWriter(OptionsFile)) {
                    using (BinaryWriter writer = new BinaryWriter(stream.BaseStream, Encoding.UTF8, true)) {
                        writer.Write(ConfigVersion);
                        writer.Write((byte)VolumeMusic);
                        writer.Write((byte)VolumeSound);
                        writer.Write(ShowEditorIntro);
                        writer.Write(ShowTimer);
                        writer.Write(ShowPuzzleNames);
                        writer.Write(AutoCrossout);
                        writer.Write(Background);
                    }
                }
            } catch (IOException ex) {
                
            }
        }

    }

}
