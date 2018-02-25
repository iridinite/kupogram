/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

namespace Kupogram {

    /// <summary>
    /// Specifies the frame of reference for a control's location.
    /// </summary>
    public enum AnchorMode {
        Absolute,
        RelativeParent,
        RelativeCenter
    }

    /// <summary>
    /// Represents a control that automatically moves itself to a certain location on the screen.
    /// </summary>
    /// <inheritdoc />
    internal class Anchor : Control {

        private AnchorMode Mode { get; }

        public override int EffX {
            get {
                switch (Mode) {
                    case AnchorMode.Absolute:
                        return X;
                    case AnchorMode.RelativeParent:
                        return base.EffX;
                    case AnchorMode.RelativeCenter:
                        return Application.Inst.GameWidth / 2 + X;
                    default:
                        return 0;
                }
            }
        }

        public override int EffY {
            get {
                switch (Mode) {
                    case AnchorMode.Absolute:
                        return Y;
                    case AnchorMode.RelativeParent:
                        return base.EffY;
                    case AnchorMode.RelativeCenter:
                        return Application.Inst.GameHeight / 2 + Y;
                    default:
                        return 0;
                }
            }
        }

        public Anchor(int x, int y, AnchorMode mode) {
            X = x;
            Y = y;
            Mode = mode;
        }

    }

}
