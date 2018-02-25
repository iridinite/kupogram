/*
** Project ShiftDrive
** (C) Mika Molenkamp, 2016-2017.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Represents an interactive text button.
    /// </summary>
    /// <inheritdoc />
    internal sealed class TextButton : Button {

        public string Caption { get; set; }

        public TextButton(int x, int y, int width, int height, string caption)
            : base(x, y, width, height) {
            this.Caption = caption;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            base.OnDraw(spriteBatch);

            var offset = state == 2 ? 1 : 0;
            var size = Assets.fontDefault.MeasureString(Caption);
            spriteBatch.DrawString(
                Assets.fontDefault,
                Caption,
                new Vector2(
                    (int)(EffX + Width / 2 - size.X / 2 + offset),
                    (int)(EffY + Height / 2 - size.Y / 2 + 4 + offset)), // +4 compensates for line spacing
                Color.Black);
        }

    }

}
