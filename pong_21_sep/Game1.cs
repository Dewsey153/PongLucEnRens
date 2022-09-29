﻿using Microsoft.Xna.Framework;
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
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace pong
{
    public class pong : Game
    {
        KeyboardState currentKBState = Keyboard.GetState();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Rectangle rectangle;
        SpriteFont StartingTextCentral;
        SpriteFont StartingTextControls;
        float ControlsPositionY = 20f;
        float Controls1PostionX = 20f;
        float Controls2PositionX = 620f;
        Texture2D ball;
        Texture2D blue;
        Texture2D red;
        Vector2 ballPosition;
        Vector2 bluePosition;
        Vector2 redPosition;
        Vector2 ballDirection;
        int blueY = 100;
        int redY = 100;
        int ballVertical;
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
        enum GameState{Start, Playing, Win};
        GameState currentState;
        enum Winner { Red, Blue, None};
        Winner winner;



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
            currentState = GameState.Start;
            winner = Winner.None;
            startBall();
            redLives = new Lives(Content, graphics.PreferredBackBufferWidth - 76);
            blueLives = new Lives(Content, 20);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            StartingTextCentral = Content.Load<SpriteFont>("StartingTextCentral");
            StartingTextControls = Content.Load<SpriteFont>("StartingTextControls");
            ball = Content.Load<Texture2D>("ball");
            blue = Content.Load<Texture2D>("bluePlayer");
            red = Content.Load<Texture2D>("redPlayer");
        }

        protected override void Update(GameTime gameTime)
        {
            if(currentState == GameState.Start)
            {
                if (Keyboard.GetState().GetPressedKeys().Length > 0)
                {
                    currentState = GameState.Playing;
                }
            }

            if (currentState == GameState.Playing)
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
                ballPosition = Vector2.Add(ballPosition, ballSpeed * ballDirection * gameTime.ElapsedGameTime.Milliseconds / 10);
                bluePosition = new Vector2(0, blueY);
                redPosition = new Vector2(graphics.PreferredBackBufferWidth - blue.Width, redY);
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    startBall();
                }
                GameOver();
            }

            GameRestart();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            spriteBatch.Begin();
            if (currentState == GameState.Start)
            {
                spriteBatch.DrawString(StartingTextCentral, "Start the game by pressing any button", new Vector2(20, graphics.PreferredBackBufferHeight/2-30), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player One", new Vector2(Controls1PostionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player Two", new Vector2(Controls2PositionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: W", new Vector2(Controls1PostionX, ControlsPositionY+20), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Down: S", new Vector2(Controls1PostionX, ControlsPositionY+35), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: Up arrow key", new Vector2(Controls2PositionX, ControlsPositionY+20), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Down: Down arrow key", new Vector2(Controls2PositionX, ControlsPositionY + 35), Color.Black);
            }
            if (currentState == GameState.Playing)
            {
                spriteBatch.Draw(ball, ballPosition, Color.White);
                spriteBatch.Draw(blue, bluePosition, Color.White);
                spriteBatch.Draw(red, redPosition, Color.White);
                blueLives.Draw(gameTime, spriteBatch);
                redLives.Draw(gameTime, spriteBatch);
            }
            if(currentState == GameState.Win)
            {
                if(winner == Winner.Blue)
                {
                    spriteBatch.DrawString(StartingTextCentral, "Blue won!", new Vector2(300, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }

                if (winner == Winner.Red)
                {
                    spriteBatch.DrawString(StartingTextCentral, "Red won!", new Vector2(300, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }

                spriteBatch.DrawString(StartingTextControls, "Press R to restart the game", new Vector2(280, graphics.PreferredBackBufferHeight/2+20), Color.Black);
            }
            spriteBatch.End();
        }
        
        public void startBall()
        {
            ballSpeed = initialballSpeed;
            ballX = graphics.PreferredBackBufferWidth / 2;
            ballY = graphics.PreferredBackBufferHeight / 2;
            int ballVerticalTemp = random.Next(-2, 2);
            if (ballVerticalTemp == 0)
            {
                ballVerticalTemp = random.Next(-2, 2);
            }
            else ballVertical = ballVerticalTemp;
            int ballSpeedXRandomTemp = random.Next(-4, 4);
            if (ballSpeedXRandomTemp == 0)
            {
                ballSpeedXRandomTemp = random.Next(-4, 4);
            }
            else ballSpeedXRandom = ballSpeedXRandomTemp;

            ballDirection = new Vector2(ballSpeedXRandom, ballVertical*(5 -Math.Abs(ballSpeedXRandom)));
            ballPosition = new Vector2(ballX, ballY);
        }

        public void ballHitPaddle()
        {
            Rectangle ballRectangle = new Rectangle((int)ballX, (int)ballY, ball.Width, ball.Height);
            Rectangle redRectangle = new Rectangle((int)redPosition.X, (int)redPosition.Y, red.Width, red.Height);
            Rectangle blueRectangle = new Rectangle((int)bluePosition.X, (int)bluePosition.Y, blue.Width, blue.Height);

            if (ballRectangle.Intersects(redRectangle))
            {
                Vector2 Angle = new Vector2(redRectangle.X, ballRectangle.Y - redRectangle.Center.Y);
                Angle.Normalize();
                if (Angle.Y != 0 || Angle.X != 0)
                {
                    ballDirection.X = ballDirection.X * -Angle.X * 3;
                    ballDirection.Y = ballDirection.Y * Angle.Y * 3;
                }
                else ballDirection.X *= -1;
                ballSpeed *= acceleration;
            }

            else if (ballRectangle.Intersects(blueRectangle))
            {
                Vector2 Angle = new Vector2(blueRectangle.Width, ballRectangle.Y - blueRectangle.Center.Y);
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

        public void GameOver()
        {
            if(blueLives.GetLives <= 0)
            {
                winner = Winner.Red;
                currentState = GameState.Win;
            }

            if(redLives.GetLives <= 0)
            {
                winner = Winner.Blue;
                currentState = GameState.Win;
            }
        }

        public void GameRestart()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                currentState = GameState.Start;
                startBall();
                blueLives.resetLives();
                redLives.resetLives();
                winner = Winner.None;
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

        public int GetLives
        {
            get { return lives; }
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
        }
    }
}