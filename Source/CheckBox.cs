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

    /// <summary>
    /// Represents a control that displays a text string.
    /// </summary>
    /// <inheritdoc />
    internal sealed class CheckBox : Control {

        public bool Checked { get; set; }
        public string Text { get; set; }

        public event Action<bool> CheckedChanged;

        private int state;

        public CheckBox(int x, int y, string text) {
            X = x;
            Y = y;
            Width = 24;
            Height = 24;
            Text = text;
            state = 0;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            int baseOffset = Checked ? 72 : 0;
            spriteBatch.Draw(Assets.GetTexture("ui/checkbox"), new Vector2(EffX, EffY), new Rectangle(baseOffset + state * 24, 0, 24, 24), Color.White);
            spriteBatch.DrawString(Assets.fontDefault, Text, new Vector2(EffX + 35, EffY + 4), Color.White);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer || !Visible) return;

            switch (state) {
                case 0: // normal
                    if (Input.GetMouseInArea(EffX, EffY, Width, Height)) state = 1;
                    break;
                case 1: // hover
                    if (!Input.GetMouseInArea(EffX, EffY, Width, Height)) {
                        state = 0;
                        break;
                    }
                    if (Input.GetMouseLeftDown()) {
                        Audio.PlaySound("event:/UI/Select");
                        state = 2;
                    }
                    break;
                case 2: // down
                    if (Input.GetMouseLeftUp()) {
                        state = 0;
                        if (Input.GetMouseInArea(EffX, EffY, Width, Height)) {
                            Checked = !Checked;
                            CheckedChanged?.Invoke(Checked);
                        }
                    }
                    break;
                case 3: // disabled
                    state = 0;
                    break;
            }
        }

    }

}
