using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pfim.MonoGame
{
    public class MyGame : Game
    {
        private SpriteBatch spriteBatch;
        private Texture2D texture;
        private readonly List<string> files;
        private string imgFile = "";
        private int imgIndex;
        private MouseState oldState;

        private void NextFile()
        {
            if (imgIndex == files.Count)
            {
                imgIndex = 0;
            }

            imgFile = files[imgIndex];
            imgIndex++;
        }

        private void PreviousFile()
        {
            if (imgIndex == 0)
            {
                imgIndex = files.Count - 1;
            }

            imgFile = files[imgIndex];
            imgIndex--;
        }

        public MyGame()
        {
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };

            var imgDirectory = Path.Combine(Environment.CurrentDirectory, "data");
            files = Directory.EnumerateFiles(imgDirectory, "*.dds")
                .Concat(Directory.EnumerateFiles(imgDirectory, "*.tga")).ToList();

            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Window.Title = Path.GetFileName(imgFile);

            // Load image data into memory
            if (imgFile != "")
            {
                texture = CreateTexture(imgFile);
            }
        }

        private Texture2D CreateTexture(string file)
        {
            var image = Pfim.FromFile(file);

            image.ApplyColorMap();

            byte[] newData;

            // Since mono game can't handle data with line padding in a stride
            // we create an stripped down array if any padding is detected
            var tightStride = image.Width * image.BitsPerPixel / 8;
            if (image.Stride != tightStride)
            {
                newData = new byte[image.Height * tightStride];
                for (int i = 0; i < image.Height; i++)
                {
                    Buffer.BlockCopy(image.Data, i * image.Stride, newData, i * tightStride, tightStride);
                }
            }
            else
            {
                newData = image.Data;
            }

            // I believe mono game core is limited in its texture support
            // so we're assuming 32bit data format is needed. One can always
            // upscale 24bit / 16bit / 15bit data (not shown in sample).
            var newTexture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);

            switch (image.Format)
            {
                case ImageFormat.Rgba32:
                    // Flip red and blue color channels.
                    for (int i = 0; i < newData.Length; i += 4)
                    {
                        var temp = newData[i + 2];
                        newData[i + 2] = newData[i];
                        newData[i] = temp;
                    }

                    newTexture.SetData(newData);
                    break;
            }

            return newTexture;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (state.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (mouseState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                NextFile();
                LoadContent();
            }
            else if (mouseState.RightButton == ButtonState.Pressed && oldState.RightButton == ButtonState.Released)
            {
                PreviousFile();
                LoadContent();
            }

            oldState = mouseState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (texture != null)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(texture, new Vector2(0, 0), Color.AliceBlue);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}