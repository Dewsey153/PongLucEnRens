using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Net;
using System.Security.Cryptography.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace pong
{
    public class pong : Game
    {
        KeyboardState currentKBState = Keyboard.GetState();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D ball;
        Texture2D blue;
        Texture2D red;
        Vector2 ballPosition;
        Vector2 bluePosition;
        Vector2 redPosition;
        int blueY = 100;
        int redY = 100;
        int ballX = 0;
        int playerSpeed = 3;

        static void Main()
        {
            pong game = new pong();
            game.Run();
        }

        public pong()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            ball = Content.Load<Texture2D>("ball");
            blue = Content.Load<Texture2D>("bluePlayer");
            red = Content.Load<Texture2D>("redPlayer");
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space)){
                ballX -=1;
            }
            else
            {
                ballX += 1;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                blueY = blueY - playerSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                blueY = blueY + playerSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                redY = redY - playerSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                redY = redY + playerSpeed;
            }

            ballPosition = new Vector2(ballX, 100);
            bluePosition = new Vector2(10, blueY);
            redPosition = new Vector2(400, redY);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            spriteBatch.Draw(ball, ballPosition, Color.White);
            spriteBatch.Draw(blue, bluePosition, Color.White);
            spriteBatch.Draw(red, redPosition, Color.White);
            spriteBatch.End();
        }
    }
}