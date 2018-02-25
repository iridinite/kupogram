/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    /// <summary>
    /// Represents a control that displays and allows editing of an integer.
    /// </summary>
    /// <inheritdoc />
    internal sealed class NumericUpDown : Control {

        /// <summary>
        /// Gets or sets the current value of the control.
        /// </summary>
        public int Value {
            get => _value;
            set => _value = MathHelper.Clamp(value, Minimum, Maximum);
        }

        /// <summary>
        /// Gets or sets the inclusive minimum value.
        /// </summary>
        public int Minimum { get; set; }

        /// <summary>
        /// Gets or sets the inclusive maximum value.
        /// </summary>
        public int Maximum { get; set; }

        private int _value;

        private readonly Button btnUp, btnDown;
        private readonly TextLabel lblValue;

        public NumericUpDown(int x, int y, int w) {
            X = x;
            Y = y;
            Width = w;
            Height = 32;

            btnUp = new TextButton(Width - 32, 0, 32, 32, "+");
            btnUp.SetTooltip("Hold Shift for large increments");
            btnUp.OnClick += sender => Value += Input.GetKey(Keys.LeftShift) ? 10 : 1;
            btnDown = new TextButton(0, 0, 32, 32, "-");
            btnDown.SetTooltip("Hold Shift for large decrements");
            btnDown.OnClick += sender => Value -= Input.GetKey(Keys.LeftShift) ? 10 : 1;
            AddChild(btnUp);
            AddChild(btnDown);

            lblValue = new TextLabel(0, 6, String.Empty, Assets.fontClues[3]);
            AddChild(lblValue);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer || !Visible) return;

            lblValue.Text = Value.ToString(CultureInfo.InvariantCulture);
            lblValue.X = Width / 2 - (int)(lblValue.Font.MeasureString(lblValue.Text).X / 2f);

            btnUp.Enabled = Value < Maximum;
            btnDown.Enabled = Value > Minimum;
        }

    }

}
