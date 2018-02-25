/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    /// <summary>
    /// Exposes functionality for obtaining mouse and keyboard state.
    /// </summary>
    internal static class Input {

        private static MouseState mousePrev;
        private static MouseState mouseCurr;
        private static KeyboardState keyPrev;
        private static KeyboardState keyCurr;
		
        public static int MouseX => mouseCurr.X;
        public static int MouseY => mouseCurr.Y;
        public static Vector2 MousePosition => new Vector2(MouseX, MouseY);
        public static int MouseScroll => mouseCurr.ScrollWheelValue - mousePrev.ScrollWheelValue;

        public static void Update() {
            mousePrev = mouseCurr;
            mouseCurr = Mouse.GetState();
            keyPrev = keyCurr;
            keyCurr = Keyboard.GetState();
        }

        public static bool GetMouseInArea(Rectangle rect) {
            return Application.Inst.IsActive &&
                MouseX >= rect.X &&
                MouseX <= rect.X + rect.Width &&
                MouseY >= rect.Y &&
                MouseY <= rect.Y + rect.Height;
        }

        public static bool GetMouseInArea(int x, int y, int w, int h) {
            return Application.Inst.IsActive &&
                GetMouseInArea(new Rectangle(x, y, w, h));
        }

        public static bool GetMouseMiddle() {
            return Application.Inst.IsActive &&
                mouseCurr.MiddleButton == ButtonState.Pressed;
        }

        public static bool GetMouseMiddleDown() {
            return Application.Inst.IsActive &&
                mouseCurr.MiddleButton == ButtonState.Pressed &&
                mousePrev.MiddleButton == ButtonState.Released;
        }

        public static bool GetMouseLeft() {
            return Application.Inst.IsActive && mouseCurr.LeftButton == ButtonState.Pressed;
        }

        public static bool GetMouseLeftDown() {
            return Application.Inst.IsActive &&
                mouseCurr.LeftButton == ButtonState.Pressed &&
                mousePrev.LeftButton == ButtonState.Released;
        }

        public static bool GetMouseLeftUp() {
            return Application.Inst.IsActive &&
                mousePrev.LeftButton == ButtonState.Pressed &&
                mouseCurr.LeftButton == ButtonState.Released;
        }

        public static bool GetMouseRight() {
            return Application.Inst.IsActive &&
                mouseCurr.RightButton == ButtonState.Pressed;
        }

        public static bool GetMouseRightDown() {
            return Application.Inst.IsActive &&
                mouseCurr.RightButton == ButtonState.Pressed &&
                mousePrev.RightButton == ButtonState.Released;
        }

        public static bool GetKey(Keys k) {
            return Application.Inst.IsActive && keyCurr.IsKeyDown(k);
        }

        public static bool GetKeyDown(Keys k) {
            return Application.Inst.IsActive && keyCurr.IsKeyDown(k) && keyPrev.IsKeyUp(k);
        }

        public static bool GetKeyUp(Keys k) {
            return Application.Inst.IsActive && keyCurr.IsKeyUp(k) && keyPrev.IsKeyDown(k);
        }

    }

}
