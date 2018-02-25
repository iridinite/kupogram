/*
 * Kupogram - a nonogram/picross game and editor
 * (C) Mika Molenkamp, October/November 2017
 * https://www.syllendel.nl/
 * 
 * Licensed under BSD-3-clause license.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kupogram {

    /// <summary>
    /// Core game application.
    /// </summary>
    public class Application : Game {

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Form form;

        private readonly Stack<Control> UIStack = new Stack<Control>();

        public static Application Inst { get; private set; }

        //public event EventHandler OnResize;

        public int GameWidth { get; private set; }
        public int GameHeight { get; private set; }

        private static readonly string[] blurbs = {
            "Crosses & Squares Edition",
            "Grid Simulator 2017",
            "Season Pass Now Available",
            "35% Off Cosmetic Microtransactions",
            "Home-grown, Organic & DRM-free",
            "Solutions Not Included",
            "No Bugtesting For You",
            "Vol. 1: The Betrayal of the Hint Number",
            "Vol. 2: The Revenge of the Puzzle Clue",
            "Vol. 3: The Return of the Answer",
            "Vol. 4: The Filled Square Strikes Back"
        };

        public Application() {
            Inst = this;
            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            // game window configuration
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Title = $"Kupogram: {blurbs[Util.RNG.Next(blurbs.Length)]}";
            form = (Form)System.Windows.Forms.Control.FromHandle(Window.Handle);
            form.MinimumSize = new System.Drawing.Size(1280, 720);
            form.Resize += Window_ClientSizeChanged;
            form.WindowState = FormWindowState.Maximized;
            Exiting += Window_Exiting;

            // singleton initialization
            Audio.Initialize();
            UserConfig.Load();
            PlayArchive.Load();
            Util.RefreshPuzzleCache();

            // monogame core init
            base.Initialize();

            // store provisional width/height
            GameWidth = Window.ClientBounds.Width;
            GameHeight = Window.ClientBounds.Height;
        }

        private void Window_Exiting(object sender, EventArgs e) {
            Audio.Shutdown();
            PlayArchive.Save();
            UserConfig.Save();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e) {
            GameWidth = Window.ClientBounds.Width;
            GameHeight = Window.ClientBounds.Height;

            graphics.PreferredBackBufferWidth = GameWidth;
            graphics.PreferredBackBufferHeight = GameHeight;
            graphics.ApplyChanges();

            //OnResize?.Invoke(sender, e);
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Assets.LoadContent(GraphicsDevice, Content);
        }

        protected override void Update(GameTime gameTime) {
            if (UIStack.Count == 0)
                //SetUI(new DialogOptions());
                //SetUI(new FormGame(Board.FromImage("Puzzles/pokeball.png"), false));
                SetUI(new FormMainMenu());

            UIStack.Reverse().ForEach(ctrl => ctrl.Update(gameTime));

            Input.Update();
            Audio.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            UIStack.Reverse().ForEach(ctrl => ctrl.Render(GraphicsDevice, spriteBatch));

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap);
            UIStack.Reverse().ForEach(ctrl => ctrl.Draw(spriteBatch));
            Tooltip.DrawQueued(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void PushUI(Control ctl) {
            UIStack.Push(ctl);
        }

        public void PopUI() {
            //UIStack.Peek();
            UIStack.Pop().Destroy();
        }

        public void SetUI(Control ctl) {
            while (UIStack.Count > 0)
                PopUI();
            PushUI(ctl);
        }

        public Control GetActiveUILayer() {
            return UIStack.Peek();
        }

    }

}
