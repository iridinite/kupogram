/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Represents a control that displays a text string.
    /// </summary>
    /// <inheritdoc />
    internal sealed class TextLabel : Control {

        public string Text { get; set; }

        public SpriteFont Font { get; set; }
        public Color TextColor { get; set; }

        public TextLabel(int x, int y, string text, SpriteFont font = null, Color? color = null) {
            X = x;
            Y = y;
            Text = text;
            Font = font ?? Assets.fontDefault;
            TextColor = color ?? Color.White;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(Font, Text, new Vector2(EffX, EffY), TextColor);
        }

    }

}
