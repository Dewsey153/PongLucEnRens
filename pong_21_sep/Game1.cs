using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using System;
using System.Net;
using System.Reflection.Metadata;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont StartingTextCentral; // Font type for text in center starting and end screen
        SpriteFont StartingTextControls; // Font type for controls in start screen en bottom text in end screen
        SoundEffect WinSoundEffect; // Sound effect for if one player wins
        SoundEffect HitSound; // Sound effect for start game and if ball hit paddle
        SoundEffect MissedSound; // Sound effect if ball missed by paddle
        const float ControlsPositionY = 20f; // Height of controls in starting screen
        const float Controls1PositionX = 20f; // x coordinate of controls player one starting screen
        const float Controls2PositionX = 560f; // x coordinate of controls player two starting screen
        Texture2D ball; // Sprite ball
        Texture2D blue; // Sprite player one (blue paddle)
        Texture2D red; // Sprite player two (red paddle)
        Vector2 ballPosition; // Updated vector for current position of ball
        Vector2 bluePosition; // Updated vector for current position of player one (blue)
        Vector2 redPosition; // Updated vector for current position of player two (red)
        Vector2 ballDirection; // Normalized updated vector which points to direction the ball is going in 
        Vector2 oldMiddleBall; // Vector which tells where the ball was in the last frame. This is compared with current position to determine path of ball when it goes fast
        Vector2 ballDifference; // Vector made by comparing current en old middle ball, to determine path between frames
        int blueY = 100;
        int redY = 100;
        int ballVertical;
        float currentBallX = 10f;
        float currentBallY = 10f;
        float oldBallX = 10f;
        float oldBallY = 10f;
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
        enum GameState { Start, Playing, Win };
        GameState currentState;
        enum Winner { Red, Blue, None };
        Winner winner;
        bool PreviousBlueHit = false;
        bool PreviousRedHit = false;
        Rectangle oldBallRectangle;
        Rectangle currentBallRectangle;
        Rectangle redRectangle;
        Rectangle blueRectangle;

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
            bluePosition.X = 0;
            bluePosition.Y = graphics.PreferredBackBufferHeight/2;
            startBall();
            redLives = new Lives(Content, graphics.PreferredBackBufferWidth - 116);
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
            WinSoundEffect = Content.Load<SoundEffect>("WinSoundEffect");
            HitSound = Content.Load<SoundEffect>("HitSoundEffect");
            MissedSound = Content.Load<SoundEffect>("MissedSound");
        }

        protected override void Update(GameTime gameTime)
        {
            if (currentState == GameState.Start)
            {
                if (Keyboard.GetState().GetPressedKeys().Length > 0 && !Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    HitSound.Play(0.1f, 0, 0);
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

                oldBallX = currentBallX;
                oldBallY = currentBallY;

                currentBallX = ballPosition.X;
                currentBallY = ballPosition.Y;

                if (redY <= 0) redY = 0;
                if (blueY <= 0) blueY = 0;
                if (redY >= graphics.PreferredBackBufferHeight - red.Height) redY = graphics.PreferredBackBufferHeight - red.Height;
                if (blueY >= graphics.PreferredBackBufferHeight - blue.Height) blueY = graphics.PreferredBackBufferHeight - blue.Height;

                checkBallHitPaddle();
                currentBallX = ballPosition.X;
                currentBallY = ballPosition.Y;
                ballMissed();
                //bounce from walls
                if (ballPosition.Y >= graphics.PreferredBackBufferHeight - ball.Height || ballPosition.Y <= 0) ballDirection.Y = -1 * ballDirection.Y;
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
                spriteBatch.DrawString(StartingTextCentral, "Start the game by pressing any button", new Vector2(20, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player One", new Vector2(Controls1PositionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player Two", new Vector2(Controls2PositionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: W", new Vector2(Controls1PositionX, ControlsPositionY + 20), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Down: S", new Vector2(Controls1PositionX, ControlsPositionY + 35), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: Up arrow key", new Vector2(Controls2PositionX, ControlsPositionY + 20), Color.Black);
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

            // Rens tot hier

            if (currentState == GameState.Win)
            {
                if (winner == Winner.Blue)
                {
                    spriteBatch.DrawString(StartingTextCentral, "Player One won!", new Vector2(250, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }

                if (winner == Winner.Red)
                {
                    spriteBatch.DrawString(StartingTextCentral, "Player Two won!", new Vector2(250, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }

                spriteBatch.DrawString(StartingTextControls, "Press R to restart the game", new Vector2(280, graphics.PreferredBackBufferHeight / 2 + 20), Color.Black);
            }
            spriteBatch.End();
        }

        public void startBall()
        {
            ballSpeed = initialballSpeed;
            ballPosition.X = graphics.PreferredBackBufferWidth / 2;
            ballPosition.Y = graphics.PreferredBackBufferHeight / 2;
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

            ballDirection = new Vector2(ballSpeedXRandom, ballVertical * (5 - Math.Abs(ballSpeedXRandom)));
            PreviousBlueHit = false;
            PreviousRedHit = false;
        }

        public void checkBallHitPaddle()
        {
            oldBallRectangle = currentBallRectangle;
            oldMiddleBall = new Vector2(oldBallX, oldBallY);
            currentBallRectangle = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ball.Width, ball.Height);
            redRectangle = new Rectangle((int)redPosition.X, (int)redPosition.Y, red.Width, red.Height);
            blueRectangle = new Rectangle((int)bluePosition.X, (int)bluePosition.Y, blue.Width, blue.Height);

            ballDifference = ballPosition - oldMiddleBall;
            for (int i = 0; i < 100; i++)
            {
                oldMiddleBall += ballDifference * 0.01f;

                if ((redRectangle.Contains(oldMiddleBall) || redRectangle.Intersects(currentBallRectangle)) && !PreviousRedHit)
                {
                    PreviousRedHit = true;
                    PreviousBlueHit = false;
                    if (redRectangle.Contains(oldMiddleBall))
                        ballPosition = oldMiddleBall;
                    ballHitPaddle();
                    break;
                }

                if ((blueRectangle.Contains(oldMiddleBall) || blueRectangle.Intersects(currentBallRectangle)) && !PreviousBlueHit)
                {
                    PreviousRedHit = false;
                    PreviousBlueHit = true;
                    if (blueRectangle.Contains(oldMiddleBall))
                        ballPosition = oldMiddleBall;
                    ballHitPaddle();
                    break;
                }
            }
        }
        void ballHitPaddle()
        {

            if (PreviousRedHit)
            {
                HitSound.Play(0.1f, 0, 1.0f);
                Vector2 Angle = new Vector2(redRectangle.X, currentBallRectangle.Y - redRectangle.Center.Y);
                Angle.Normalize();
                if (Angle.Y != 0 || Angle.X != 0)
                {
                    ballDirection.X = ballDirection.X * -Angle.X * 3;
                    ballDirection.Y = ballDirection.Y * Angle.Y * 3;
                }
                else
                    ballDirection.X *= -1;
                ballSpeed *= acceleration;
            }


            else if (PreviousBlueHit)
            {
                HitSound.Play(0.1f, 0, -1.0f);
                Vector2 Angle = new Vector2(blueRectangle.Width, currentBallRectangle.Y - blueRectangle.Center.Y);
                Angle.Normalize();
                if (Angle.Y != 0 || Angle.X != 0)
                {
                    ballDirection.X = ballDirection.X * -Angle.X * 3;
                    ballDirection.Y = ballDirection.Y * Angle.Y * 3;
                }
                else
                    ballDirection.X *= -1;
                ballSpeed *= acceleration;
            }
        }

        private void GameOver()
        {
            if (blueLives.GetLives <= 0)
            {
                WinSoundEffect.Play(1.0f, 0, 0);
                winner = Winner.Red;
                currentState = GameState.Win;
            }

            if (redLives.GetLives <= 0)
            {
                WinSoundEffect.Play(1.0f, 0, 0);
                winner = Winner.Blue;
                currentState = GameState.Win;
            }
        }

        private void GameRestart()
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


        private void ballMissed()
        {
            if (ballPosition.X < -20)
            {
                MissedSound.Play(0.2f, 0, 0);
                blueLives.takeLive();
                startBall();
            }

            if (ballPosition.X > graphics.PreferredBackBufferWidth + 20)
            {
                MissedSound.Play(0.2f,0,0);
                redLives.takeLive();
                startBall();
            }
        }

    }
    class Lives
    {
        int lives = 5;
        Texture2D lifeSprite;
        Vector2 livesPosition = new Vector2(20, 20);
        public Lives(ContentManager Content, int livesPositionX)
        {
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
            lives = 5;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(lifeSprite, livesPosition + new Vector2(i * 20, 0), Color.White);
            }
        }
    }
}