/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    internal sealed class FormGame : Control {

        private bool doVictoryDance;
        private float boardOffset;
        private float boardResultPos = 80f;
        private float boardResultOpacity;
        private float victoryWait = 1f;
        private float victoryMessageAmp = -MathHelper.PiOver2;
        private float victoryMessageSize = 1f;
        private float victoryMessagePos;

        private Anchor commandButtonAnchor;
        private ImageButton btnSave, btnUndo, btnRedo;
        private Button btnLeaveAfterClear;
        private TimeSpan timer;

        /// <summary>
        /// Returns the <seealso cref="BoardRenderer"/> associated with this game session.
        /// </summary>
        public BoardRenderer Renderer { get; }

        /// <summary>
        /// Indicates whether the game is in editor mode.
        /// </summary>
        public bool Editor { get; }

        /// <summary>
        /// Indicates whether the board has been changed in edit mode.
        /// </summary>
        public bool Dirty { get; private set; }

        public FormGame(Board selectedBoard, bool editor) {
            this.Editor = editor;
            Renderer = new BoardRenderer(this) {
                Board = selectedBoard,
                ShowClues = !editor
            };
            AddChild(new Background(Renderer));
            AddChild(Renderer);

            Audio.PlayMusic(editor ? "event:/Music/Edit" : "event:/Music/Game");

            DrawMode = ControlDrawMode.ChildrenFirst;
            CommandButtonSetup();
        }

        /// <summary>
        /// Clears the board state and starts over.
        /// </summary>
        public void Restart() {
            // this resets state and hint crossout
            Renderer.Reset();
            // clear undo
            Renderer.History.Clear();
            UpdateUndoRedoButtons();
            // reset time
            timer = TimeSpan.Zero;
        }

        private void CommandButtonSetup() {
            btnLeaveAfterClear = new TextButton(0, 0, 180, 35, "Sweet!");
            btnLeaveAfterClear.OnClick += sender => {
                Application.Inst.SetUI(new FormMainMenu());
                Application.Inst.PushUI(new DialogPickBoard(false));
            };
            btnLeaveAfterClear.Visible = false;
            AddChild(btnLeaveAfterClear);

            commandButtonAnchor = new Anchor(20, 20, AnchorMode.Absolute);
            AddChild(commandButtonAnchor);

            var iconTexture = Assets.GetTexture("ui/modeicons");
            var btnConfig = new ImageButton(0, 0, 40, 40, iconTexture);
            btnConfig.SetSourceRect(new Rectangle(64, 0, 32, 32));
            btnConfig.SetTooltip("Game Menu");
            btnConfig.OnClick += sender => Application.Inst.PushUI(new DialogPauseMenu(this));
            commandButtonAnchor.AddChild(btnConfig);

            if (Editor) {
                btnSave = new ImageButton(50, 0, 40, 40, iconTexture);
                btnSave.SetSourceRect(new Rectangle(128, 0, 32, 32));
                btnSave.SetTooltip("Save");
                btnSave.OnClick += sender => {
                    Renderer.Board.Save();
                    Util.RefreshPuzzleCache();
                    Dirty = false;
                    ((Button)sender).Enabled = false;
                };
                btnSave.Enabled = false;
                commandButtonAnchor.AddChild(btnSave);

                var btnImportImage = new ImageButton(100, 0, 40, 40, iconTexture);
                btnImportImage.SetSourceRect(new Rectangle(256, 0, 32, 32));
                btnImportImage.SetTooltip("Import from Image");
                btnImportImage.OnClick += sender =>
                    Application.Inst.PushUI(new DialogImportImage(this));
                commandButtonAnchor.AddChild(btnImportImage);

                var btnClueToggle = new ImageButton(150, 0, 40, 40, iconTexture);
                btnClueToggle.SetSourceRect(new Rectangle(96, 0, 32, 32));
                btnClueToggle.SetTooltip("Toggle Hint Visibility");
                btnClueToggle.OnClick += sender => Renderer.ShowClues = !Renderer.ShowClues;
                commandButtonAnchor.AddChild(btnClueToggle);

                var btnMetadata = new ImageButton(200, 0, 40, 40, iconTexture);
                btnMetadata.SetSourceRect(new Rectangle(160, 0, 32, 32));
                btnMetadata.SetTooltip("Edit Puzzle Info");
                btnMetadata.OnClick += sender => {
                    Application.Inst.PushUI(new DialogBoardMeta(Renderer.Board, false));
                    MakeDirty();
                };
                commandButtonAnchor.AddChild(btnMetadata);

#if DEBUG
                // allows saving a puzzle with no author, so it can be included with default puzzle pack
                var btnSaveFactory = new ImageButton(250, 0, 40, 40, iconTexture);
                btnSaveFactory.SetSourceRect(new Rectangle(128, 0, 32, 32));
                btnSaveFactory.SetTooltip("Save As Factory (Reset Author)");
                btnSaveFactory.OnClick += sender => {
                    Renderer.Board.Author = Guid.Empty;
                    Renderer.Board.Save();
                };
                commandButtonAnchor.AddChild(btnSaveFactory);
#endif
            } else {
                btnUndo = new ImageButton(50, 0, 40, 40, iconTexture);
                btnUndo.SetSourceRect(new Rectangle(192, 0, 32, 32));
                btnUndo.SetTooltip("Undo");
                btnUndo.OnClick += sender => {
                    Renderer.History.Undo(Renderer.Board);
                    Renderer.RecomputeState();
                    UpdateUndoRedoButtons();
                };
                btnUndo.Enabled = false;
                commandButtonAnchor.AddChild(btnUndo);

                btnRedo = new ImageButton(100, 0, 40, 40, iconTexture);
                btnRedo.SetSourceRect(new Rectangle(224, 0, 32, 32));
                btnRedo.SetTooltip("Redo");
                btnRedo.OnClick += sender => {
                    Renderer.History.Redo(Renderer.Board);
                    Renderer.RecomputeState();
                    UpdateUndoRedoButtons();
                };
                btnRedo.Enabled = false;
                commandButtonAnchor.AddChild(btnRedo);
            }
        }

        /// <summary>
        /// Updates the enabled states of the undo/redo buttons.
        /// </summary>
        public void UpdateUndoRedoButtons() {
            btnUndo.Enabled = Renderer.History.IsUndoAvailable;
            btnRedo.Enabled = Renderer.History.IsRedoAvailable;
        }

        /// <summary>
        /// Marks the board as having been changed, enabling the save button.
        /// </summary>
        public void MakeDirty() {
            Dirty = true;
            btnSave.Enabled = true;
        }

        /// <summary>
        /// Starts the completion animation.
        /// </summary>
        public void BeginVictoryDance() {
            doVictoryDance = true;
            boardOffset = Renderer.Position.y;
        }

        /// <summary>
        /// Returns whether the completion animation has started.
        /// </summary>
        public bool IsFinished() {
            return doVictoryDance;
        }

        private void DrawVictoryDance(SpriteBatch spriteBatch) {
            if (!IsFinished()) return;

            // 'clear' sprite
            spriteBatch.Draw(Assets.GetTexture("ui/roundmsgs"),
                new Vector2(Application.Inst.GameWidth * 0.5f, Application.Inst.GameHeight * 0.5f - victoryMessagePos - 70),
                new Rectangle(0, 0, 256, 128), Color.White,
                0f,
                new Vector2(128, 64),
                1f + (float)Math.Sin(victoryMessageAmp) * victoryMessageSize,
                SpriteEffects.None,
                0f);

            if (UserConfig.ShowTimer) {
                // final time taken by player to finish
                var timerStr = $"in {timer.TotalMinutes:00}:{timer.Seconds:00}";
                var timerPos = new Vector2(
                    Application.Inst.GameWidth * 0.5f - Assets.fontClues[4].MeasureString(timerStr).X * 0.5f,
                    Application.Inst.GameHeight * 0.5f + boardResultPos - 260);
                spriteBatch.DrawStringOutline(Assets.fontClues[4], timerStr, timerPos,
                    1, Color.White * boardResultOpacity, Color.Black * boardResultOpacity);
            }

            // board picture
            if (Renderer.Board.Preview == null)
                Renderer.Board.GeneratePreviewImage(spriteBatch.GraphicsDevice, spriteBatch);
            spriteBatch.Draw(Renderer.Board.Preview,
                new Vector2(Application.Inst.GameWidth * 0.5f, Application.Inst.GameHeight * 0.5f + boardResultPos - 90),
                null, Color.White * boardResultOpacity, 0f,
                new Vector2(Renderer.Board.Preview.Width * 0.5f, Renderer.Board.Preview.Height * 0.5f),
                1f, SpriteEffects.None, 0f);
            // board title
            var textPos = new Vector2(
                (int)(Application.Inst.GameWidth * 0.5f - Assets.fontBold.MeasureString(Renderer.Board.Title).X * 0.5f),
                (int)(Application.Inst.GameHeight * 0.5f + boardResultPos + 50));
            spriteBatch.DrawStringOutline(Assets.fontBold, Renderer.Board.Title, textPos, 2, Color.White * boardResultOpacity, Color.Black * boardResultOpacity);
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            // game timer
            if (!Editor && !IsFinished()) {
                int y = 80;
                if (UserConfig.ShowTimer) {
                    // show game timer
                    spriteBatch.DrawStringOutline(Assets.fontClues[4], $"{timer.TotalMinutes:00}:{timer.Seconds:00}", new Vector2(24, y), 2, Color.White, Color.Black);
                    y += 45;
                }
                // show user controls
                spriteBatch.Draw(Assets.GetTexture("ui/modeicons"), new Vector2(24, y), new Rectangle(288, 0, 32, 32), Color.White);
                spriteBatch.Draw(Assets.GetTexture("ui/modeicons"), new Vector2(24, y + 32), new Rectangle(320, 0, 32, 32), Color.White);
                spriteBatch.Draw(Assets.GetTexture("ui/modeicons"), new Vector2(24, y + 64), new Rectangle(352, 0, 32, 32), Color.White);
                spriteBatch.DrawString(Assets.fontDefault, "Fill / Clear", new Vector2(64, y + 8), Color.Black);
                spriteBatch.DrawString(Assets.fontDefault, "Cross out", new Vector2(64, y + 40), Color.Black);
                spriteBatch.DrawString(Assets.fontDefault, "Pan / Zoom", new Vector2(64, y + 72), Color.Black);
            } else if (Editor) {
                // board title
                spriteBatch.DrawStringOutline(Assets.fontDefault, $"Editing '{Renderer.Board.Title}'", new Vector2(24, 80), 2, Color.White, Color.Black);
            }

            // victory message and renderer.Board result image
            DrawVictoryDance(spriteBatch);
        }

        protected override void OnUpdate(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // popup with info for first-time users
            if (Editor && UserConfig.ShowEditorIntro) {
                UserConfig.ShowEditorIntro = false;
                Application.Inst.PushUI(new DialogEditorIntro());
            }

            // increase game timer (stop counting when puzzle is finished)
            if (!IsFinished())
                timer += TimeSpan.FromSeconds(dt);

            // victory dance animation
            if (IsFinished()) {
                commandButtonAnchor.Visible = false;

                if (victoryMessageSize > 0f) {
                    victoryMessageSize -= dt * 1.5f;
                    victoryMessageAmp += dt * 12f;
                } else if (victoryWait > 0f) {
                    victoryWait -= dt;
                } else if (Renderer.Opacity > 0f) {
                    Renderer.Opacity = Math.Max(0f, Renderer.Opacity - dt * 0.5f);
                    boardOffset += dt * 72f;
                    Renderer.Position.y = (int)boardOffset;
                    victoryMessagePos += dt * 80f;
                    if (Renderer.Opacity <= 0f)
                        victoryWait = 1f;
                } else if (boardResultOpacity < 1f) {
                    boardResultPos += dt * 50f;
                    boardResultOpacity += dt * 2f;
                    if (boardResultOpacity >= 1f)
                        victoryWait = 1f;
                } else {
                    btnLeaveAfterClear.X = Application.Inst.GameWidth / 2 - 90;
                    btnLeaveAfterClear.Y = Application.Inst.GameHeight / 2 + 270;
                    btnLeaveAfterClear.Visible = true;
                }

                return;
            }

            // ignore player input if a menu is open
            if (!IsActiveLayer) return;

            // shortcut, open menu with esc
            if (Input.GetKeyDown(Keys.Escape))
                Application.Inst.PushUI(new DialogPauseMenu(this));
        }

    }

}
