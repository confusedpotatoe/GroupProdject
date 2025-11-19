

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        public event EventHandler<int>? GameFinished;
        public bool CloseOnGameOver { get; set; }
        public int LatestScore => score;

        // --- Game constants ---
        private const int WindowWidth = 800; 
        private const int WindowHeight = 800;
        private const int BallRadius = 7;
        private int PaddleWidth = 100;
        private const int PaddleHeight = 20;
        private const double PaddleSpeed = 13;
        private const int PlayAreaMargin = 2;     // Pixels padding on all sides, optional
        private const int PaddleAreaHeight = 400;   // Space below bricks for paddle and ball
        private const int BrickRows = 7;
        private const int BrickCols = 10;
        private const int BrickWidth = 60;
        private const int BrickHeight = 25;
        private const int BrickStartX = 60;
        private const int BrickStartY = 40;
        private const int BrickXSpacing = 70;
        private const int BrickYSpacing = 30;
        private Rectangle playAreaRect;
        private int score = 0;                // Your current score
        private int brickStreak = 0;          // How many bricks hit in one ball bounce
        private int scoreMultiplier = 1;      // The current multiplier
        private bool isPaused = false;

        // --- Game state ---
        private System.Windows.Forms.Timer gameTimer;
        private List<Ball> balls = new List<Ball>();
        private List<Brick> bricks;
        private List<PowerUp> powerUps = new List<PowerUp>();
        private List<ScorePopup> scorePopups = new List<ScorePopup>();
        private Random rand = new Random();
        private bool isGameOver = false;
        private bool gameFinishedRaised = false;
        private double elapsedSeconds = 0;


        // Paddle movement
        private double paddleX;
        private int paddleY;
        private bool leftPressed, rightPressed;





        public Form1()
        {
            InitializeComponent();

            // --- Form setup ---

            playAreaRect = new Rectangle(
                BrickStartX - PlayAreaMargin,
                BrickStartY - PlayAreaMargin,
                (BrickCols - 1) * BrickXSpacing + BrickWidth + PlayAreaMargin * 2,
                (BrickRows - 1) * BrickYSpacing + BrickHeight + PaddleAreaHeight + PlayAreaMargin
            );

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.ClientSize = new Size(WindowWidth, WindowHeight);
            this.DoubleBuffered = true;
            


            // --- Paddle setup ---
            int paddleBottomMargin = 10;
            paddleY = playAreaRect.Bottom - PaddleHeight - paddleBottomMargin;
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;

            // --- Ball setup ---
            int ballStartOffsetY = 50; // Pixels above the paddle

            balls.Clear();
            balls.Add(new Ball(
                x: (int)(paddleX + PaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 6, vy: 6,
                radius: BallRadius
            ));



            // --- Bricks setup ---
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

            // --- Timer for animation ---
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // --- Input events ---
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        // ---- DRAW EVERYTHING ----
       // Inside your Form1 class
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // Fonts
            var fontScore = new Font("Arial", 18, FontStyle.Bold);
            var fontMultiplier = new Font("Arial", 16, FontStyle.Bold);
            var fontGameOver = new Font("Arial", 20, FontStyle.Bold);

            // Timer, score, and multiplier labels
            int minutes = (int)elapsedSeconds / 60, seconds = (int)elapsedSeconds % 60;
            g.DrawString($"Time: {minutes:D2}:{seconds:D2}", fontScore, Brushes.White, playAreaRect.Left + 420, playAreaRect.Top - 40);
            g.DrawString($"Score: {score}", fontScore, Brushes.Yellow, playAreaRect.Left, playAreaRect.Top - 40);
            g.DrawString($"Multiplier: x{scoreMultiplier}", fontMultiplier, Brushes.Orange, playAreaRect.Left + 180, playAreaRect.Top - 40);

            // Play area
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(22, 22, 40)))
                g.FillRectangle(bgBrush, playAreaRect);
            using (Pen borderPen = new Pen(Color.White, 4))
                g.DrawRectangle(borderPen, playAreaRect);

            DrawBricks(g);
            DrawPaddle(g);
            DrawBalls(g);

            foreach (var ball in balls)
            {
                Rectangle ballRect = new Rectangle(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
                using (Brush ballBrush = new SolidBrush(Color.Red))
                using (Pen ballPen = new Pen(Color.White, 2))
                {
                    g.FillEllipse(ballBrush, ballRect);
                    g.DrawEllipse(ballPen, ballRect);
                }
            }

            foreach (var p in powerUps)
                p.Draw(e.Graphics);


            if (isGameOver)
            {
                string overText = "Game Over! Press SPACE to restart";
                SizeF sz = g.MeasureString(overText, fontGameOver);
                float cx = (ClientSize.Width - sz.Width) / 2, cy = (ClientSize.Height - sz.Height) / 2;
                g.DrawString(overText, fontGameOver, Brushes.Red, cx, cy);
            }
            if (isPaused && !isGameOver)
            {
                string pauseText = "Paused (press P to resume)";
                SizeF sz = g.MeasureString(pauseText, fontGameOver);
                float px = (ClientSize.Width - sz.Width) / 2, py = (ClientSize.Height - sz.Height) / 2 + 45;
                g.DrawString(pauseText, fontGameOver, Brushes.Cyan, px, py);
            }
        }


        private void ActivatePowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Multiball:
                    if (balls.Count > 0)
                    {
                        Ball mainBall = balls[0];
                        // Add two new balls at the same spot, with slightly different velocities
                        balls.Add(new Ball(mainBall.X, mainBall.Y, 6, 6, mainBall.Radius));
                        balls.Add(new Ball(mainBall.X, mainBall.Y, -6, 6, mainBall.Radius));
                    }
                    break;
                case PowerUpType.PaddleExtender:
                    PaddleWidth += 30;
                    // Optionally, timer to reset width after a few seconds
                    break;
            }
        }

        // These must be outside Form1_Paint:
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
                        g.FillRectangle(bBrush, r);
                        g.DrawRectangle(bPen, r);
                    }
                }
            }
        }

        private void DrawPaddle(Graphics g)
        {
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight);
            using (var paddleBrush = new SolidBrush(Color.FromArgb(36, 162, 255)))
            using (var paddlePen = new Pen(Color.Blue, 2))
            {
                g.FillRectangle(paddleBrush, paddleRect);
                g.DrawRectangle(paddlePen, paddleRect);
            }
        }

        private void DrawBalls(Graphics g)
        {
            foreach (var ball in balls)
            {
                Rectangle ballRect = new Rectangle(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
                using (Brush ballBrush = new SolidBrush(Color.Red))
                using (Pen ballPen = new Pen(Color.White, 2))
                {
                    g.FillEllipse(ballBrush, ballRect);
                    g.DrawEllipse(ballPen, ballRect);
                }
            }
        }

        // ---- GAME LOGIC + COLLISION ----
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (leftPressed && paddleX > playAreaRect.Left)
                paddleX -= PaddleSpeed;
            if (rightPressed && paddleX < playAreaRect.Right - PaddleWidth)
                paddleX += PaddleSpeed;

            if (!isGameOver && !isPaused)
                elapsedSeconds += gameTimer.Interval / 1000.0;

            if (isPaused)
            {
                Invalidate();
                return;
            }

            foreach (var powerUp in powerUps.ToList())
            {
                powerUp.UpdatePosition();

                // Remove powerups that fall out
                if (powerUp.Y > playAreaRect.Bottom)
                    powerUps.Remove(powerUp);

                // Check collision with paddle:
                Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight);
                Rectangle powerUpRect = new Rectangle(powerUp.X, powerUp.Y, powerUp.Width, powerUp.Height);
                if (paddleRect.IntersectsWith(powerUpRect))
                {
                    ActivatePowerUp(powerUp.Type);
                    powerUps.Remove(powerUp); // Remove collected powerup
                }
            }
            // Only run ball logic if a ball exists
            foreach (var ball in balls.ToList())
            {
                ball.UpdatePosition();

                // Remove the ball if it goes out of bounds (bottom)
                if (ball.Y + ball.Radius * 2 > playAreaRect.Bottom)
                {
                    balls.Remove(ball);
                    if (balls.Count == 0)
                    {
                        TriggerGameOver();
                        return;
                    }
                    continue;
                }

                foreach (var powerUp in powerUps.ToList())
                {
                    powerUp.UpdatePosition();

                    // OPTIONAL: Remove PowerUps that fall below playAreaRect
                    if (powerUp.Y > playAreaRect.Bottom)
                        powerUps.Remove(powerUp);
                }

                // Ball <-> brick collisions
                foreach (var brick in bricks)
                {
                    if (brick.IsVisible && BallHitsRect(ball, brick))
                    {
                        brick.IsVisible = false;
                        ball.InvertVerticalVelocity();
                        brickStreak++;
                        scoreMultiplier = Math.Max(1, brickStreak);
                        score += 10 * scoreMultiplier;
                        scorePopups.Add(new ScorePopup(brick.X + brick.Width / 2, brick.Y + brick.Height / 2, 10 * scoreMultiplier));

                        // Power-up chance
                        if (rand.NextDouble() < 0.2)
                        {
                            var powerUpType = (PowerUpType)(rand.Next(0, 2));
                            powerUps.Add(new PowerUp(brick.X + brick.Width / 2, brick.Y + brick.Height / 2, powerUpType));
                        }


                        break; // Only one brick hit per frame for this ball
                    }
                }

                // Ball <-> wall collisions
                if (ball.X <= playAreaRect.Left)
                {
                    ball.InvertHorizontalVelocity();
                    ball.SetPosition(playAreaRect.Left, ball.Y);
                    brickStreak = 0;
                    scoreMultiplier = 1;
                }
                if (ball.X + ball.Radius * 2 >= playAreaRect.Right)
                {
                    ball.InvertHorizontalVelocity();
                    ball.SetPosition(playAreaRect.Right - ball.Radius * 2, ball.Y);
                    brickStreak = 0;
                    scoreMultiplier = 1;
                }
                if (ball.Y <= playAreaRect.Top)
                {
                    ball.InvertVerticalVelocity();
                    ball.SetPosition(ball.X, playAreaRect.Top);
                    brickStreak = 0;
                    scoreMultiplier = 1;
                }

                // Ball <-> paddle collision
                if (BallHitsRect(ball, new Rectangle((int)paddleX, paddleY, PaddleWidth, PaddleHeight)))
                {
                    double ballCenter = ball.X + ball.Radius;
                    double paddleCenter = paddleX + PaddleWidth / 2.0;
                    double hitPos = (ballCenter - paddleCenter) / (PaddleWidth / 2.0);

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



            // Redraw everything
            Invalidate();
        }

        // ---- HELPER: Ball Hits Rect ----
        private bool BallHitsRect(Ball ball, Rectangle rect)
        {
            if (ball == null) return false; // Safe check!
            return
                ball.X + ball.Radius * 2 >= rect.X &&
                ball.X <= rect.X + rect.Width &&
                ball.Y + ball.Radius * 2 >= rect.Y &&
                ball.Y <= rect.Y + rect.Height;
        }
        private bool BallHitsRect(Ball ball, Brick b)
        {
            return BallHitsRect(
                ball,
                new Rectangle(b.X, b.Y, b.Width, b.Height)
            );
        }

        // ---- INPUT HANDLING ----
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) leftPressed = true;
            if (e.KeyCode == Keys.Right) rightPressed = true;

            // If game over and SPACE is pressed, restart
            if (isGameOver && e.KeyCode == Keys.Space)
            {
                RestartGame();
            }
            if (e.KeyCode == Keys.P)
            {
                isPaused = !isPaused;     // Toggle pause mode
                Invalidate();             // Force redraw to display "Paused" message
            }
        }

        private void RestartGame()
        {
            // Reset all bricks
            foreach (var brick in bricks)
                brick.IsVisible = true;


            gameTimer?.Start();
            gameFinishedRaised = false;
            isGameOver = false;

            score = 0;
            brickStreak = 0;
            scoreMultiplier = 1;
            elapsedSeconds = 0;

            // Center paddle
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;
            paddleY = playAreaRect.Bottom - PaddleHeight - 10;

            // New ball (above paddle)
            balls.Clear(); // remove any existing balls (from e.g. Multiball powerup)
            balls.Add(new Ball(
                x: (int)(paddleX + PaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 6, vy: 6,
                radius: BallRadius
            ));
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
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

        private void RaiseGameFinished()
        {
            if (gameFinishedRaised)
            {
                return;
            }

            gameFinishedRaised = true;
            GameFinished?.Invoke(this, score);

            if (CloseOnGameOver)
            {
                BeginInvoke(new Action(Close));
            }
        }
    }
}
