/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kupogram {

    /// <summary>
    /// Renders and updates a <seealso cref="T:Kupogram.Board" />.
    /// </summary>
    /// <inheritdoc />
    internal sealed class BoardRenderer : Control {

        private static readonly int[] tilesizeByIndex = {
            12, 16, 24, 32, 48
        };

        private Board board;
        private int tilesize, tilesizeindex;
        private List<int>[] statehor, statever;

        private Texture2D boardTexture;
        private SpriteFont clueFont;

        public (int x, int y) Position;
        private (int x, int y) hover, oldhover;
        private (int x, int y) linestart, lineend;
        private int lastMouseX, lastMouseY, lineCellTotal;
        private CellState targetState;
        private bool drawingLine, captureMouse;

        private static readonly Color clueColor1 = Color.Wheat; //Color.FromNonPremultiplied(221, 198, 177, 255);
        private static readonly Color clueColor2 = Color.LightSteelBlue;

        /// <summary>
        /// Gets or sets the game form that owns this renderer.
        /// </summary>
        private FormGame Master { get; }

        /// <summary>
        /// Gets a reference to the undo/redo stack.
        /// </summary>
        public UndoRedoStack History { get; } = new UndoRedoStack();

        /// <summary>
        /// Indicates whether or not the board hint numbers should be drawn.
        /// </summary>
        public bool ShowClues { get; set; }

        /// <summary>
        /// Gets or sets the board opacity, from 0.0 to 1.0.
        /// </summary>
        public float Opacity { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the <seealso cref="Board"/> for this renderer to work with.
        /// </summary>
        public Board Board {
            get => board;
            set {
                board = value;
                Reset();
            }
        }

        public BoardRenderer(FormGame master) {
            Master = master;
        }

        protected override void OnDraw(SpriteBatch spriteBatch) {
            // board state
            DrawBoard(spriteBatch);

            // clue numbers
            if (ShowClues)
                DrawClues(spriteBatch);
        }

        protected override void OnUpdate(GameTime gameTime) {
            if (Master.IsFinished()) {
                hover = (x: -1, y: -1);
                return;
            }

            if (!IsActiveLayer) return;

            // scroll zoom
            if (Input.MouseScroll > 0 && tilesizeindex < 4) {
                ZoomTo(tilesizeindex + 1);
            }
            if (Input.MouseScroll < 0 && tilesizeindex > 0) {
                ZoomTo(tilesizeindex - 1);
            }

            // cell hover
            if (Input.MouseX < Position.x ||
                Input.MouseY < Position.y ||
                Input.MouseX >= Position.x + board.Width * tilesize ||
                Input.MouseY >= Position.y + board.Height * tilesize) {
                hover = (x: -1, y: -1);
            } else {
                hover.x = (Input.MouseX - Position.x) / tilesize;
                hover.y = (Input.MouseY - Position.y) / tilesize;
            }

            // board panning
            if (Input.GetKey(Keys.Space) || Input.GetMouseMiddle()) {
                // reset hover coords if moving board
                hover = (x: -1, y: -1);
                if (Input.GetKeyDown(Keys.Space) || Input.GetMouseMiddleDown()) {
                    // hacky fix for board instantly moving
                    lastMouseX = Input.MouseX;
                    lastMouseY = Input.MouseY;
                }

                if (Input.GetMouseLeft() || Input.GetMouseRight() || Input.GetMouseMiddle()) {
                    int dx = Input.MouseX - lastMouseX;
                    int dy = Input.MouseY - lastMouseY;
                    Position.x += dx;
                    Position.y += dy;
                }
                lastMouseX = Input.MouseX;
                lastMouseY = Input.MouseY;
            }

            // only respond to mouse events if the event was started in this form
            if (Input.GetMouseLeftDown() || Input.GetMouseRightDown())
                captureMouse = true;
            //if (Input.GetMouseLeftUp())
            //    captureMouse = false;

            // drawing on the board
            if (captureMouse) {
                if (Master.Editor)
                    UpdateInputEditor();
                else
                    UpdateInputGame();
            }

            // reset hover disable coords if not clicking anything
            if (!Input.GetMouseLeft() && !Input.GetMouseRight())
                oldhover = (x: -1, y: -1);

            // restrict panning to window bounds
            int margin = 100 + tilesize * 4;
            int xmax = Application.Inst.GameWidth - 16;
            int xmin = -board.Width * tilesize + margin;
            int ymax = Application.Inst.GameHeight - 16;
            int ymin = -board.Height * tilesize + margin;
            if (Position.x < xmin) Position.x = xmin;
            if (Position.x > xmax) Position.x = xmax;
            if (Position.y < ymin) Position.y = ymin;
            if (Position.y > ymax) Position.y = ymax;
        }

        public void Reset() {
            board.Prepare();
            History.Clear();
            Opacity = 1f;

            captureMouse = false;
            drawingLine = false;
            hover = (x: -1, y: -1);
            linestart = (x: -1, y: -1);
            lineend = (x: -1, y: -1);

            // find a good starting size to fit the whole board
            tilesizeindex = tilesizeByIndex.Length;
            do {
                tilesizeindex--;
                UpdateBoardAssets();
            } while (tilesizeindex > 0 && !TryTilesize(tilesize));

            Position.x = Application.Inst.GameWidth / 2 - board.Width * tilesize / 2;
            Position.y = Application.Inst.GameHeight / 2 - board.Height * tilesize / 2;
            statehor = new List<int>[board.Width];
            statever = new List<int>[board.Height];
        }

        public void RecomputeState() {
            statehor = new List<int>[board.Width];
            statever = new List<int>[board.Height];
            for (int i = 0; i < board.Width; i++)
                statehor[i] = board.FindEdges(i, false, true);
            for (int i = 0; i < board.Height; i++)
                statever[i] = board.FindEdges(i, true, true);
        }

        private bool TryTilesize(int size) {
            // measure the max size of the board
            int totalx = (int)(board.Width * 1.5f * size);
            int totaly = (int)(board.Height * 1.5f * size);

            return totaly < Application.Inst.GameHeight - 64 && totalx < Application.Inst.GameWidth - 256;
        }

        private void DrawBoard(SpriteBatch spriteBatch) {
            for (int y = 0; y < board.Height; y++) {
                for (int x = 0; x < board.Width; x++) {
                    // render board cells
                    CellState state;
                    if (Master.Editor)
                        state = board.GetSolutionAt(x, y) ? CellState.Filled : CellState.Empty;
                    else
                        state = board.GetStateAt(x, y);

                    spriteBatch.Draw(boardTexture,
                        new Vector2(Position.x + x * tilesize, Position.y + y * tilesize),
                        new Rectangle((int)state * tilesize, 0, tilesize, tilesize),
                        Color.White * Opacity);
                }

                // horizontal group indicators
                if (y == 0 || y == board.Height || y % 5 != 0) continue;

                // horizontal red bar
                spriteBatch.Draw(boardTexture,
                    new Rectangle(Position.x, Position.y + y * tilesize - (tilesize / 2), board.Width * tilesize, tilesize),
                    new Rectangle(5 * tilesize, 0, tilesize, tilesize),
                    Color.White * Opacity);

                // number
                if (board.Height > 10) {
                    var groupHintPos = new Vector2(
                        Math.Min(Application.Inst.GameWidth - 48, Position.x + board.Width * tilesize) + 6,
                        Position.y + y * tilesize - clueFont.MeasureString(y.ToString()).Y + 2
                    );
                    spriteBatch.DrawStringShadow(clueFont, y.ToString(), groupHintPos, 2, Color.AliceBlue * Opacity, Color.Black * Opacity);
                }
            }
            for (int x = 0; x < board.Width; x++) {
                // vertical group indicators
                if (x == 0 || x == board.Width || x % 5 != 0) continue;

                // red line
                spriteBatch.Draw(boardTexture,
                    new Rectangle(Position.x + x * tilesize - tilesize / 2, Position.y, tilesize, board.Height * tilesize),
                    new Rectangle(4 * tilesize, 0, tilesize, tilesize),
                    Color.White * Opacity);

                // number indicator
                if (board.Width > 10) {
                    var groupHintPos = new Vector2(
                        Position.x + x * tilesize - clueFont.MeasureString(x.ToString()).X - 2,
                        Math.Min(Application.Inst.GameHeight - 48, Position.y + board.Height * tilesize) + 6
                    );
                    spriteBatch.DrawStringShadow(clueFont, x.ToString(), groupHintPos, 2, Color.AliceBlue * Opacity, Color.Black * Opacity);
                }
            }

            // line drawing preview
            if (!drawingLine) return;
            for (int y = Math.Min(lineend.y, linestart.y); y <= Math.Max(lineend.y, linestart.y); y++)
            for (int x = Math.Min(lineend.x, linestart.x); x <= Math.Max(lineend.x, linestart.x); x++) {
                // when crossing out, do not overwrite filled squares
                if (targetState == CellState.Crossed && board.GetStateAt(x, y) == CellState.Filled) continue;
                // draw light preview
                spriteBatch.Draw(boardTexture,
                    new Vector2(Position.x + x * tilesize, Position.y + y * tilesize),
                    new Rectangle((int)(targetState) * tilesize, 0, tilesize, tilesize),
                    Color.LightSteelBlue * 0.75f);
            }
            int linearDist = Math.Max(Math.Abs(lineend.x - linestart.x), Math.Abs(lineend.y - linestart.y)) + 1;
            spriteBatch.DrawStringOutline(Assets.fontClues[4],
                lineCellTotal > 1 && linearDist != lineCellTotal && targetState != CellState.Empty
                    ? $"{linearDist} / {lineCellTotal}"
                    : linearDist.ToString(),
                Input.MousePosition + new Vector2(20, 20), 2, Color.AliceBlue, Color.Black);
        }

        private void DrawClues(SpriteBatch spriteBatch) {
            int clueBackLength = 64 + tilesize * 8;
            int clueShadowDist = tilesizeindex >= 4 ? 2 : 1;

            // horizontal clues
            for (int x = 0; x < board.CluesHor.Length; x++) {
                // background
                spriteBatch.Draw(Assets.GetTexture("board/clueh"),
                    new Rectangle(Position.x + x * tilesize, Position.y - clueBackLength, tilesize, clueBackLength),
                    (hover.x == x ? Color.White : (x % 2 == 0 ? clueColor1 : clueColor2)) * Opacity);

                // list of numbers
                var column = board.CluesHor[x];
                for (int y = 0; y < column.Count; y++) {
                    var str = column[y].ToString();
                    var size = clueFont.MeasureString(str);
                    var pos = new Vector2(
                        Position.x + (x + 0.5f) * tilesize,
                        Position.y - column.Count * tilesize + (y + 0.5f) * tilesize + tilesize / 12f);
                    var greyout = (statehor[x]?.SequenceEqual(column) ?? false) || board.HintCrossHor[x][y];
                    //spriteBatch.DrawString(clueFont, str, pos, Color.Black * Opacity, 0f, size * 0.5f, Vector2.One, SpriteEffects.None, 0f);
                    //spriteBatch.DrawString(clueFont, str, pos - Vector2.One * clueShadowDist, Color.White * Opacity, 0f, size * 0.5f, Vector2.One, SpriteEffects.None, 0f);
                    spriteBatch.DrawStringOutline(clueFont,
                        str,
                        pos,
                        clueShadowDist,
                        (greyout ? Color.LightGray : Color.White) * Opacity,
                        (greyout ? Color.Gray : Color.Black) * Opacity * 0.8f,
                        0f,
                        size * 0.5f,
                        Vector2.One,
                        SpriteEffects.None);
                    // click hint number to cross it out
                    // this should probably be in Update, but I don't want to duplicate all this pos calc code
                    if (Input.GetMouseLeftDown() && Input.GetMouseInArea((int)(pos.X - size.X * 0.5f), (int)(pos.Y - size.Y * 0.5f), (int)size.X, (int)size.Y))
                        board.ToggleHintCross(false, x, y);
                }
            }

            // vertical clues
            for (int y = 0; y < board.CluesVer.Length; y++) {
                // background
                spriteBatch.Draw(Assets.GetTexture("board/cluev"),
                    new Rectangle(Position.x - clueBackLength, Position.y + y * tilesize, clueBackLength, tilesize),
                    (hover.y == y ? Color.White : (y % 2 == 0 ? clueColor2 : clueColor1)) * Opacity);

                // list of numbers
                var row = board.CluesVer[y];
                for (int x = 0; x < row.Count; x++) {
                    var str = row[x].ToString();
                    var size = clueFont.MeasureString(str);
                    var pos = new Vector2(
                        Position.x - row.Count * tilesize + (x + 0.5f) * tilesize,
                        Position.y + (y + 0.5f) * tilesize + 1 + tilesize / 24f);
                    var greyout = (statever[y]?.SequenceEqual(row) ?? false) || board.HintCrossVer[y][x];
                    spriteBatch.DrawStringOutline(clueFont,
                        str,
                        pos,
                        clueShadowDist,
                        (greyout ? Color.LightGray : Color.White) * Opacity,
                        (greyout ? Color.Gray : Color.Black) * Opacity * 0.8f,
                        0f,
                        size * 0.5f,
                        Vector2.One,
                        SpriteEffects.None);
                    // click hint number to cross it out
                    if (Input.GetMouseLeftDown() && Input.GetMouseInArea((int)(pos.X - size.X * 0.5f), (int)(pos.Y - size.Y * 0.5f), (int)size.X, (int)size.Y))
                        board.ToggleHintCross(true, y, x);
                    //spriteBatch.DrawString(clueFont, str, pos - Vector2.One * clueShadowDist, Color.White * Opacity, 0f, size * 0.5f, Vector2.One, SpriteEffects.None, 0f);
                }
            }
        }

        private void UpdateBoardAssets() {
            tilesize = tilesizeByIndex[tilesizeindex];
            boardTexture = Assets.GetTexture("board/tiles" + tilesize);
            clueFont = Assets.fontClues[tilesizeindex];
        }

        private void ZoomTo(int level) {
            int oldTilesize = tilesize;
            tilesizeindex = level;
            UpdateBoardAssets();

            Position.x -= (tilesize - oldTilesize) * board.Width / 2;
            Position.y -= (tilesize - oldTilesize) * board.Height / 2;
        }

        private void UpdateInputGame() {
            // if mouse button is released, finalize
            if (drawingLine && !Input.GetMouseLeft() && !Input.GetMouseRight()) {
                FinishLine();
            }

            // avoid doing stuff outside of board coords
            if (hover.x == -1 || hover.y == -1) return;

            // begin line drawing by clicking a mouse button
            if (!drawingLine && Input.GetMouseLeftDown()) {
                // set the target state to the opposite of what the user clicked on
                targetState = board.GetStateAt(hover.x, hover.y) != CellState.Empty && board.GetStateAt(hover.x, hover.y) != CellState.CrossedAuto
                    ? CellState.Empty
                    : CellState.Filled;
                linestart = (x: hover.x, y: hover.y);
                drawingLine = true;
                //Audio.PlaySound("event:/UI/PencilMode");
            }
            if (!drawingLine && Input.GetMouseRightDown()) {
                // set the target state to the opposite of what the user clicked on
                targetState = board.GetStateAt(hover.x, hover.y) == CellState.Crossed
                    ? CellState.Empty
                    : CellState.Crossed;
                linestart = (x: hover.x, y: hover.y);
                drawingLine = true;
                //Audio.PlaySound("event:/UI/PencilMode");
            }

            // are we drawing a line, and hovering over a different tile than last frame?
            if (!drawingLine) return;
            if (hover.x == oldhover.x && hover.y == oldhover.y) return;
            // save hover coords so we don't repeat them over and over
            oldhover.x = hover.x;
            oldhover.y = hover.y;

            //int linearDist = (int)Math.Ceiling(Vector2.Distance(new Vector2(hover.x, hover.y), new Vector2(linestartx, linestarty)));
            int linearDist = Math.Max(Math.Abs(hover.x - linestart.x), Math.Abs(hover.y - linestart.y));
            var targetList = new[] {
                // four possible targets
                (x: linestart.x - linearDist, y: linestart.y, vertical: false),
                (x: linestart.x + linearDist, y: linestart.y, vertical: false),
                (x: linestart.x, y: linestart.y - linearDist, vertical: true),
                (x: linestart.x, y: linestart.y + linearDist, vertical: true),
            };
            // figure out the closest line end point
            var bestDist = float.MaxValue;
            var bestTarget = (x: 0, y: 0, vertical: false);
            for (int i = 0; i < targetList.Length; i++) {
                var newdist = Vector2.Distance(new Vector2(hover.x, hover.y), new Vector2(targetList[i].x, targetList[i].y));
                if (newdist < bestDist) {
                    bestDist = newdist;
                    bestTarget = targetList[i];
                }
            }
            if (bestTarget.x != lineend.x || bestTarget.y != lineend.y) {
                // if this new target differs somehow, play a sound and save the new target
                lineend = (x: bestTarget.x, y: bestTarget.y);
                Audio.PlaySound("event:/UI/PencilMode");
            }
            // figure out the total length of this line
            if (bestTarget.vertical) {
                int ymin = Math.Min(linestart.y, lineend.y);
                int ymax = Math.Max(linestart.y, lineend.y);
                while (ymin > 0 && board.GetStateAt(linestart.x, ymin - 1) == targetState)
                    ymin--;
                while (ymax < board.Height - 1 && board.GetStateAt(linestart.x, ymax + 1) == targetState)
                    ymax++;
                lineCellTotal = ymax - ymin + 1;
            } else {
                int xmin = Math.Min(linestart.x, lineend.x);
                int xmax = Math.Max(linestart.x, lineend.x);
                while (xmin > 0 && board.GetStateAt(xmin - 1, linestart.y) == targetState)
                    xmin--;
                while (xmax < board.Width - 1 && board.GetStateAt(xmax + 1, linestart.y) == targetState)
                    xmax++;
                lineCellTotal = xmax - xmin + 1;
            }
        }

        private void FinishLine() {
            // push undo state
            History.Push(board);
            Master.UpdateUndoRedoButtons();
            // draw the line
            for (int y = Math.Min(lineend.y, linestart.y); y <= Math.Max(lineend.y, linestart.y); y++)
            for (int x = Math.Min(lineend.x, linestart.x); x <= Math.Max(lineend.x, linestart.x); x++) {
                // when crossing out, do not overwrite filled squares
                if (targetState == CellState.Crossed && board.GetStateAt(x, y) == CellState.Filled) continue;
                // update board state
                board.SetStateAt(x, y, targetState);
            }
            // reset linedrawing state
            drawingLine = false;
            linestart = (x: -1, y: -1);
            lineend = (x: -1, y: -1);
            // sound effect
            switch (targetState) {
                case CellState.Empty:
                    Audio.PlaySound("event:/Board/Erase");
                    break;
                case CellState.Filled:
                    Audio.PlaySound("event:/Board/Fill");
                    break;
                case CellState.Crossed:
                    Audio.PlaySound("event:/Board/Cross");
                    break;
            }

            // move over the grid to find completed lines, and pseudo-crossout cells that were left open
            for (int checkx = 0; checkx < board.Width; checkx++) {
                // columns
                statehor[checkx] = board.FindEdges(checkx, false, true);
                if (!UserConfig.AutoCrossout || !statehor[checkx].SequenceEqual(board.CluesHor[checkx])) continue;
                // the column matches the clues, so cross out other squares
                for (int pcelly = 0; pcelly < board.Height; pcelly++)
                    if (board.GetStateAt(checkx, pcelly) == CellState.Empty)
                        board.SetStateAt(checkx, pcelly, CellState.CrossedAuto);
            }
            for (int checky = 0; checky < board.Height; checky++) {
                // rows
                statever[checky] = board.FindEdges(checky, true, true);
                if (!UserConfig.AutoCrossout || !statever[checky].SequenceEqual(board.CluesVer[checky])) continue;
                // the row matches the clues, so cross out other squares
                for (int pcellx = 0; pcellx < board.Width; pcellx++)
                    if (board.GetStateAt(pcellx, checky) == CellState.Empty)
                        board.SetStateAt(pcellx, checky, CellState.CrossedAuto);
            }

            // maybe we finished the board now?
            if (board.IsSolved()) {
                PlayArchive.MarkSolved(board.ID);
                Audio.PlayMusic("event:/Music/Clear");
                Master.BeginVictoryDance();
            }
        }

        private void UpdateInputEditor() {
            // are we hovering over a different tile than last frame?
            if (hover.x == -1 || (hover.x == oldhover.x && hover.y == oldhover.y)) return;
            if (!Input.GetMouseLeft()) return;

            // fill cells
            if (Input.GetMouseLeftDown()) {
                // set the target state to the opposite of what the user clicked on
                targetState = board.GetSolutionAt(hover.x, hover.y)
                    ? CellState.Empty
                    : CellState.Filled;
            }
            board.SetSolutionAt(hover.x, hover.y, targetState == CellState.Filled);
            board.GenerateClues();
            Master.MakeDirty();
            oldhover.x = hover.x;
            oldhover.y = hover.y;
        }

    }

}
