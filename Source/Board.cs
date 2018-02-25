/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Describes the state of a cell on the game board.
    /// </summary>
    public enum CellState {
        Empty = 0,
        Filled = 1,
        Crossed = 2,
        CrossedAuto = 3
    }

    /// <summary>
    /// Represents a nonogram puzzle board.
    /// </summary>
    public sealed class Board {

        private static readonly char[] PUZZLE_FILE_MAGIC = {'K', 'G', 'R', 'M'};
        private const byte PUZZLE_FILE_VERSION = 2;

        private bool[] solution;
        private string cachedFilename = String.Empty;
        private string hiddenTitle = "????????";

        /// <summary>
        /// The unique ID of this puzzle.
        /// </summary>
        public Guid ID { get; private set; }

        /// <summary>
        /// The unique ID of this puzzle.
        /// </summary>
        public Guid Author { get; set; }

        /// <summary>
        /// The name of the nonogram puzzle.
        /// </summary>
        public string Title { get; set; } = "Untitled";

        /// <summary>
        /// Returns the puzzle name if the puzzle was solved before, otherwise a string of question marks.
        /// </summary>
        public string DisplayTitle => UserConfig.ShowPuzzleNames || IsPreviouslySolved ? Title : hiddenTitle;

        /// <summary>
        /// Returns true if the puzzle has already been solved, or if the current player was the author.
        /// </summary>
        public bool IsPreviouslySolved => PlayArchive.Identity == Author || PlayArchive.IsSolved(ID);

        /// <summary>
        /// Gets or sets horizontal size of the grid.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets or sets the vertical size of the grid.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets or sets the board state array (user input).
        /// </summary>
        public CellState[] State { get; set; }

        /// <summary>
        /// Gets or sets a preview image. May be null if not yet generated.
        /// </summary>
        public RenderTarget2D Preview { get; private set; }

        /// <summary>
        /// Returns the set of clues on the x axis.
        /// </summary>
        public List<int>[] CluesHor { get; private set; }

        /// <summary>
        /// Returns the set of clues on the y axis.
        /// </summary>
        public List<int>[] CluesVer { get; private set; }

        /// <summary>
        /// Returns the set of clues on the x axis.
        /// </summary>
        public List<bool>[] HintCrossHor { get; private set; }

        /// <summary>
        /// Returns the set of clues on the y axis.
        /// </summary>
        public List<bool>[] HintCrossVer { get; private set; }

        /// <summary>
        /// Gets a list of <seealso cref="Category"/> entries this board belongs to.
        /// </summary>
        public List<Guid> Categories { get; } = new List<Guid>();

        /// <summary>
        /// Constructs an uninitialized Board object.
        /// </summary>
        private Board() {}

        /// <summary>
        /// Constructs a Board with a specified size and nonogram image.
        /// </summary>
        private Board(int width, int height, bool[] solution) {
            Width = width;
            Height = height;
            this.solution = solution;
        }

        /// <summary>
        /// Constructs an empty board with a specified size and a random unique ID.
        /// </summary>
        public Board(int width, int height)
            : this(width, height, new bool[width * height]) {
            ID = Guid.NewGuid();
            Author = PlayArchive.Identity;
        }

        /// <summary>
        /// Creates a new board based on an image file.
        /// </summary>
        /// <param name="filename">The path to the file.</param>
        public static Board FromImage(string filename) {
            using (var image = new System.Drawing.Bitmap(filename)) {
                bool[] solution = new bool[image.Width * image.Height];
                for (int y = 0; y < image.Height; y++) {
                    for (int x = 0; x < image.Width; x++) {
                        solution[y * image.Width + x] = image.GetPixel(x, y).GetBrightness() < 0.5f;
                    }
                }
                return new Board(image.Width, image.Height, solution) {
                    ID = Guid.NewGuid(),
                    Author = PlayArchive.Identity
                };
            }
        }

        /// <summary>
        /// Parses a saved Kupogram puzzle file and returns it as a Board instance.
        /// </summary>
        /// <param name="filename">The path to the file.</param>
        public static Board FromFile(string filename) {
            using (StreamReader stream = new StreamReader(filename)) {
                using (BinaryReader reader = new BinaryReader(stream.BaseStream, Encoding.UTF8, true)) {
                    // verify magic number and file version
                    if (!PUZZLE_FILE_MAGIC.SequenceEqual(reader.ReadChars(4)))
                        throw new InvalidDataException("File is corrupted or not a Kupogram board");
                    byte version = reader.ReadByte();
                    if (version < 1 || version > PUZZLE_FILE_VERSION)
                        throw new InvalidDataException("File version incompatible");

                    try {
                        // set up a board with basic metadata
                        var ret = new Board {
                            ID = new Guid(reader.ReadBytes(16)),
                            Author = new Guid(reader.ReadBytes(16)),
                            Title = reader.ReadString(),
                            Width = reader.ReadInt32(),
                            Height = reader.ReadInt32()
                        };

                        ret.hiddenTitle = new string('?', ret.Title.Length);
                        ret.cachedFilename = filename;

                        // load list of categories
                        if (version >= 2) {
                            int numCategories = reader.ReadInt32();
                            while (numCategories > 0) {
                                ret.Categories.Add(new Guid(reader.ReadBytes(16)));
                                numCategories--;
                            }
                        }

                        // deserialize the solution array
                        ret.solution = new bool[ret.Width * ret.Height];
                        var serialized = new BitArray(reader.ReadBytes(reader.ReadInt32()));
                        for (int i = 0; i < ret.Width * ret.Height; i++)
                            ret.solution[i] = serialized[i];

                        return ret;
                    } catch (Exception ex) {
                        throw new InvalidDataException("Failed to read puzzle file.", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the board for usage in gameplay.
        /// </summary>
        public void Prepare() {
            State = new CellState[Width * Height];
            GenerateClues();
        }

        /// <summary>
        /// Regenerates the list of clues for both axes.
        /// </summary>
        public void GenerateClues() {
            CluesHor = FindEdges(false, false);
            CluesVer = FindEdges(true, false);

            //if (HintCrossHor == null || HintCrossHor.Length != CluesHor.Length)
            HintCrossHor = new List<bool>[CluesHor.Length];
            //if (HintCrossVer == null || HintCrossVer.Length != CluesVer.Length)
            HintCrossVer = new List<bool>[CluesVer.Length];

            for (int i = 0; i < CluesHor.Length; i++) {
                // skip if already correctly sized
                if (HintCrossHor[i] != null && HintCrossHor[i].Count == CluesHor[i].Count) continue;
                // make new list and add as many bools as there are hint numbers
                HintCrossHor[i] = new List<bool>();
                CluesHor[i].ForEach(_ => HintCrossHor[i].Add(false));
            }

            for (int i = 0; i < CluesVer.Length; i++) {
                // skip if already correctly sized
                if (HintCrossVer[i] != null && HintCrossVer[i].Count == CluesVer[i].Count) continue;
                // make new list and add as many bools as there are hint numbers
                HintCrossVer[i] = new List<bool>();
                CluesVer[i].ForEach(_ => HintCrossVer[i].Add(false));
            }
        }

        /// <summary>
        /// Resizes the game board to a new specific size.
        /// </summary>
        public void Resize(int newwidth, int newheight) {
            // allocate a new solution array and copy the overlapping area
            var newsolution = new bool[newwidth * newheight];
            for (int y = 0; y < Math.Min(newheight, Height); y++)
            for (int x = 0; x < Math.Min(newwidth, Width); x++) {
                newsolution[y * newwidth + x] = GetSolutionAt(x, y);
            }

            // save changes
            solution = newsolution;
            Width = newwidth;
            Height = newheight;
            Prepare();
        }

        /// <summary>
        /// Detects edges in a single row or column of the nonogram image.
        /// </summary>
        /// <param name="i">The row or column index.</param>
        /// <param name="vertical">If true, use a column, otherwise, use a row.</param>
        /// <param name="useState">If true, look at player input state, otherwise, use the solution.</param>
        public List<int> FindEdges(int i, bool vertical, bool useState) {
            bool GetAt(int x, int y) {
                if (useState)
                    return GetStateAt(x, y) == CellState.Filled;
                return GetSolutionAt(x, y);
            }

            var maxj = vertical ? Width : Height;
            var ret = new List<int>();
            var found = 0;
            for (int j = 0; j < maxj; j++) {
                var mustFill = vertical ? GetAt(j, i) : GetAt(i, j);
                if (!mustFill && found > 0) {
                    ret.Add(found);
                    found = 0;
                }
                if (mustFill) {
                    found++;
                }
            }
            if (found > 0 || ret.Count == 0) {
                ret.Add(found);
            }
            return ret;
        }

        /// <summary>
        /// Detects edges in the nonogram image and outputs a list of clues.
        /// </summary>
        public List<int>[] FindEdges(bool vertical, bool useState) {
            var edgetable = new List<int>[vertical ? Height : Width];
            var maxi = vertical ? Height : Width;

            for (int i = 0; i < maxi; i++) {
                edgetable[i] = FindEdges(i, vertical, useState);
            }

            return edgetable;
        }

        /// <summary>
        /// Toggles the crossout state of a hint number.
        /// </summary>
        /// <param name="vertical">Specifies which axis. If true, use CluesVer, otherwise CluesHor.</param>
        /// <param name="i">The first array index.</param>
        /// <param name="j">The second array index.</param>
        public void ToggleHintCross(bool vertical, int i, int j) {
            var arr = vertical ? HintCrossVer : HintCrossHor;
            var newstate = !arr[i][j];
            arr[i][j] = newstate;

            Audio.PlaySound(newstate ? "event:/Board/Cross" : "event:/Board/Erase");
        }

        /// <summary>
        /// Returns a UI-friendly string that lists this board's category names.
        /// </summary>
        public string GetCategoryNames() {
            // find the names associated with the guids
            var list = new List<string>(
                Categories.Select(target => Category.All.Find(candidate => candidate.ID == target).Name)
            );
            if (list.Count == 0)
                list.Add("None");
            //Category.All.Where(cat => Categories.Contains(cat.ID))
            //Categories.ForEach(guid => list.Add(Category.All.Find(cat => cat.ID == guid)));
            return string.Join(", ", list);
        }

        /// <summary>
        /// Returns whether the cell at the specified location must be filled or not.
        /// </summary>
        public bool GetSolutionAt(int x, int y) {
            return solution[y * Width + x];
        }

        /// <summary>
        /// Changes the solution state at the specified location.
        /// </summary>
        public void SetSolutionAt(int x, int y, bool fill) {
            // skip the effects etc if we're not changing anything
            if (GetSolutionAt(x, y) == fill) return;

            solution[y * Width + x] = fill;
            Audio.PlaySound(fill ? "event:/Board/Fill" : "event:/Board/Erase");
        }

        /// <summary>
        /// Returns the cell state at the specified location.
        /// </summary>
        public CellState GetStateAt(int x, int y) {
            return State[y * Width + x];
        }

        /// <summary>
        /// Changes the cell state at the specified location and plays a sound.
        /// </summary>
        public void SetStateAt(int x, int y, CellState val) {
            // skip the effects etc if we're not changing anything
            if (GetStateAt(x, y) == val) return;

            // store board state
            State[y * Width + x] = val;

            // erase auto-placed crosses in this row and col
            if (val == CellState.Empty || val == CellState.Filled) {
                for (int i = 0; i < Width; i++)
                    if (GetStateAt(i, y) == CellState.CrossedAuto)
                        SetStateAt(i, y, CellState.Empty);
                for (int i = 0; i < Height; i++)
                    if (GetStateAt(x, i) == CellState.CrossedAuto)
                        SetStateAt(x, i, CellState.Empty);
            }
        }

        /// <summary>
        /// Returns true if the puzzle has been solved (i.e. all cell states are correct).
        /// </summary>
        public bool IsSolved() {
            // try to find a cell that has an incorrect state
            for (int i = 0; i < solution.Length; i++) {
                if (solution[i] && State[i] != CellState.Filled) return false;
                if (!solution[i] && State[i] == CellState.Filled) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a file name that can be used for storing this puzzle.
        /// </summary>
        private string GetFilename() {
            if (String.IsNullOrWhiteSpace(Title))
                throw new ArgumentNullException(nameof(Title));

            // generate and cache a filename based on title and GUID
            return Util.PuzzleCustomDir.FullName
                + Path.DirectorySeparatorChar
                + new string($"{Title.Substring(0, Math.Min(Title.Length, 20))}_{ID}".ToLowerInvariant().Select(ch => {
                    // strip non-alphanumeric characters
                    if ((ch < 'a' || ch > 'z') && (ch < '0' || ch > '9'))
                        return '_';
                    return ch;
                }).ToArray())
                + ".kgram";
        }

        /// <summary>
        /// Saves the puzzle to disk.
        /// </summary>
        public void Save() {
            // this mechanism ensures that if the puzzle name changes, the file name will also change
            // but no duplicate (old) file will be left behind
            if (File.Exists(cachedFilename))
                File.Delete(cachedFilename);
            cachedFilename = GetFilename();

            // write file to disk
            using (StreamWriter stream = new StreamWriter(cachedFilename)) {
                using (BinaryWriter writer = new BinaryWriter(stream.BaseStream, Encoding.UTF8, true)) {
                    // general metadata
                    writer.Write(PUZZLE_FILE_MAGIC);
                    writer.Write(PUZZLE_FILE_VERSION);
                    writer.Write(ID.ToByteArray());
                    writer.Write(Author.ToByteArray());
                    writer.Write(Title);
                    writer.Write(Width);
                    writer.Write(Height);

                    // array of categories
                    writer.Write(Categories.Count);
                    Categories.ForEach(guid => writer.Write(guid.ToByteArray()));

                    // solution array, serialized as a compact bit array
                    var array = new BitArray(solution);
                    var serialized = new byte[array.Count / 8 + 1];
                    array.CopyTo(serialized, 0);
                    writer.Write(serialized.Length);
                    writer.Write(serialized);
                }
            }
        }

        /// <summary>
        /// Creates and caches an image showing the solution of this nonogram.
        /// </summary>
        /// <param name="graphicsDevice">A <seealso cref="GraphicsDevice"/> to use for drawing.</param>
        /// <param name="spriteBatch">A <seealso cref="SpriteBatch"/>, associated with the <paramref name="graphicsDevice"/>, to use for 2D drawing.</param>
        public void GeneratePreviewImage(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch) {
            if (Preview != null) return;

            int maxdim = Math.Max(Width, Height);
            int cellsize = (int)Math.Ceiling(200f / maxdim); //  (int)((1f - maxdim / 100f) * 30);

            // create and activate a render target for our preview image
            Preview = new RenderTarget2D(graphicsDevice, Width * cellsize + 20, Height * cellsize + 20, false, SurfaceFormat.Color, DepthFormat.None);
            graphicsDevice.SetRenderTarget(Preview);
            graphicsDevice.Clear(Color.White);

            // draw the image, add black squares for filled images
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    if (GetSolutionAt(x, y))
                        spriteBatch.Draw(
                            Assets.GetTexture("ui/rect"),
                            new Rectangle(10 + x * cellsize, 10 + y * cellsize, cellsize, cellsize),
                            Color.Black);
                }
            }
            spriteBatch.End();
        }

    }

}
