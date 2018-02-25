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
    /// A dialog box that shows some info about the editor.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogEditorIntro : DialogBase {

        private static readonly string message =
            "Welcome to the nonogram editor. Here are some things to keep in mind:" +
            Environment.NewLine + Environment.NewLine +
            "* Custom puzzles will be saved to 'My Documents/My Games/Kupogram/Puzzles'. You can share these files with other people." +
            Environment.NewLine + Environment.NewLine +
            "* Make sure your puzzle only has one solution. Even if the hint numbers match up, the game only considers the one solution you drew to be correct.";

        public DialogEditorIntro()
            : base(520, 400) {
            Title = "Hello!";

            var wrapped = Assets.fontDefault.WrapText(message, 480f);
            AddChild(new TextLabel(20, 50, wrapped));

            var btnCancel = new TextButton(Width - 170, Height - 55, 150, 35, "OK!");
            btnCancel.OnClick += sender => Application.Inst.PopUI();
            btnCancel.CancelSound = true;
            AddChild(btnCancel);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer) return;

            // close menu with esc
            if (Input.GetKeyDown(Keys.Escape))
                Close();
        }

    }

}
