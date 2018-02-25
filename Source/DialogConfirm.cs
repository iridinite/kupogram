/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

namespace Kupogram {

    /// <summary>
    /// A dialog box that shows the user a yes/no question and invokes either of two delegates.
    /// </summary>
    /// <inheritdoc />
    internal sealed class DialogConfirm : DialogBase {

        public DialogConfirm(string message, string title, string yesText, string noText, OnClickHandler onYes, OnClickHandler onNo)
            : base(450, 220) {
            Title = title;

            message = Assets.fontDefault.WrapText(message, 410f);
            AddChild(new TextLabel(20, 50, message));
            //Height = (int)Assets.fontDefault.MeasureString(message).Y + 100;

            var btnYes = new TextButton(20, 160, 200, 40, yesText);
            btnYes.OnClick += sender => {
                Application.Inst.PopUI();
                onYes?.Invoke(btnYes);
            };
            AddChild(btnYes);

            var btnNo = new TextButton(230, 160, 200, 40, noText);
            btnNo.OnClick += sender => {
                Application.Inst.PopUI();
                onNo?.Invoke(btnNo);
            };
            btnNo.CancelSound = true;
            AddChild(btnNo);
        }

    }

}
