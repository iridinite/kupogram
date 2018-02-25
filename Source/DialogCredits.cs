/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    /// <summary>
    /// A dialog box that shows development credits.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogCredits : DialogBase {

        private static readonly string creditsmsg = "Written by Mika Molenkamp."
            + Environment.NewLine + Environment.NewLine
            + "Uses MonoGame Framework 3.6" + Environment.NewLine
            + "(C) MonoGame Team & contributors"
            + Environment.NewLine + Environment.NewLine
            + "Uses FMOD Studio / FMOD Sound System" + Environment.NewLine
            + "(C) Firelight Technologies"
            + Environment.NewLine + Environment.NewLine
            + "\"Brittle Rille\", \"Eternity\", \"Silver Flame\""
            + Environment.NewLine + "(C) Kevin MacLeod (incompetech.com)"
            + Environment.NewLine + "Licensed under Creative Commons BY 3.0 (creativecommons.org/licenses/by/3.0)";

        public DialogCredits()
            : base(530, 450) {
            Title = "Credits";

            AddChild(new TextLabel(20, 50, Assets.fontDefault.WrapText(creditsmsg, 490f)));

            // exit button
            var btnOK = new TextButton(Width - 180, Height - 55, 160, 35, "OK");
            btnOK.OnClick += sender => Application.Inst.PopUI();
            btnOK.CancelSound = true;
            AddChild(btnOK);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer) return;

            // close menu with esc
            if (Input.GetKeyDown(Keys.Escape))
                Close();
        }

    }

}
