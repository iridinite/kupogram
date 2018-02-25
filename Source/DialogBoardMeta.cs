/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using Microsoft.Xna.Framework;

namespace Kupogram {

    /// <summary>
    /// A dialog box that allows reconfiguration of a <seealso cref="Board"/>.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogBoardMeta : DialogBase {

        private readonly TextField txtTitle;
        private readonly NumericUpDown nudWidth, nudHeight;
        private readonly TextLabel lblCategories;

        private readonly Board board;

        public DialogBoardMeta(Board board, bool noCancel)
            : base(450, 370) {
            this.board = board;
            Title = "Board Settings";

            AddChild(new TextLabel(20, 50, "Title:"));
            txtTitle = new TextField(20, 80, 410) {Text = board.Title};
            AddChild(txtTitle);

            AddChild(new TextLabel(20, 130, "Width:"));
            nudWidth = new NumericUpDown(20, 160, 180) {
                Minimum = 5,
                Maximum = 100,
                Value = board.Width
            };
            AddChild(nudWidth);

            AddChild(new TextLabel(240, 130, "Height:"));
            nudHeight = new NumericUpDown(240, 160, 180) {
                Minimum = 5,
                Maximum = 100,
                Value = board.Height
            };
            AddChild(nudHeight);

            AddChild(new TextLabel(20, 220, "Categories:"));
            lblCategories = new TextLabel(20, 250, String.Empty);
            AddChild(lblCategories);
            var btnCats = new TextButton(150, 212, 100, 32, "Edit...");
            btnCats.OnClick += sender => Application.Inst.PushUI(new DialogBoardCats(board));
            AddChild(btnCats);

            var btnSave = new TextButton(120, Height - 55, 150, 35, "Accept");
            btnSave.OnClick += btnSave_OnClick;
            btnSave.CancelSound = true;
            AddChild(btnSave);

            if (noCancel) {
                btnSave.Caption = "OK";
                btnSave.X = 280; // btnCancel's normal location
                return;
            }

            var btnCancel = new TextButton(280, Height - 55, 150, 35, "Cancel");
            btnCancel.OnClick += sender => Application.Inst.PopUI();
            btnCancel.CancelSound = true;
            btnCancel.SetTooltip("Changes will be discarded");
            AddChild(btnCancel);
        }

        private void btnSave_OnClick(Control sender) {
            board.Title = txtTitle.Text;

            if (nudWidth.Value != board.Width || nudHeight.Value != board.Height) {
                board.Resize(nudWidth.Value, nudHeight.Value);
            }

            Application.Inst.PopUI();
        }

        protected override void OnUpdate(GameTime gameTime) {
            lblCategories.Text = Assets.fontDefault.WrapText(board.GetCategoryNames(), Width - 40);
        }

    }

}
