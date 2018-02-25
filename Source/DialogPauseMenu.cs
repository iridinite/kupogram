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
    /// In-game menu / pause menu dialog box.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogPauseMenu : DialogBase {

        //private readonly FormGame master;

        public DialogPauseMenu(FormGame master)
            : base(270, 260) {
            //this.master = master;
            Title = "Game Menu";

            var btnResume = new TextButton(20, 50, 230, 40, "Resume");
            btnResume.OnClick += sender => Application.Inst.PopUI();
            btnResume.CancelSound = true;

            var btnOptions = new TextButton(20, 100, 230, 40, "Options");
            btnOptions.OnClick += sender => Application.Inst.PushUI(new DialogOptions());

            var btnRestart = new TextButton(20, 150, 230, 40, "Restart");
            btnRestart.OnClick += sender => Application.Inst.PushUI(new DialogConfirm(
                "Are you sure you want to clear the board? This action cannot be undone.",
                "Clear Board?",
                "Restart", "Never mind",
                alt => {
                    master.Restart();
                    Application.Inst.PopUI(); // pop the pause menu too
                }, null));
            btnRestart.Enabled = !master.Editor;

            var btnExit = new TextButton(20, 200, 230, 40, "Exit");
            if (master.Editor && !master.Dirty) {
                // if editor mode, and there are no unsaved changes, don't bother with a confirm dialog
                btnExit.OnClick += sender => Application.Inst.SetUI(new FormMainMenu());
            } else {
                // confirm progress loss or discarding of unsaved changes
                btnExit.OnClick += sender => Application.Inst.PushUI(new DialogConfirm(
                    master.Editor
                        ? "There are unsaved changes, if you exit they will be discarded. If you want to save them, go back and click the Save button."
                        : "Are you sure you want to return to the main menu? You'll lose any progress on this board so far.",
                    master.Editor
                        ? "Unsaved Changes"
                        : "Really quit?",
                    "Exit to Menu", "Never mind",
                    alt => Application.Inst.SetUI(new FormMainMenu()), null));
            }

            AddChild(btnResume);
            AddChild(btnOptions);
            AddChild(btnRestart);
            AddChild(btnExit);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer) return;

            // close menu with esc
            if (Input.GetKeyDown(Keys.Escape))
                Close();
        }

    }

}
