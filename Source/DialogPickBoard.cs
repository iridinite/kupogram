/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    internal sealed class DialogPickBoard : DialogBase {

        private const int DLG_WIDTH = 450;
        private const int DLG_HEIGHT = 650;

        private readonly bool EditMode;

        private RenderTarget2D boardListImage;
        private List<Board> catalog;
        private int hoverIndex;

        private float scrollTarget, scrollCurrent;

        public DialogPickBoard(bool editor)
            : base(DLG_WIDTH, DLG_HEIGHT) {
            Title = editor ? "Edit Puzzle" : "Select a Puzzle";
            EditMode = editor;
            hoverIndex = -1;
            catalog = editor ? Util.EditableCatalog : Util.PuzzleCatalog;

            // cancel button
            var btnBack = new TextButton(DLG_WIDTH - 160, DLG_HEIGHT - 45, 150, 35, "Back");
            btnBack.OnClick += sender => Application.Inst.PopUI();
            btnBack.CancelSound = true;
            AddChild(btnBack);

            // create-new button for editor mode
            if (editor) {
                var btnNew = new TextButton(DLG_WIDTH - 350, DLG_HEIGHT - 45, 180, 35, "Create New");
                btnNew.OnClick += sender => {
                    var emptyBoard = new Board(10, 10);
                    var frmGame = new FormGame(emptyBoard, true);
                    frmGame.MakeDirty();
                    Application.Inst.SetUI(frmGame);
                    Application.Inst.PushUI(new DialogBoardMeta(emptyBoard, true));
                };
                AddChild(btnNew);
            }

            // list refresh button
            var btnRefresh = new ImageButton(10, DLG_HEIGHT - 45, 35, 35, Assets.GetTexture("ui/modeicons"));
            btnRefresh.SetSourceRect(new Rectangle(384, 0, 32, 32));
            btnRefresh.SetTooltip("Refresh List");
            btnRefresh.OnClick += sender => {
                boardListImage?.Dispose();
                boardListImage = null;
                Util.RefreshPuzzleCache();
            };
            AddChild(btnRefresh);
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            base.OnDraw(spriteBatch);

            if (boardListImage == null) {
                spriteBatch.DrawString(Assets.fontDefault, "No puzzles found. Add some maybe?", new Vector2(EffX + 20, EffY + 50), Color.White);
            } else {
                spriteBatch.Draw(boardListImage, new Vector2(EffX + 10, EffY + 50), Color.White);
            }

            if (hoverIndex != -1 && catalog[hoverIndex].IsPreviouslySolved) {
                spriteBatch.DrawStringOutline(Assets.fontBold, catalog[hoverIndex].DisplayTitle, new Vector2(EffX + Width + 32, EffY + 32), 2, Color.White, Color.Black);
                spriteBatch.Draw(catalog[hoverIndex].Preview, new Vector2(EffX + Width + 32, EffY + 72), Color.White);
            }
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (!IsActiveLayer) return;

            // close menu with esc
            if (Input.GetKeyDown(Keys.Escape)) {
                Close();
                return;
            }

            hoverIndex = -1;
            if (boardListImage != null && Input.GetMouseInArea(EffX + 10, EffY + 50, boardListImage.Width, boardListImage.Height)) {
                // adjust scroll target
                scrollTarget += Input.MouseScroll;
                scrollTarget = MathHelper.Clamp(scrollTarget, Math.Min(0f, -(catalog.Count * 62 - boardListImage.Height)), 0f);
                // interpolate scrolling
                float scrollDiff = scrollTarget - scrollCurrent;
                scrollCurrent += scrollDiff * 16f * (float)gameTime.ElapsedGameTime.TotalSeconds;

                for (int i = 0; i < catalog.Count; i++) {
                    //var board = catalog[i];
                    var y = (int)scrollCurrent + EffY + 50 + i * 62;
                    if (Input.GetMouseInArea(new Rectangle(EffX, y, DLG_WIDTH - 20, 62))) {
                        hoverIndex = i;
                    }
                }
            }

            if (Input.GetMouseLeftDown() && hoverIndex != -1) {
                Audio.PlaySound("event:/UI/Confirm");
                Application.Inst.SetUI(new FormGame(catalog[hoverIndex], EditMode));
            }
        }

        protected override void OnRender(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
            // generate preview image for all catalog items
            foreach (var board in catalog)
                board.GeneratePreviewImage(graphicsDevice, spriteBatch);

            // don't render a list if we have nothing in it
            if (catalog.Count == 0) return;

            // refresh render target
            if (boardListImage == null || boardListImage.IsDisposed || boardListImage.IsContentLost
                /*boardListImage.Height != 58 * catalog.Count*/) {
                if (boardListImage != null && !boardListImage.IsDisposed)
                    boardListImage.Dispose();
                boardListImage = new RenderTarget2D(graphicsDevice, DLG_WIDTH - 20, DLG_HEIGHT - 110, false, SurfaceFormat.Color, DepthFormat.None);
            }

            graphicsDevice.SetRenderTarget(boardListImage);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp);

            // draw each item in the list
            for (int i = 0; i < catalog.Count; i++) {
                var board = catalog[i];
                var y = (int)scrollCurrent + i * 62;

                // hover bg
                if (hoverIndex == i) {
                    spriteBatch.Draw(Assets.GetTexture("ui/rect"), new Rectangle(0, y - 4, DLG_WIDTH - 20, 58), Color.White * 0.15f);
                }

                if (board.IsPreviouslySolved) {
                    // board image
                    spriteBatch.Draw(board.Preview,
                        new Vector2(35, y + 25),
                        null,
                        Color.White,
                        0f,
                        new Vector2(board.Preview.Width / 2f, board.Preview.Height / 2f),
                        0.25f,
                        SpriteEffects.None,
                        0f);
                } else {
                    // big 'ol question mark
                    var tsize = Assets.fontClues[4].MeasureString("?");
                    spriteBatch.DrawStringShadow(Assets.fontClues[4],
                        "?",
                        new Vector2(35, y + 27),
                        2,
                        Color.Wheat,
                        Color.DimGray,
                        0f,
                        tsize * 0.5f,
                        Vector2.One,
                        SpriteEffects.None);
                }
                // title
                spriteBatch.DrawString(Assets.fontDefault,
                    $"{board.DisplayTitle}{Environment.NewLine}{board.Width} x {board.Height}",
                    new Vector2(72, y + 4),
                    Color.White);

#if DEBUG // debug info (uuid / author)
                var sidetext1 = board.ID.ToString().Substring(0, 8);
                var sidetext2 = board.Author.ToString().Substring(0, 8);
                spriteBatch.DrawString(Assets.fontTooltip, sidetext1, new Vector2(Width - 24 - Assets.fontTooltip.MeasureString(sidetext1).X, y + 5), Color.DarkGray);
                spriteBatch.DrawString(Assets.fontTooltip, sidetext2, new Vector2(Width - 24 - Assets.fontTooltip.MeasureString(sidetext2).X, y + 27), Color.DarkGray);
#endif
            }

            spriteBatch.End();
        }

        protected override void OnDestroy() {
            boardListImage?.Dispose();
        }

    }

}
