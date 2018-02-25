/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    internal static class Assets {

        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public static SpriteFont
            fontDefault,
            fontBold,
            fontTooltip;

        public static SpriteFont[]
            fontClues;

        public static Texture2D GetTexture(string name) {
            name = name.ToLowerInvariant();
            if (!textures.ContainsKey(name))
                throw new KeyNotFoundException($"Texture '{name}' was not found.");
            return textures[name];
        }

        private static string CleanFilename(FileInfo file, DirectoryInfo dir) {
            string shortname = file.FullName.Substring(dir.FullName.Length).Replace('\\', '/').ToLowerInvariant();
            shortname = shortname.Substring(0, shortname.Length - file.Extension.Length);
            return shortname;
        }

        public static void LoadContent(GraphicsDevice graphicsDevice, ContentManager content) {
            // load font textures
            fontDefault = content.Load<SpriteFont>("Fonts/Default");
            fontDefault.LineSpacing += 5;
            fontBold = content.Load<SpriteFont>("Fonts/Bold");
            fontTooltip = content.Load<SpriteFont>("Fonts/Tooltip");

            fontClues = new SpriteFont[5];
            fontClues[0] = content.Load<SpriteFont>("Fonts/Clues12");
            fontClues[1] = content.Load<SpriteFont>("Fonts/Clues16");
            fontClues[2] = content.Load<SpriteFont>("Fonts/Clues24");
            fontClues[3] = content.Load<SpriteFont>("Fonts/Clues32");
            fontClues[4] = content.Load<SpriteFont>("Fonts/Clues48");

            // get folder containing texture and sprite files
            DirectoryInfo texturesDir = new DirectoryInfo($"{content.RootDirectory}{Path.DirectorySeparatorChar}Textures{Path.DirectorySeparatorChar}");

            // enumerate and load all textures
            foreach (FileInfo file in texturesDir.GetFiles("*.xnb", SearchOption.AllDirectories)) {
                string shortname = CleanFilename(file, texturesDir);
                textures.Add(shortname, content.Load<Texture2D>("Textures/" + shortname));
            }
        }

    }

}
