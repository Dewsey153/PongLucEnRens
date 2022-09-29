using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using System;
using System.Net;
using System.Reflection.Metadata;
using System.Resources;
using System.Security.Cryptography.Xml;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Color = Microsoft.Xna.Framework.Color;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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
        Vector2 ballDirection;
        int blueY = 100;
        int redY = 100;
        float ballX = 10f;
        float ballY = 10f;
        float ballSpeed;
        const float initialballSpeed = 4f;
        const float acceleration = 1.1f;
        int ballSpeedXRandom = 3;
        int playerSpeed = 10;
        float minBallDirectionX = 0.3f;
        float minBallDirectionY = 0.2f;
        Random random = new Random();
        Lives redLives;
        Lives blueLives;
        


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
            startBall();
            redLives = new Lives(Content, graphics.PreferredBackBufferWidth - 76);
            blueLives = new Lives(Content, 20);
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
                blueY = blueY - playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                blueY = blueY + playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                redY = redY - playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                redY = redY + playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
            }

            ballX = ballPosition.X;
            ballY = ballPosition.Y;

            if (redY <= 0) redY = 0;
            if (blueY <= 0) blueY = 0;
            if (redY >= graphics.PreferredBackBufferHeight - red.Height) redY = graphics.PreferredBackBufferHeight - red.Height;
            if (blueY >= graphics.PreferredBackBufferHeight - blue.Height) blueY = graphics.PreferredBackBufferHeight - blue.Height;

            ballHitPaddle();
            ballMissed();
            //bounce from walls
            if (ballY >= graphics.PreferredBackBufferHeight - ball.Height || ballY <= 0) ballDirection.Y = -1 * ballDirection.Y;
            //move ball and players
            ballDirection.Normalize();
            if (ballDirection.X < minBallDirectionX && ballDirection.X > 0) ballDirection.X = minBallDirectionX; // X component ballDirection must be higher than a certain value to make sure the ball doesn't move too slow to the other side
            if (ballDirection.X > -minBallDirectionX && ballDirection.X < 0) ballDirection.X = -minBallDirectionX;
            if (ballDirection.Y < minBallDirectionY && ballDirection.Y > 0) ballDirection.Y = minBallDirectionY; // Y component ballDirection must be higher than a certain value to make sure the ball doesn't move in an almost straight line for eternity
            if (ballDirection.Y > -minBallDirectionY && ballDirection.Y < 0) ballDirection.Y = -minBallDirectionY;

            ballDirection.Normalize();
            ballPosition = Vector2.Add(ballPosition, ballSpeed * ballDirection*gameTime.ElapsedGameTime.Milliseconds/10);
            bluePosition = new Vector2(0, blueY);
            redPosition = new Vector2(graphics.PreferredBackBufferWidth - blue.Width, redY);
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                startBall();
            }
            GameRestart();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            spriteBatch.Draw(ball, ballPosition, Color.White);
            spriteBatch.Draw(blue, bluePosition, Color.White);
            spriteBatch.Draw(red, redPosition, Color.White);
            blueLives.Draw(gameTime, spriteBatch);
            redLives.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }
        
        public void startBall()
        {
            ballSpeed = initialballSpeed;
            ballX = graphics.PreferredBackBufferWidth / 2;
            ballY = graphics.PreferredBackBufferHeight / 2;
            int ballSpeedXRandomTemp = random.Next(-4, 4);
            if (ballSpeedXRandomTemp == 0)
            {
                ballSpeedXRandomTemp = random.Next(-4, 4);
            }
            else ballSpeedXRandom = ballSpeedXRandomTemp;

            ballDirection = new Vector2(ballSpeedXRandom, 5 - Math.Abs(ballSpeedXRandom));
            ballPosition = new Vector2(ballX, ballY);
        }

        public void ballHitPaddle()
        {
            if (ballX <= (bluePosition.X + blue.Width))
            {
                if (ballY + ball.Height >= bluePosition.Y && ballY <= bluePosition.Y + blue.Height)
                {
                    Vector2 Angle = new Vector2(blue.Width, ballY - (bluePosition.Y + blue.Height / 2));
                    Angle.Normalize();
                    if (Angle.Y != 0 || Angle.X != 0)
                    {
                        ballDirection.X = ballDirection.X * -Angle.X * 3;
                        ballDirection.Y = ballDirection.Y * Angle.Y * 3;
                    }
                    else ballDirection.X *= -1;
                    ballSpeed *= acceleration;
                }
            }

            else if (ballX + ball.Width >= redPosition.X)
            {
                if (ballY + ball.Height >= redPosition.Y && ballY <= redPosition.Y + red.Height)
                {
                    Vector2 Angle = new Vector2(graphics.PreferredBackBufferWidth - red.Width, ballY - (redPosition.Y + red.Height / 2));
                    Angle.Normalize();
                    if (Angle.Y != 0 || Angle.X != 0)
                    {
                        ballDirection.X = ballDirection.X * -Angle.X * 3;
                        ballDirection.Y = ballDirection.Y * Angle.Y * 3;
                    }
                    else ballDirection.X *= -1;
                    ballSpeed *= acceleration;
                }
            }
        }

        public void ballMissed()
        {
            if (ballX < 0)
            {
                blueLives.takeLive();
                startBall();
            }

            if (ballX > graphics.PreferredBackBufferWidth)
            {
                redLives.takeLive();
                startBall();
            }
        }
        public void GameRestart()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                startBall();
                blueLives.resetLives();
                redLives.resetLives();
            }
        }
    }
    class Lives {
        int lives = 3;
        Texture2D lifeSprite;
        Vector2 livesPosition = new Vector2(20, 20);
        public Lives(ContentManager Content, int livesPositionX) {
            lifeSprite = Content.Load<Texture2D>("ball");
            livesPosition.X = livesPositionX;
        }
        
        public void takeLive()
        {
            lives--;
        }
        public void resetLives()
        {
            lives = 3;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for(int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(lifeSprite, livesPosition + new Vector2(i * 20, 0), Color.White);
            }
            //switch (lives)
            //{
            //    case 1:
            //        spriteBatch.Draw(lifeSprite, life1Position, Color.White);
            //        break;
            //    default:
            //        break;
            //}
            
        }
    }
}