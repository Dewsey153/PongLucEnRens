using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SharpDX;
using System;
using System.CodeDom;
using System.Net;
using System.Reflection.Metadata;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
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
        const float Controls2PositionX = 520f; // x coordinate of controls player two starting screen
        Texture2D ball; // Sprite ball
        Texture2D blue; // Sprite player one (blue paddle)
        Texture2D red; // Sprite player two (red paddle)
        Vector2 ballPosition; // Updated vector for current position of ball
        Vector2 bluePosition; // Updated vector for current position of player one (blue)
        Vector2 redPosition; // Updated vector for current position of player two (red)
        Vector2 ballDirection; // Normalized updated vector which points to direction the ball is going in 
        Vector2 oldMiddleBall; // Vector which tells where the ball was in the last frame. This is compared with current position to determine path of ball when it goes fast
        Vector2 ballDifference; // Vector made by comparing current en old middle ball, to determine path between frames
        int ballVertical; // Holds vertical component of vector ball at random start
        float currentBallX; // Holds current X coordinate of ball to copy to oldBallX
        float currentBallY; // Holds current Y coordinate of ball to copy to oldBallY
        float oldBallX; // Holds X coordinate of ball from previous frame
        float oldBallY; // Holds Y coordinate of ball from previous frame
        float ballSpeed; // Holds current speed of ball
        const float initialballSpeed = 4f; // Constant variable for speed in which ball starts
        const float acceleration = 1.1f; // Constand variable for the acceleration of the ball when it hits a paddle
        int ballSpeedXRandom = 3; // Is later changed into a random int for the X component of the ballDirection at the start
        int playerSpeed = 10; // For the speed in which players move their paddles
        const float minBallDirectionX = 0.3f; // minimal value of the x component of ballDirection, to make sure the ball doesn't take too long to get to the other side
        const float minBallDirectionY = 0.2f; // minimal value of the y component of ballDirection, to make sure the ball doesn't go to the other side too directly
        Random random = new Random();
        Lives redLives;
        Lives blueLives;
        enum GameState { Start, Playing, Win }; // Enumerated type indicates the different gamestates possible
        GameState currentState; // Contains the word for the current gamestate
        enum Winner { Red, Blue, None }; // Enumeratef type tells if there is a winner and who that is
        Winner winner; // Contains name of winner (red, blue or none)
        bool PreviousBlueHit = false; // boolean to make sure red and blue take turns in hitting the ball
        bool PreviousRedHit = false; // boolean to make sure red and blue take turns in hitting the ball
        Rectangle currentBallRectangle; // Rectangle around the ball
        Rectangle blueRectangle; // rectangle around player one
        Rectangle redRectangle; // rectangle around player two

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
            currentState = GameState.Start; // Game starts with gamestate 'Start'
            winner = Winner.None; // There is no winner when the game just started
            bluePosition = new Vector2(0, 200); // Start blue paddle at left side of screen at about the middle
            redPosition = new Vector2(graphics.PreferredBackBufferWidth - 16, 200); // Start red paddle at right side of screen at about the middle
            blueLives = new Lives(Content, 20); // Call class Lives for player one and set x coordinate so that all lives are visible on screen
            redLives = new Lives(Content, graphics.PreferredBackBufferWidth - 116); // Call class Lives for player two and set x coordinate so that all lives are visible on screen
            startBall(); // Set the ball to be ready behind the start screen
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load all fonts, sprites and sound effects in the correct variables
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
            {   // Play sound and set gamestate to 'playing' if any button other than R is pressed. R is an exception, because the sound played multiple times at the same time if R was continuously pressed at the start screen
                if (Keyboard.GetState().GetPressedKeys().Length > 0 && !Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    HitSound.Play(0.1f, 0, 0); // Play hitsound at 10% of original volume
                    currentState = GameState.Playing; // Change gamestate to Playing
                }
            }

            if (currentState == GameState.Playing)
            { // This is the code that is only active if the actual game is played
                if (Keyboard.GetState().IsKeyDown(Keys.W))
                { // Move player one up if W is pressed
                    bluePosition.Y = bluePosition.Y - playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.S))
                { // Move player one down if S is pressed
                    bluePosition.Y = bluePosition.Y + playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                { // Move player two up if the up key is pressed
                    redPosition.Y = redPosition.Y - playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                { // Move player two down if the down key is pressed
                    redPosition.Y = redPosition.Y + playerSpeed * gameTime.ElapsedGameTime.Milliseconds / 10;
                }

                // Copy the previous gotten position of the ball to oldBall variables and get new position ball. This way, the positions can be compared
                oldBallX = currentBallX;
                oldBallY = currentBallY;

                currentBallX = ballPosition.X;
                currentBallY = ballPosition.Y;

                //Make sure the players can not move beyond the screen
                if (redPosition.Y <= 0) redPosition.Y = 0;
                if (bluePosition.Y <= 0) bluePosition.Y = 0;
                if (redPosition.Y >= graphics.PreferredBackBufferHeight - red.Height) redPosition.Y = graphics.PreferredBackBufferHeight - red.Height;
                if (bluePosition.Y >= graphics.PreferredBackBufferHeight - blue.Height) bluePosition.Y = graphics.PreferredBackBufferHeight - blue.Height;

                checkBallHitPaddle(); // Checks if the ball hits one of the paddles
                ballMissed(); // Checks if the ball is missed and resets rally
                // Bounce the ball from the top and bottom of the screen
                if (ballPosition.Y >= graphics.PreferredBackBufferHeight - ball.Height || ballPosition.Y <= 0) ballDirection.Y = -1 * ballDirection.Y;
                ballDirection.Normalize(); // Normalize vector direction ball to make sure it is always between one and zero and can be checked
                if (ballDirection.X < minBallDirectionX && ballDirection.X > 0) ballDirection.X = minBallDirectionX; // X component ballDirection must be higher than a certain value to make sure the ball doesn't move too slow to the other side
                if (ballDirection.X > -minBallDirectionX && ballDirection.X < 0) ballDirection.X = -minBallDirectionX;
                if (ballDirection.Y < minBallDirectionY && ballDirection.Y > 0) ballDirection.Y = minBallDirectionY; // Y component ballDirection must be higher than a certain value to make sure the ball doesn't move in an almost straight line for eternity
                if (ballDirection.Y > -minBallDirectionY && ballDirection.Y < 0) ballDirection.Y = -minBallDirectionY;

                ballDirection.Normalize(); // Normalize vector ballDirection a second time to make sure it does not affect speed op ball
                ballPosition = Vector2.Add(ballPosition, ballSpeed * ballDirection * gameTime.ElapsedGameTime.Milliseconds / 10); // add velocity to the position of the ball
                
                GameOver(); // Check if a player has zero lives and act on it
            }

            GameRestart(); // Always check if the R button is pressed for a restart of the game
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White); // Clear screen to be able to draw on it
            spriteBatch.Begin();
            if (currentState == GameState.Start)
            {
                // Add text at the starting screen with controls for both players and instuctions to start the game
                spriteBatch.DrawString(StartingTextCentral, "Press any button to begin!", new Vector2(120, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player One", new Vector2(Controls1PositionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Controls Player Two", new Vector2(Controls2PositionX, ControlsPositionY), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: W", new Vector2(Controls1PositionX, ControlsPositionY + 20), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Down: S", new Vector2(Controls1PositionX, ControlsPositionY + 40), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Up: Up arrow key", new Vector2(Controls2PositionX, ControlsPositionY + 20), Color.Black);
                spriteBatch.DrawString(StartingTextControls, "Down: Down arrow key", new Vector2(Controls2PositionX, ControlsPositionY + 40), Color.Black);
            }
            if (currentState == GameState.Playing)
            {
                // Draw two paddles, ball and the amount of lives left on the screen if the game is being played
                spriteBatch.Draw(ball, ballPosition, Color.White);
                spriteBatch.Draw(blue, bluePosition, Color.White);
                spriteBatch.Draw(red, redPosition, Color.White);
                blueLives.Draw(gameTime, spriteBatch);
                redLives.Draw(gameTime, spriteBatch);
            }
            
            if (currentState == GameState.Win)//Runs when Gamestate becomes 'Win'
            {
                if (winner == Winner.Blue) //Displays message below when Blue Player wins
                {
                    spriteBatch.DrawString(StartingTextCentral, "Player One won!", new Vector2(250, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }

                if (winner == Winner.Red) //Displays message below when Red Player wins
                {
                    spriteBatch.DrawString(StartingTextCentral, "Player Two won!", new Vector2(250, graphics.PreferredBackBufferHeight / 2 - 30), Color.Black);
                }
                //Displays restart message beneath other message
                spriteBatch.DrawString(StartingTextControls, "Press R to restart the game", new Vector2(280, graphics.PreferredBackBufferHeight / 2 + 20), Color.Black);
            }
            spriteBatch.End();
        }

        public void startBall() // Called when ball needs to take default position and speed
        {
            ballSpeed = initialballSpeed;
            ballPosition.X = graphics.PreferredBackBufferWidth / 2;
            ballPosition.Y = graphics.PreferredBackBufferHeight / 2;
            int ballVerticalTemp = random.Next(-2, 2); // Generates a temporary number which will determine whether a ball starts by going up or down.
            if (ballVerticalTemp == 0) // Vertical ball speed may not become zero
            {
                ballVerticalTemp = random.Next(-2, 2);
            }
            else ballVertical = ballVerticalTemp; //Vertical ballspeed given definitve number
            int ballSpeedXRandomTemp = random.Next(-4, 4); // Generates a temporary number to determine horizontal direction
            if (ballSpeedXRandomTemp == 0) // Horizontal direction can not be 0
            {
                ballSpeedXRandomTemp = random.Next(-4, 4);
            }
            else ballSpeedXRandom = ballSpeedXRandomTemp; // Horizontal direction given definitive number

            ballDirection = new Vector2(ballSpeedXRandom, ballVertical * (5 - Math.Abs(ballSpeedXRandom))); //Previous ball directions used to define balldirection vector
            PreviousBlueHit = false; //Booleans made false to allow the ball to bounce of any of the two paddles.
            PreviousRedHit = false;
        }

        public void checkBallHitPaddle() // This method checks if the ball has hit the paddle
        {
            oldMiddleBall = new Vector2(oldBallX, oldBallY); //The old position of the ball stored in vector
            currentBallRectangle = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ball.Width, ball.Height); //Ball given rectangle
            redRectangle = new Rectangle((int)redPosition.X, (int)redPosition.Y, red.Width, red.Height); //Red player given rectangle
            blueRectangle = new Rectangle((int)bluePosition.X, (int)bluePosition.Y, blue.Width, blue.Height); // Blue player given rectangle

            ballDifference = ballPosition - oldMiddleBall; // Calculates the difference between the old and new position of the ball and stores it in a variable
            for (int i = 0; i < 100; i++) //Loop that checks if ball hit paddle, checked 100 times per frame
            {
                oldMiddleBall += ballDifference * 0.01f; //for every instance of the loop a position between the old and new positions of the ball is chosen

                if ((redRectangle.Contains(oldMiddleBall) || redRectangle.Intersects(currentBallRectangle)) && !PreviousRedHit) //if the previously chosen position is in the rectangle of the paddle and the paddle has not been hit yet
                {
                    PreviousRedHit = true; //This paddle has been hit
                    PreviousBlueHit = false; //The other paddle has not been hit just now
                    if (redRectangle.Contains(oldMiddleBall)) //returns ball to the front of the paddle where it hit
                        ballPosition = oldMiddleBall;
                    ballHitPaddle(); //calls the method that changes the direction of the ball
                    break; // ends all instances of this loop
                }
                //the other if statement performs the same instructions for the other paddle
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
        void ballHitPaddle() // Called when ball has hit a paddle
        {

            if (PreviousRedHit) // if the ball hit the paddle
            {
                HitSound.Play(0.1f, 0, 1.0f); // play soundeffect for hitting paddle
                Vector2 Angle = new Vector2(redRectangle.X, currentBallRectangle.Y - redRectangle.Center.Y); // calculate the angle at which it must bounce back
                Angle.Normalize(); //Normalise the angle to not increase speed unnecesseraly 
                if (Angle.Y != 0 || Angle.X != 0) // if the angle calculated was not zero
                {
                    ballDirection.X = ballDirection.X * -Angle.X * 3;  // horizontal direction flipped according to angle
                    ballDirection.Y = ballDirection.Y * Angle.Y * 3;   // vertical direction changed according to angle
                }
                else
                    ballDirection.X *= -1; // Flips ball direction horizontally if previous statement failed
                ballSpeed *= acceleration; //Accelerates ball by previously defined amount
            }


            else if (PreviousBlueHit) //Performs same checks as previous statement for the other paddle
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

        private void GameOver() // Called to check if lives are zero
        {
            if (blueLives.GetLives <= 0) //If blue has zero lives
            {
                WinSoundEffect.Play(1.0f, 0, 0); // Play appropriate sound effect
                winner = Winner.Red; //Winner changed to blue
                currentState = GameState.Win; //Sets game state to win
            }

            if (redLives.GetLives <= 0)//Same if statement but for red player
            {
                WinSoundEffect.Play(1.0f, 0, 0);
                winner = Winner.Blue;
                currentState = GameState.Win;
            }
        }

        private void GameRestart() // Called every frame
        {

            if (Keyboard.GetState().IsKeyDown(Keys.R))//If the R key is pressed during gameplay
            {
                currentState = GameState.Start; // Gamestate returned to start
                startBall(); //start ball statement called
                blueLives.resetLives(); //reset lives statement called for blue player
                redLives.resetLives(); //reset lives statement called for red player
                winner = Winner.None; //Winner set to none
            }
        }


        private void ballMissed() //Called every frame
        {
            if (ballPosition.X < -20) // if ball is left of the screen
            {
                MissedSound.Play(0.2f, 0, 0); //correct sound is played
                blueLives.takeLive();//blue player loses one life(method explained later)
                startBall();//start ball method called
            }

            if (ballPosition.X > graphics.PreferredBackBufferWidth + 20)//same statement for other side of the screen and red player
            {
                MissedSound.Play(0.2f,0,0);
                redLives.takeLive();
                startBall();
            }
        }

    }
    class Lives //New class to manage lives
    {
        int lives = 5; // Lives variable defined and set as 5
        Texture2D lifeSprite; //Life texture defined
        Vector2 livesPosition = new Vector2(20, 20);//Lives position defined
        public Lives(ContentManager Content, int livesPositionX)//Loads textures and defines their position
        {
            lifeSprite = Content.Load<Texture2D>("ball");
            livesPosition.X = livesPositionX;
        }

        public int GetLives //returns number of lives
        {
            get { return lives; }
        }

        public void takeLive() //Removes one life when called
        {
            lives--;
        }
        public void resetLives() //Resets lives to given number
        {
            lives = 5;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)  // Draws lives until the loop stops
        {
            for (int i = 0; i < lives; i++) // loops for the amount of lives
            {
                spriteBatch.Draw(lifeSprite, livesPosition + new Vector2(i * 20, 0), Color.White); // draws lives next to each other
            }
        }
    }
}