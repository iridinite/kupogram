/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kupogram {

    /// <summary>
    /// Persistently records player progress, such as a list of previously solved puzzles.
    /// </summary>
    internal static class PlayArchive {

        private const byte ArchiveVersion = 1;
        private static readonly string ArchiveFile = Util.ConfigDir.FullName + Path.DirectorySeparatorChar + "archive.bin";

        private static readonly List<Guid> SolvedPuzzles = new List<Guid>();

        /// <summary>
        /// The unique identity of this player.
        /// </summary>
        public static Guid Identity { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the specified puzzle ID has already been solved.
        /// </summary>
        public static bool IsSolved(Guid ID) {
            return SolvedPuzzles.Contains(ID);
        }

        /// <summary>
        /// Marks the specified puzzle ID as having been solved.
        /// </summary>
        public static void MarkSolved(Guid ID) {
            if (!SolvedPuzzles.Contains(ID))
                SolvedPuzzles.Add(ID);
        }

        /// <summary>
        /// Loads the play archive from persistent storage, or re-initializes it.
        /// </summary>
        public static void Load() {
            SolvedPuzzles.Clear();

            // if the archive file does not exist, assume new player
            if (!File.Exists(ArchiveFile)) {
                Identity = Guid.NewGuid();
                return;
            }

            try {
                using (StreamReader stream = new StreamReader(ArchiveFile)) {
                    using (BinaryReader reader = new BinaryReader(stream.BaseStream, Encoding.UTF8, true)) {
                        if (reader.ReadByte() != ArchiveVersion) return;

                        // load player identity
                        Identity = new Guid(reader.ReadBytes(16));

                        // load list of previously solved puzzles
                        int numSolved = reader.ReadInt32();
                        while (numSolved > 0) {
                            SolvedPuzzles.Add(new Guid(reader.ReadBytes(16)));
                            numSolved--;
                        }
                    }
                }
            } catch (IOException) {
                // read fail.. assume new identity?
                // TODO: notify player
                Identity = Guid.NewGuid();
            }
        }

        /// <summary>
        /// Saves the play archive to persistent storage.
        /// </summary>
        public static void Save() {
            try {
                using (StreamWriter stream = new StreamWriter(ArchiveFile)) {
                    using (BinaryWriter writer = new BinaryWriter(stream.BaseStream, Encoding.UTF8, true)) {
                        writer.Write(ArchiveVersion);
                        writer.Write(Identity.ToByteArray());
                        writer.Write(SolvedPuzzles.Count);
                        SolvedPuzzles.ForEach(id => writer.Write(id.ToByteArray()));
                    }
                }
            } catch (IOException) {}
        }

    }

}
