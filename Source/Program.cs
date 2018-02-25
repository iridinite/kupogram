/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;

namespace Kupogram {

    public static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() {
            // handler for logging crashes
            AppDomain.CurrentDomain.UnhandledException += Program_UnhandledException;
            // run the game
            using (var game = new Application())
                game.Run();
        }

        private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e.ExceptionObject is Exception ex)
                Util.WriteExceptionReport(ex);
        }

    }

}
