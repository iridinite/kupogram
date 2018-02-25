/*
** Project ShiftDrive
** (C) Mika Molenkamp, 2016-2017.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Fires when the user clicks the associated <see cref="Control"/>.
    /// </summary>
    /// <param name="sender">The <see cref="Control"/> that initiated the event.</param>
    internal delegate void OnClickHandler(Control sender);

    /// <summary>
    /// Represents an interactive button.
    /// </summary>
    /// <inheritdoc />
    internal abstract class Button : Control {

        public bool Enabled { get; set; }
        public bool CancelSound { get; set; }

        public event OnClickHandler OnClick;

        protected int state;

        private Tooltip tooltip;

        protected Button(int x, int y, int width, int height) {
            // x = -1 means that the button should be centered
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
            Enabled = true;
            CancelSound = false;
            state = 0;
            tooltip = null;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            spriteBatch.Draw9Slice(Assets.GetTexture("ui/button"), new Rectangle(EffX, EffY, Width, Height), Color.White, 8, state);

            if (Visible && Enabled && IsActiveLayer)
                tooltip?.Draw();
        }

        protected override void OnUpdate(GameTime gameTime) {
            // disabled state
            if (!Enabled) {
                state = 3;
                return;
            }

            // do not respond to user input while animating or on an inactive layer
            if (!Visible || !IsActiveLayer)
                return;

            // update tooltip position and visibility
            tooltip?.SetTrigger(new Rectangle(EffX, EffY, Width, Height));
            tooltip?.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

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
                        Audio.PlaySound("event:/UI/Confirm");
                        state = 2;
                    }
                    break;
                case 2: // down
                    if (Input.GetMouseLeftUp()) {
                        state = 0;
                        if (Input.GetMouseInArea(EffX, EffY, Width, Height)) {
                            if (CancelSound)
                                Audio.PlaySound("event:/UI/Dismiss");
                            OnClick?.Invoke(this);
                        }
                    }
                    break;
                case 3: // disabled
                    state = 0;
                    break;
            }
        }

        public void SetTooltip(string text) {
            tooltip = new Tooltip(new Rectangle(EffX, EffY, Width, Height), text);
        }

    }

}
