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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Kupogram {

    /// <summary>
    /// Common functionality used throughout the program.
    /// </summary>
    internal static class Util {

        internal static readonly DirectoryInfo SaveDir = new DirectoryInfo(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            Path.DirectorySeparatorChar + "My Games" +
            Path.DirectorySeparatorChar + "Kupogram");

        internal static readonly DirectoryInfo BaseDir = new DirectoryInfo(
            AppDomain.CurrentDomain.BaseDirectory);

        internal static readonly DirectoryInfo PuzzleFactoryDir = new DirectoryInfo(
            BaseDir.FullName + Path.DirectorySeparatorChar + "Puzzles");

        internal static readonly DirectoryInfo PuzzleCustomDir = new DirectoryInfo(
            SaveDir.FullName + Path.DirectorySeparatorChar + "Puzzles");

        internal static readonly DirectoryInfo ConfigDir = new DirectoryInfo(
            SaveDir.FullName + Path.DirectorySeparatorChar + "Save");

        public static List<Board> PuzzleCatalog { get; } = new List<Board>();
        public static List<Board> EditableCatalog { get; } = new List<Board>();

        /// <summary>
        /// A public RNG object that may be used for whatever purpose.
        /// </summary>
        public static readonly Random RNG = new Random();

        /// <summary>
        /// Static initializer
        /// </summary>
        static Util() {
            Debug.Assert(HasWritePermission());

            if (!SaveDir.Exists)
                SaveDir.Create();
            if (!ConfigDir.Exists)
                ConfigDir.Create();
            //if (!PuzzleFactoryDir.Exists) // write access to app dir not guaranteed
            //    PuzzleFactoryDir.Create();
            if (!PuzzleCustomDir.Exists)
                PuzzleCustomDir.Create();
            //Writer = new StreamWriter(BaseDir.FullName + Path.DirectorySeparatorChar + "output.log");
        }

        /// <summary>
        /// Verifies if the calling thread has permission to write a file to the application directory.
        /// </summary>
        private static bool HasWritePermission() {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, SaveDir.FullName);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        /// <summary>
        /// Reloads the list of puzzles, and builds preview images for them.
        /// </summary>
        public static void RefreshPuzzleCache() {
            // get rid of existing items in the list (deallocate preview images on the GPU, too)
            PuzzleCatalog.ForEach(board => board.Preview?.Dispose());
            PuzzleCatalog.Clear();
            EditableCatalog.Clear();

            // load all board files
            var files = PuzzleFactoryDir.GetFiles("*.kgram").Concat(PuzzleCustomDir.GetFiles("*.kgram"));
            foreach (var file in files) {
                try {
                    // parse from file
                    var board = Board.FromFile(file.FullName);
#if !DEBUG
                    // disallow editing of boards that the user didn't make
                    if (board.Author == PlayArchive.Identity)
#endif
                        EditableCatalog.Add(board);
                    // add to the list
                    PuzzleCatalog.Add(board);
                } catch (InvalidDataException) {}
            }
        }

        /// <summary>
        /// Writes a text file to the application directory describing an <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to describe.</param>
        public static void WriteExceptionReport(Exception ex) {
            if (!HasWritePermission()) return;

            try {
                using (StreamWriter exWriter = new StreamWriter($"{SaveDir.FullName}{Path.DirectorySeparatorChar}crash{DateTime.Now.ToFileTime()}.log")) {
                    exWriter.WriteLine("===================================");
                    exWriter.WriteLine("EXCEPTION REPORT");
                    exWriter.WriteLine("===================================");
                    exWriter.WriteLine();

                    exWriter.WriteLine("Kupogram " + Assembly.GetExecutingAssembly().GetName().Version);
                    exWriter.WriteLine($"Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
                    exWriter.WriteLine("OS: " + Environment.OSVersion);
                    exWriter.WriteLine("Target Site: " + ex.TargetSite);
                    exWriter.WriteLine();

                    exWriter.WriteLine(ex);

                    exWriter.WriteLine();
                    exWriter.WriteLine("===================================");
                    exWriter.WriteLine("END OF EXCEPTION REPORT");
                    exWriter.WriteLine("===================================");
                }
            } catch (Exception) {
                // we're already failing if this method is called at all,
                // no point in throwing more exceptions if logging fails.
            }
        }

    }

}
