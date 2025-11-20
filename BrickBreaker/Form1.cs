using System.ComponentModel;

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        public event EventHandler<int>? GameFinished;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool CloseOnGameOver { get; set; }
        public int LatestScore => score;

        // --- Window constants ---
        private int WindowWidth = 800;            // Width of the game window
        private int WindowHeight = 800;           // Height of the game window
        private Rectangle playAreaRect;                     // Rectangle defining play area for bricks, paddle, ball
        private const int PlayAreaMargin = 2;             // Margin of the play area from bricks (just padding)
        private const int PaddleAreaHeight = 400;         // Height of area below bricks for paddle/ball space

        // --- Ball constants ---
        private const int BallRadius = 7;                // Radius of the ball
        private bool ballReadyToShoot = true; // Indicates if ball is waiting on paddle to be shot


        // --- Paddle constants ---
        private int PaddleWidth = 100;                    // Width of the paddle
        private const int PaddleHeight = 20;              // Height of the paddle
        private const double PaddleSpeed = 13;            // Speed at which paddle moves
        private int originalPaddleWidth;                   // Store original paddle width for power-up resets

        // Paddle blinking effect variables
        private bool isPaddleBlinking = false;
        private int paddleBlinkCounter = 0;
        private Color normalPaddleColor = Color.FromArgb(36, 162, 255);
        private Color blinkPaddleColor = Color.OrangeRed;
        private int paddleExtenderTicksLeft = 0; // Number of ticks left for paddle extender effect

        // Paddle movement variables
        private double paddleX;                              // Current X position of the paddle (floating point for smooth movement)
        private int paddleY;                                 // Fixed Y position of paddle
        private bool leftPressed, rightPressed;             // Whether left or right keys are currently pressed

        // --- Brick constants ---
        private const int BrickRows = 7;                   // Number of brick rows
        private const int BrickCols = 10;                  // Number of brick columns
        private const int BrickWidth = 60;                 // Width of each brick
        private const int BrickHeight = 25;                // Height of each brick
        private const int BrickStartX = 60;                 // Starting X position of first brick
        private const int BrickStartY = 40;                 // Starting Y position of first brick
        private const int BrickXSpacing = 70;               // Horizontal spacing between bricks
        private const int BrickYSpacing = 30;               // Vertical spacing between bricks
        private double timeSinceColorChange = 0;            // Timer for brick color changes
        private double colorChangeInterval = 2;             // Interval in seconds for brick color changes

        // --- Game variables ---
        private int score = 0;                              // Current player score
        private int brickStreak = 0;                        // Count of bricks hit in current ball bounce streak
        private int scoreMultiplier = 1;                    // Score multiplier based on streak
        private bool isPaused = false;                      // Game pause status
        private bool isGameOver = false;                    // Game over state
        private bool gameFinishedRaised = false;            // Ensures GameFinished fires only once
        private double elapsedSeconds = 0;                  // Total elapsed time in seconds

        // --- Game state ---
        private System.Windows.Forms.Timer gameTimer;      // Timer controlling game update ticks
        private List<Ball> balls = new List<Ball>();       // List of balls in play (for multiball)
        private List<Brick> bricks;                         // List of bricks in the level
        private List<PowerUp> powerUps = new List<PowerUp>();// List of active powerups falling
        private List<ScorePopup> scorePopups = new List<ScorePopup>(); // List of score popup animations
        private Random rand = new Random();                 // Random number generator for colors/powerups
      


        // Constructor: Initialize game form and components
        public Form1()
        {
            InitializeComponent();

            // Calculate the rectangle defining the play area including bricks, paddle area, and margin
            playAreaRect = new Rectangle(
                BrickStartX - PlayAreaMargin,
                BrickStartY - PlayAreaMargin,
                (BrickCols - 1) * BrickXSpacing + BrickWidth + PlayAreaMargin * 2,
                (BrickRows - 1) * BrickYSpacing + BrickHeight + PaddleAreaHeight + PlayAreaMargin
            );

            // Setup form properties for size, border style, and double buffering for smooth graphics
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.ClientSize = new Size(WindowWidth, WindowHeight);
            this.DoubleBuffered = true;

            // Setup paddle position near bottom of play area with a margin
            int paddleBottomMargin = 10;
            paddleY = playAreaRect.Bottom - PaddleHeight - paddleBottomMargin;
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;
            PaddleWidth = 100;              // Initial paddle width
            originalPaddleWidth = PaddleWidth;


            // Setup initial ball just above the paddle, values for velocity and radius
            balls.Clear();
            balls.Add(new Ball(
                x: (int)(paddleX + PaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 6, vy: 6,
                radius: BallRadius
            ));

            // Initialize bricks list and create bricks with colors randomized
            bricks = new List<Brick>();
            for (int row = 0; row < BrickRows; row++)
            {
                for (int col = 0; col < BrickCols; col++)
                {
                    bricks.Add(new Brick
                    {
                        X = BrickStartX + col * BrickXSpacing,
                        Y = BrickStartY + row * BrickYSpacing,
                        Width = BrickWidth,
                        Height = BrickHeight,
                        IsVisible = true,
                        BrickColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256))
                    });
                }
            }

            // Setup the timer for game updates at approx 60 FPS (tick every ~16ms)
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // Attach paint and keyboard event handlers for rendering and input
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        // Paint event handler to render the game elements each frame
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);  // Clear background to black

            // Fonts used for displaying score, multiplier, and game state messages
            var fontScore = new Font("Arial", 18, FontStyle.Bold);
            var fontMultiplier = new Font("Arial", 16, FontStyle.Bold);
            var fontGameOver = new Font("Arial", 20, FontStyle.Bold);

            // Format elapsedSeconds as minutes and seconds and draw time, score, multiplier labels
            int minutes = (int)elapsedSeconds / 60, seconds = (int)elapsedSeconds % 60;
            g.DrawString($"Time: {minutes:D2}:{seconds:D2}", fontScore, Brushes.White, playAreaRect.Left + 420, playAreaRect.Top - 40);
            g.DrawString($"Score: {score}", fontScore, Brushes.Yellow, playAreaRect.Left, playAreaRect.Top - 40);
            g.DrawString($"Multiplier: x{scoreMultiplier}", fontMultiplier, Brushes.Orange, playAreaRect.Left + 180, playAreaRect.Top - 40);

            // Draw play area background with a dark color and white border
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(22, 22, 40)))
                g.FillRectangle(bgBrush, playAreaRect);
            using (Pen borderPen = new Pen(Color.White, 4))
                g.DrawRectangle(borderPen, playAreaRect);

            // Draw bricks, paddle, and balls using helper methods
            DrawBricks(g);
            DrawPaddle(g);
            DrawBalls(g);

            // Draw any active power ups
            foreach (var p in powerUps)
                p.Draw(e.Graphics);

            // If game is over, display game over message centered on screen
            if (isGameOver)
            {
                string overText = "Game Over! Press SPACE to restart";
                SizeF sz = g.MeasureString(overText, fontGameOver);
                float cx = (ClientSize.Width - sz.Width) / 2, cy = (ClientSize.Height - sz.Height) / 2;
                g.DrawString(overText, fontGameOver, Brushes.Red, cx, cy);
            }
            // If paused (and not game over), display pause message
            if (ballReadyToShoot && !isGameOver && !isPaused)
            {
                string launchText = "Press UP ARROW to launch the ball";
                var fontLaunch = new Font("Arial", 16, FontStyle.Bold);
                SizeF textSize = g.MeasureString(launchText, fontLaunch);
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = paddleY - 80; // position above the paddle

                using (Brush brush = new SolidBrush(Color.White))
                {
                    g.DrawString(launchText, fontLaunch, brush, x, y);
                }
            }
            foreach (var popup in scorePopups)
            {
                popup.Draw(g);
            }



        }

        // Activates the effect of a collected power-up based on type
        private void ActivatePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Multiball:
                    if (balls.Count > 0)
                    {
                        Ball mainBall = balls[0];
                        // Add two extra balls with slightly different velocities for multiball effect
                        balls.Add(new Ball(mainBall.X, mainBall.Y, 6, -6, mainBall.Radius));
                        balls.Add(new Ball(mainBall.X, mainBall.Y, -6, -6, mainBall.Radius));
                    }
                    break;
                case PowerUpType.PaddleExtender:
                    PaddleWidth += 50; // Increase paddle width
                    paddleExtenderTicksLeft = 312; // 5000ms/16ms ≈ 312 ticks (for 5 seconds)
                    isPaddleBlinking = false;
                    break;
            }
        }

        // Helper method to draw all bricks in the game
        private void DrawBricks(Graphics g)
        {
            foreach (var brick in bricks)
            {
                if (brick.IsVisible)
                {
                    using (SolidBrush bBrush = new SolidBrush(brick.BrickColor))
                    using (Pen bPen = new Pen(Color.DarkGray, 1))
                    {
                        var r = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);
                        g.FillRectangle(bBrush, r);    // Fill brick rectangle with color
                        g.DrawRectangle(bPen, r);      // Draw border around brick
                    }
                }
            }
        }

        // Draw the paddle graphics at current position
        private void DrawPaddle(Graphics g)
        {
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight); // Paddle rectangle

            Color drawColor = normalPaddleColor; // Default paddle color
            if (isPaddleBlinking && (paddleBlinkCounter / 8) % 2 == 0) // Blink every 8 ticks
                drawColor = blinkPaddleColor; // Alternate color for blinking effect

            using (var paddleBrush = new SolidBrush(drawColor)) // Create brush with determined color
            using (var paddlePen = new Pen(Color.Blue, 2)) // Pen for paddle border
            {
                g.FillRectangle(paddleBrush, paddleRect); // Draw filled paddle
                g.DrawRectangle(paddlePen, paddleRect); // Draw paddle border
            }
        }


        // Draw all balls currently in play
        private void DrawBalls(Graphics g)
        {
            foreach (var ball in balls)
            {
                Rectangle ballRect = new Rectangle(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
                using (Brush ballBrush = new SolidBrush(Color.Red))
                using (Pen ballPen = new Pen(Color.White, 2))
                {
                    g.FillEllipse(ballBrush, ballRect);  // Draw filled circle
                    g.DrawEllipse(ballPen, ballRect);    // Draw border ellipse
                }
            }
        }

        // Main game logic executed on each timer tick (~60 times per second)
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isGameOver && !isPaused)
            {
                elapsedSeconds += gameTimer.Interval / 1000.0;
                timeSinceColorChange += gameTimer.Interval / 1000.0;
            }
            else
            {
                // If game over or paused, we typically don't want to run physics
                if (isPaused) Invalidate();
                return;
            }

            // --- 1. UPDATE SCORE POPUPS ---
            for (int i = scorePopups.Count - 1; i >= 0; i--)
            {
                scorePopups[i].Update();
                if (!scorePopups[i].IsAlive)
                {
                    scorePopups.RemoveAt(i);
                }
            }

            // --- 2. HANDLE BRICK COLORS ---
            if (timeSinceColorChange >= colorChangeInterval)
            {
                foreach (var brick in bricks)
                {
                    if (brick.IsVisible)
                        brick.BrickColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
                }
                timeSinceColorChange = 0;
            }

            // --- 3. MOVE PADDLE ---
            if (leftPressed && paddleX > playAreaRect.Left)
                paddleX -= PaddleSpeed;
            if (rightPressed && paddleX < playAreaRect.Right - PaddleWidth)
                paddleX += PaddleSpeed;


            // --- 4. UPDATE POWERUPS (Optimized Reverse Loop) ---
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = powerUps[i];
                powerUp.UpdatePosition();

                // Remove if off screen
                if (powerUp.Y > playAreaRect.Bottom)
                {
                    powerUps.RemoveAt(i);
                    continue;
                }

                // Check collision
                Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight);
                Rectangle powerUpRect = new Rectangle(powerUp.X, powerUp.Y, powerUp.Width, powerUp.Height);

                if (paddleRect.IntersectsWith(powerUpRect))
                {
                    ActivatePowerUp(powerUp.Type);
                    powerUps.RemoveAt(i);
                }
            }

            // --- 5. UPDATE BALLS (Optimized Reverse Loop) ---
            // We use a for-loop here so we can remove balls safely without .ToList()
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                var ball = balls[i];

                if (ballReadyToShoot)
                {
                    ball.X = (int)(paddleX + PaddleWidth / 2 - BallRadius);
                    ball.Y = paddleY - 50;
                }
                else
                {
                    ball.UpdatePosition();
                }

                // Ball Death
                if (ball.Y + ball.Radius * 2 > playAreaRect.Bottom)
                {
                    balls.RemoveAt(i);
                    if (balls.Count == 0)
                    {
                        TriggerGameOver();
                        return; // Exit immediately on game over
                    }
                    continue;
                }

                // Brick Collisions
                bool hitBrick = false;
                foreach (var brick in bricks)
                {
                    if (brick.IsVisible && BallHitsRect(ball, brick))
                    {
                        brick.IsVisible = false;
                        ball.InvertVerticalVelocity();
                        brickStreak++;
                        scoreMultiplier = Math.Max(1, brickStreak);
                        score += 10 * scoreMultiplier;

                        // Add visual flare
                        scorePopups.Add(new ScorePopup(brick.X + brick.Width / 2, brick.Y + brick.Height / 2, 10 * scoreMultiplier));

                        // Chance for PowerUp
                        if (rand.NextDouble() < 0.2)
                        {
                            var powerUpType = (PowerUpType)(rand.Next(0, 2));
                            powerUps.Add(new PowerUp(brick.X + brick.Width / 2, brick.Y + brick.Height / 2, powerUpType));
                        }
                        hitBrick = true;
                        break;
                    }
                }

                // Wall Collisions (Only check if we didn't just hit a brick to prevent sticking)
                if (!hitBrick)
                {
                    if (ball.X <= playAreaRect.Left)
                    {
                        ball.InvertHorizontalVelocity();
                        ball.SetPosition(playAreaRect.Left, ball.Y);
                        brickStreak = 0; scoreMultiplier = 1;
                    }
                    if (ball.X + ball.Radius * 2 >= playAreaRect.Right)
                    {
                        ball.InvertHorizontalVelocity();
                        ball.SetPosition(playAreaRect.Right - ball.Radius * 2, ball.Y);
                        brickStreak = 0; scoreMultiplier = 1;
                    }
                    if (ball.Y <= playAreaRect.Top)
                    {
                        ball.InvertVerticalVelocity();
                        ball.SetPosition(ball.X, playAreaRect.Top);
                        brickStreak = 0; scoreMultiplier = 1;
                    }
                }

                // Paddle Collision
                if (BallHitsRect(ball, new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight)))
                {
                    double ballCenter = ball.X + ball.Radius;
                    double paddleCenter = paddleX + PaddleWidth / 2.0;
                    double hitPos = (ballCenter - paddleCenter) / (PaddleWidth / 2.0);

                    // Physics math
                    double BallSpeed = 7.0;
                    double maxHorizontal = BallSpeed * 0.8;
                    double vx = hitPos * maxHorizontal;
                    double vy = -Math.Sqrt(BallSpeed * BallSpeed - vx * vx);

                    ball.VX = vx;
                    ball.VY = vy;

                    brickStreak = 0;
                    scoreMultiplier = 1;
                }
            }

            // --- 6. UPDATE PADDLE EFFECTS ---
            if (paddleExtenderTicksLeft > 0)
            {
                paddleExtenderTicksLeft--;

                // Handle Blinking
                if (paddleExtenderTicksLeft < 62)
                {
                    isPaddleBlinking = true;
                    paddleBlinkCounter++;
                }
                else
                {
                    isPaddleBlinking = false;
                    paddleBlinkCounter = 0;
                }

                // Handle Expiration
                if (paddleExtenderTicksLeft == 0)
                {
                    PaddleWidth = originalPaddleWidth;
                    isPaddleBlinking = false;
                    paddleBlinkCounter = 0;
                }
            }

            // Redraw screen
            Invalidate();
        }

        // Helper method to check if a ball intersects a rectangle (overlap test)
        private bool BallHitsRect(Ball ball, Rectangle rect)
        {
            if (ball == null) return false; // Safety check in case ball is null
            return
                ball.X + ball.Radius * 2 >= rect.X &&
                ball.X <= rect.X + rect.Width &&
                ball.Y + ball.Radius * 2 >= rect.Y &&
                ball.Y <= rect.Y + rect.Height;
        }

        // Overload for Ball and Brick collision check by converting brick to rectangle
        private bool BallHitsRect(Ball ball, Brick b)
        {
            return BallHitsRect(
                ball,
                new Rectangle(b.X, b.Y, b.Width, b.Height)
            );
        }

        // Keyboard handler for key press events
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Mark left or right pressed flags
            if (e.KeyCode == Keys.Left) leftPressed = true;
            if (e.KeyCode == Keys.Right) rightPressed = true;

            // If game over and spacebar pressed, restart game
            if (isGameOver && e.KeyCode == Keys.Space)
            {

                RestartGame();
                ballReadyToShoot = true; // Ensure ball waiting after restart

            }

            // Toggle pause state with 'P' key
            if (e.KeyCode == Keys.P)
            {
                isPaused = !isPaused;     // Flip pause status
                Invalidate();             // Redraw to show pause message
            }
            if (e.KeyCode == Keys.Up && ballReadyToShoot)
            {
                // Give the ball an initial shooting velocity
                var mainBall = balls.FirstOrDefault();
                if (mainBall != null)
                {
                    mainBall.VX = 0;
                    mainBall.VY = -7; // direction upwards
                    ballReadyToShoot = false; // ball is now in motion
                }
            }

        }

        // Keyboard handler for key release events
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // Unset flags on key release
            if (e.KeyCode == Keys.Left) leftPressed = false;
            if (e.KeyCode == Keys.Right) rightPressed = false;
        }

        private void TriggerGameOver()
        {
            if (isGameOver)
            {
                return;
            }

            isGameOver = true;
            gameTimer?.Stop();
            RaiseGameFinished();
            Invalidate();
        }
        private void RestartGame()
        {
            // 1. Reset Game State
            score = 0;
            brickStreak = 0;
            scoreMultiplier = 1;
            elapsedSeconds = 0;
            isGameOver = false;
            gameFinishedRaised = false;
            gameTimer.Start(); // Important: Start the timer again!

            // 2. Reset Bricks
            foreach (var brick in bricks)
            {
                brick.IsVisible = true;
                // Optional: Reshuffle colors on restart
                brick.BrickColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            }

            // 3. Clear old objects
            powerUps.Clear();
            scorePopups.Clear();
            balls.Clear();

            // 4. Reset Ball and Paddle
            PaddleWidth = originalPaddleWidth; // Reset paddle size
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0; // Center paddle

            // Add the starting ball
            balls.Add(new Ball(
                x: (int)(paddleX + PaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 0, vy: 0,
                radius: BallRadius
            ));

            ballReadyToShoot = true;
            Invalidate();
        }

        private void RaiseGameFinished()
        {
            if (gameFinishedRaised)
            {
                return;
            }

            gameFinishedRaised = true;
            GameFinished?.Invoke(this, score);

        }
    }
}
