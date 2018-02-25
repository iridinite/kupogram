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
    /// Draws a parallaxing background image.
    /// </summary>
    /// <inheritdoc />
    internal class Background : Control {

        private readonly BoardRenderer renderer;

        public Background(BoardRenderer renderer) {
            this.renderer = renderer;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(Assets.GetTexture("board/back_lpaper"),
                Vector2.Zero,
                new Rectangle(-renderer.Position.x / 12, -renderer.Position.y / 12, Application.Inst.GameWidth, Application.Inst.GameHeight),
                Color.White);
        }

    }

}
