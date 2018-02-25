/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

namespace Kupogram {

    /// <summary>
    /// A dialog box that allows viewing and setting of a <seealso cref="Board"/>'s <seealso cref="Category"/> entries.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogBoardCats : DialogBase {

        public DialogBoardCats(Board board)
            : base(300, 120) {
            Title = "Categories";

            int y = 50;
            Category.All.ForEach(cat => {
                var box = new CheckBox(20, y, cat.Name);
                box.Checked = board.Categories.Contains(cat.ID);
                box.CheckedChanged += check => {
                    if (check)
                        board.Categories.Add(cat.ID);
                    else
                        board.Categories.Remove(cat.ID);
                };
                AddChild(box);
                y += 32;
            });

            // make the window taller to fit all the checkboxes
            Height += Category.All.Count * 32;
            Y -= Category.All.Count * 16;

            // exit button
            var btnOK = new TextButton(Width - 180, Height - 55, 160, 35, "OK");
            btnOK.OnClick += sender => Application.Inst.PopUI();
            btnOK.CancelSound = true;
            AddChild(btnOK);
        }

    }

}
