/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.IO;

namespace Kupogram {

    /// <summary>
    /// Dialog box that allows importing image files into nonogram boards.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogImportImage : DialogBase {

        public DialogImportImage(FormGame master)
            : base(520, 400) {
            Title = "Game Menu";

            AddChild(new TextLabel(20, 50, Assets.fontDefault.WrapText("This will import 'My Documents/My Games/Kupogram/board.png' as a nonogram board. Each pixel corresponds to one cell, and dark pixels (under 50% gray) will be interpreted as filled cells. Maybe I'll make this a bit more user-friendly some day?" + Environment.NewLine + Environment.NewLine + "WARNING: This will clear any image you've currently drawn, and may change the board size.", 480f)));

            var btnCancel = new TextButton(Width - 170, Height - 58, 150, 38, "Cancel");
            btnCancel.OnClick += sender => Application.Inst.PopUI();
            btnCancel.CancelSound = true;
            AddChild(btnCancel);

            var btnImport = new TextButton(Width - 330, Height - 58, 150, 38, "Import");
            btnImport.OnClick += sender => {
                try {
                    var newboard = Board.FromImage(Util.SaveDir.FullName + Path.DirectorySeparatorChar + "board.png");
                    //newboard.ID = master.GetBoard().ID;
                    newboard.Title = master.Renderer.Board.Title;
                    newboard.Author = master.Renderer.Board.Author;
                    newboard.Categories.AddRange(master.Renderer.Board.Categories);
                    //master.SetBoard(newboard);
                    master.Renderer.Board = newboard;
                    master.MakeDirty();
                } catch (Exception) {
                    // ignored
                }
                Application.Inst.PopUI();
            };
            btnImport.CancelSound = true;
            AddChild(btnImport);
        }

    }

}
