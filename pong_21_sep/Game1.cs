using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Net;
using System.Resources;
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
        int ballX = 10;
        int ballY = 10;
        Vector2 ballSpeed;
        int ballSpeedXRandom;
        int playerSpeed = 3;
        Random random = new Random();


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
            ballX = graphics.PreferredBackBufferWidth / 2;
            ballY = graphics.PreferredBackBufferHeight / 2;
            int ballSpeedXRandomTemp = random.Next(-4, 4);
            if (ballSpeedXRandomTemp == 0)
            {
                ballSpeedXRandomTemp = random.Next(-4, 4);
            }
            else ballSpeedXRandom = ballSpeedXRandomTemp;

            ballSpeed = new Vector2(ballSpeedXRandom, 5 - Math.Abs(ballSpeedXRandom));
            ballPosition = new Vector2(ballX, ballY);
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

            ballX = (int)ballPosition.X;
            ballY = (int)ballPosition.Y;

            if (redY <= 0) redY = 0;
            if (blueY <= 0) blueY = 0;
            if (redY >= graphics.PreferredBackBufferHeight - red.Height) redY = graphics.PreferredBackBufferHeight - red.Height;
            if (blueY >= graphics.PreferredBackBufferHeight - blue.Height) blueY = graphics.PreferredBackBufferHeight - blue.Height;
            //bounce from walls
            if (ballX >= graphics.PreferredBackBufferWidth - ball.Width || ballX <= 0) ballSpeed.X = -1 * ballSpeed.X;
            if (ballY >= graphics.PreferredBackBufferHeight - ball.Height || ballY <= 0) ballSpeed.Y = -1 * ballSpeed.Y;
            //move ball and players
            ballPosition = Vector2.Add(ballPosition, ballSpeed);
            bluePosition = new Vector2(0, blueY);
            redPosition = new Vector2(graphics.PreferredBackBufferWidth - blue.Width, redY);
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