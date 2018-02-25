/*
** Project ShiftDrive
** (C) Mika Molenkamp, 2016-2017.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// A container for extension methods.
    /// </summary>
    internal static class Extensions {

        /// <summary>
        /// Iterates over a collection and invokes the specified delegate on each item.
        /// </summary>
        /// <param name="source">The collection to iterate through.</param>
        /// <param name="fn">The function to invoke for each item.</param>
        public static void ForEach<TResult>(this IEnumerable<TResult> source, Action<TResult> fn) {
            var copy = source.ToArray(); // copy protects against enumerable modification errors
            foreach (var item in copy) {
                fn(item);
            }
        }

        /// <summary>
        /// Word-wraps a string so that it will not overflow a box of the specified width.
        /// </summary>
        /// <param name="font">A <see cref="SpriteFont"/> to reference for measurement.</param>
        /// <param name="text">The text to wrap.</param>
        /// <param name="width">The line-wrap edge, in pixels.</param>
        [Pure]
        public static string WrapText(this SpriteFont font, string text, float width) {
            StringBuilder result = new StringBuilder();
            StringBuilder totalline = new StringBuilder();
            string[] lines = text.Replace("\x0D", "").Split('\x0A'); // strip \r, split on \n

            for (int i = 0; i < lines.Length; i++) {
                // add words until overflow, then line-break
                totalline.Clear();
                string line = lines[i];
                string[] words = line.Split(' ');
                foreach (string word in words) {
                    if (font.MeasureString(totalline + word).X > width) {
                        result.AppendLine(totalline.ToString());
                        totalline.Clear();
                    }
                    totalline.Append(word);
                    totalline.Append(' ');
                }
                // don't leave off the last line
                if (totalline.Length > 0)
                    result.Append(totalline);
                // add line breaks in between, but not on the last line
                if (i < lines.Length)
                    result.AppendLine();
            }

            return result.ToString();
        }

        /// <summary>
        /// Draws a background with a border, using a 9-sliced texture.
        /// </summary>
        /// <param name="spriteBatch">The <seealso cref="SpriteBatch"/> instance to draw with.</param>
        /// <param name="tex">The sliced texture to use. Subtextures are expected to be evenly split in a 3x3 grid.</param>
        /// <param name="dest">The screen-coordinate rectangle at which to draw the border and background.</param>
        /// <param name="tilesize">The size, in pixels, of individual tiles (square, both width and height).</param>
        public static void Draw9Slice(this SpriteBatch spriteBatch, Texture2D tex, Rectangle dest, Color color, int tilesize, int state = 0) {
            int doublesize = tilesize * 2;
            int xoffset = state * tilesize * 3;

            spriteBatch.Draw(tex,
                new Rectangle(dest.X, dest.Y, tilesize, tilesize),
                new Rectangle(xoffset, 0, tilesize, tilesize), color); // top left
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + tilesize, dest.Y, dest.Width - doublesize, tilesize),
                new Rectangle(xoffset + tilesize, 0, tilesize, tilesize), color); // top middle
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + dest.Width - tilesize, dest.Y, tilesize, tilesize),
                new Rectangle(xoffset + doublesize, 0, tilesize, tilesize), color); // top right

            spriteBatch.Draw(tex,
                new Rectangle(dest.X, dest.Y + tilesize, tilesize, dest.Height - doublesize),
                new Rectangle(xoffset, tilesize, tilesize, tilesize), color); // middle left
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + tilesize, dest.Y + tilesize, dest.Width - doublesize, dest.Height - doublesize),
                new Rectangle(xoffset + tilesize, tilesize, tilesize, tilesize), color); // center
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + dest.Width - tilesize, dest.Y + tilesize, tilesize, dest.Height - doublesize),
                new Rectangle(xoffset + doublesize, tilesize, tilesize, tilesize), color); // middle right

            spriteBatch.Draw(tex,
                new Rectangle(dest.X, dest.Y + dest.Height - tilesize, tilesize, tilesize),
                new Rectangle(xoffset, doublesize, tilesize, tilesize), color); // bottom left
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + tilesize, dest.Y + dest.Height - tilesize, dest.Width - doublesize, tilesize),
                new Rectangle(xoffset + tilesize, doublesize, tilesize, tilesize), color); // bottom middle
            spriteBatch.Draw(tex,
                new Rectangle(dest.X + dest.Width - tilesize, dest.Y + dest.Height - tilesize, tilesize, tilesize),
                new Rectangle(xoffset + doublesize, doublesize, tilesize, tilesize), color); // bottom right
        }

        /// <summary>
        /// Draws a string with a shadow beneath it. Parameters are mostly the same to SpriteBatch.DrawString.
        /// </summary>
        public static void DrawStringShadow(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, int shadowOffset, Color color, Color shadow) {
            spriteBatch.DrawStringShadow(font, text, position, shadowOffset, color, shadow, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
        }

        /// <summary>
        /// Draws a string with a shadow beneath it. Parameters are mostly the same to SpriteBatch.DrawString.
        /// </summary>
        public static void DrawStringShadow(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, int shadowOffset, Color color, Color shadow, float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip) {
            spriteBatch.DrawString(font, text, position + new Vector2(shadowOffset), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, flip, 0f);
        }

        /// <summary>
        /// Draws a string with a colored outline. Parameters are mostly the same as SpriteBatch.DrawString.
        /// </summary>
        public static void DrawStringOutline(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, int outlineOffset, Color color, Color shadow) {
            spriteBatch.DrawStringOutline(font, text, position, outlineOffset, color, shadow, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None);
        }

        /// <summary>
        /// Draws a string with a colored outline. Parameters are mostly the same as SpriteBatch.DrawString.
        /// </summary>
        public static void DrawStringOutline(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, int outlineOffset, Color color, Color shadow, float rotation, Vector2 origin, Vector2 scale, SpriteEffects flip) {
            spriteBatch.DrawString(font, text, position + new Vector2(outlineOffset, outlineOffset), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(-outlineOffset, outlineOffset), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(outlineOffset, -outlineOffset), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(-outlineOffset, -outlineOffset), shadow, rotation, origin, scale, flip, 0f);

            spriteBatch.DrawString(font, text, position + new Vector2(outlineOffset, 0), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(-outlineOffset, 0), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(0, outlineOffset), shadow, rotation, origin, scale, flip, 0f);
            spriteBatch.DrawString(font, text, position + new Vector2(0, -outlineOffset), shadow, rotation, origin, scale, flip, 0f);

            spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, flip, 0f);
        }

    }

}
