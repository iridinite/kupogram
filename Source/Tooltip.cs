/*
** Project ShiftDrive
** (C) Mika Molenkamp, 2016-2017.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Represents a mouse-over tooltip.
    /// </summary>
    internal class Tooltip {

        private static readonly List<Tooltip> renderQueue = new List<Tooltip>();

        private Rectangle trigger;
        private string text;

        private bool visible;
        private int posx;
        private int posy;
        private readonly int boxwidth;
        private readonly int boxheight;

        public Tooltip(Rectangle trigger, string text) {
            this.trigger = trigger;
            this.text = Assets.fontTooltip.WrapText(text, 350f);
            visible = false;

            Vector2 boxsize = Assets.fontTooltip.MeasureString(text) + new Vector2(20, 20);
            boxwidth = (int)boxsize.X;
            boxheight = (int)boxsize.Y;
        }

        public void Draw() {
            if (!visible) return;
            renderQueue.Add(this);
        }

        public void SetTrigger(Rectangle newarea) {
            this.trigger = newarea;
        }

        public void Update(float deltaTime) {
            // is the user hovering over the trigger area?
            if (Input.GetMouseInArea(trigger)) {
                // apply mouse coordinates
                posx = Input.MouseX + 12;
                posy = Input.MouseY + 12;
                // if mouse overlaps tooltip itself, hide it
                if (Input.GetMouseInArea(posx, posy, boxwidth, boxheight)) {
                    visible = false;
                    return;
                }
                // if already visible, no need to do anything else
                visible = true;
            } else {
                visible = false;
            }
        }

        public static void DrawQueued(SpriteBatch spriteBatch) {
            foreach (Tooltip tt in renderQueue) {
                spriteBatch.Draw9Slice(Assets.GetTexture("ui/tooltip"), new Rectangle(tt.posx, tt.posy, tt.boxwidth, tt.boxheight), Color.White, 16);
                spriteBatch.DrawString(Assets.fontTooltip, tt.text, new Vector2(tt.posx + 10, tt.posy + 11), Color.Black);
            }
            renderQueue.Clear();
        }

    }

}
