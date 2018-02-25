/*
** Project ShiftDrive
** (C) Mika Molenkamp, 2016-2017.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Represents an interactive button with an image label.
    /// </summary>
    /// <inheritdoc />
    internal sealed class ImageButton : Button {

        private Texture2D image;
        private Rectangle? imageSource;
        private Vector2 imageSize;
        private Color imageColor;

        public ImageButton(int x, int y, int width, int height, Texture2D image)
            : base(x, y, width, height) {
            this.image = image;
            imageColor = Color.White;
            SetSourceRect(null);
        }

        public ImageButton(int x, int y, int width, int height, Texture2D image, Color color)
            : this(x, y, width, height, image) {
            imageColor = color;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            base.OnDraw(spriteBatch);

            int textOffset = (state == 2) ? 2 : 0;
            spriteBatch.Draw(image,
                new Rectangle(
                    (int)(EffX + Width / 2 - imageSize.X / 2),
                    (int)(EffY + Height / 2 - imageSize.Y / 2 + textOffset),
                    (int)imageSize.X,
                    (int)imageSize.Y),
                imageSource,
                imageColor);
        }

        public void SetSourceRect(Rectangle? rect) {
            imageSource = rect;
            imageSize = rect.HasValue
                ? new Vector2(rect.Value.Width, rect.Value.Height)
                : new Vector2(image.Width, image.Height);
        }

    }

}
