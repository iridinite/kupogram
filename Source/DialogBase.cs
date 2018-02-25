/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <inheritdoc />
    /// <summary>
    /// Base class for dialog boxes.
    /// </summary>
    internal abstract class DialogBase : Anchor {

        protected string Title { get; set; } = String.Empty;

        protected DialogBase(int width, int height)
            : base(-width / 2, -height / 2, AnchorMode.RelativeCenter) {
            Width = width;
            Height = height;
            DrawMode = ControlDrawMode.ChildrenLast;
            Audio.PlaySound("event:/UI/Open");
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            // darken everything behind the topmost layer
            if (IsActiveLayer)
                spriteBatch.Draw(Assets.GetTexture("ui/rect"), new Rectangle(0, 0, Application.Inst.GameWidth, Application.Inst.GameHeight), Color.Black * 0.5f);

            // dialog background and title text
            spriteBatch.Draw9Slice(Assets.GetTexture("ui/dialog"), new Rectangle(EffX, EffY, Width, Height), Color.White, 40);
            spriteBatch.DrawStringShadow(Assets.fontDefault, Title, new Vector2(EffX + 7, EffY + 7), 2, Color.White, Color.Black);
        }

        protected void Close() {
            Audio.PlaySound("event:/UI/Dismiss");
            Application.Inst.PopUI();
        }

    }

}
