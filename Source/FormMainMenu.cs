/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    internal sealed class FormMainMenu : Control {

        private readonly Anchor titleAnchor;
        private float bgOffset;

        public FormMainMenu() {
            DrawMode = ControlDrawMode.ChildrenLast;
            Audio.PlayMusic("event:/Music/Home");

            titleAnchor = new Anchor(0, -150, AnchorMode.RelativeCenter);
            var menuAnchor = new Anchor(-135, 200, AnchorMode.RelativeParent);

            var btnPlay = new TextButton(0, 0, 270, 40, "Select Puzzle");
            btnPlay.OnClick += sender => Application.Inst.PushUI(new DialogPickBoard(false));
            var btnEdit = new TextButton(0, 50, 270, 40, "Editor");
            btnEdit.OnClick += sender => Application.Inst.PushUI(new DialogPickBoard(true));
            var btnOptions = new TextButton(0, 100, 130, 40, "Options");
            btnOptions.OnClick += sender => Application.Inst.PushUI(new DialogOptions());
            var btnCredits = new TextButton(140, 100, 130, 40, "Credits");
            btnCredits.OnClick += sender => Application.Inst.PushUI(new DialogCredits());
            var btnExit = new TextButton(0, 150, 270, 40, "Exit");
            btnExit.OnClick += sender => Application.Inst.PushUI(new DialogConfirm(
                "There are still puzzles that need solving, you know...", ":(",
                "I said quit!", "Never mind",
                alt => Application.Inst.Exit(), null));

            menuAnchor.AddChild(btnPlay);
            menuAnchor.AddChild(btnEdit);
            menuAnchor.AddChild(btnOptions);
            menuAnchor.AddChild(btnCredits);
            menuAnchor.AddChild(btnExit);
            titleAnchor.AddChild(menuAnchor);
            AddChild(titleAnchor);
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            // scrolling background
            const int segment = 160;
            var x = 0;
            var odd = false;
            var bgtex = Assets.GetTexture("board/back_lpaper");
            while (x < Application.Inst.GameWidth) {
                spriteBatch.Draw(bgtex,
                    new Rectangle(x, 0, segment, Application.Inst.GameHeight),
                    new Rectangle(x, odd ? (int)-bgOffset : (int)bgOffset, segment, Application.Inst.GameHeight),
                    /*odd ? Color.WhiteSmoke : Color.LightGray*/ Color.White);

                x += segment;
                odd = !odd;
            }

            // dark boxes
            spriteBatch.Draw(Assets.GetTexture("ui/rect"),
                new Rectangle(0, titleAnchor.EffY - 60, Application.Inst.GameWidth, 120),
                Color.Black * 0.65f);
            spriteBatch.Draw(Assets.GetTexture("ui/rect"),
                new Rectangle(Application.Inst.GameWidth / 2 - 155, titleAnchor.EffY + 180, 310, 230),
                Color.Black * 0.45f);

            // title
            spriteBatch.Draw(Assets.GetTexture("ui/title"),
                new Vector2(titleAnchor.EffX, titleAnchor.EffY),
                null,
                Color.White,
                0f,
                new Vector2(357f, 60f),
                0.75f, //(float)Math.Sin(titleBounce / 1.7f) * 0.05f + 1f,
                SpriteEffects.None,
                0f);

            // some text
            var versionstr = "b" + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            spriteBatch.DrawStringOutline(Assets.fontDefault, "(C) Mika Molenkamp", new Vector2(20, Application.Inst.GameHeight - 35), 2, Color.White, Color.Black);
            spriteBatch.DrawStringOutline(Assets.fontDefault, versionstr, new Vector2(Application.Inst.GameWidth - 20 - Assets.fontDefault.MeasureString(versionstr).X, Application.Inst.GameHeight - 35), 2, Color.White, Color.Black);
        }

        protected override void OnUpdate(GameTime gameTime) {
            //titleBounce += (float)gameTime.ElapsedGameTime.TotalSeconds;
            bgOffset += (float)gameTime.ElapsedGameTime.TotalSeconds * 16f;
        }

    }

}
