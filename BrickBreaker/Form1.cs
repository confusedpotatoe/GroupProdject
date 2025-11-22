using System.ComponentModel;
using System.Drawing.Text;

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        // --- Events & Properties ---
        public event EventHandler<int>? GameFinished;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool CloseOnGameOver { get; set; }
        public int LatestScore => score;

        #region 1. Game Constants & Settings
        // Window Layout
        private const int PlayAreaMargin = 2;
        private const int HudHeightOffset = 50;
        private const int PaddleBottomMargin = 10;

        // Gameplay Constants
        private const int InitialBrickRows = 7;
        private const int InitialBrickCols = 10;
        private const int BrickWidth = 60;
        private const int BrickHeight = 25;
        private const int BrickXSpacing = 70;
        private const int BrickYSpacing = 30;
        private const int PaddleAreaHeight = 400;

        // Physics
        private const double BasePaddleSpeed = 13;
        private const int BallRadius = 7;
        #endregion

        #region 2. Game State
        // Mechanics
        private System.Windows.Forms.Timer gameTimer;
        private Random rand = new Random();
        private Rectangle playAreaRect;
        private bool isPaused = false;
        private bool isGameOver = false;
        private bool gameFinishedRaised = false;
        private bool ballReadyToShoot = true;

        // Scoring & Stats
        private int score = 0;
        private int currentLevel = 1;
        private double elapsedSeconds = 0;

        // Paddle State
        private double paddleX;
        private int paddleY;
        private int currentPaddleWidth = 100;
        private int originalPaddleWidth = 100;
        private bool leftPressed, rightPressed;

        // Visual Effects
        private float borderHue = 0f;
        private double timeSinceColorChange = 0;
        private bool isPaddleBlinking = false;
        private int paddleBlinkCounter = 0;
        private int paddleExtenderTicksLeft = 0;
        #endregion

        #region 3. Game Entities
        // Ball, Brick, PowerUp, ScorePopup classes are defineS  in separate files
        private List<Ball> balls = new List<Ball>();
        private List<Brick> bricks = new List<Brick>();
        private List<PowerUp> powerUps = new List<PowerUp>();
        private List<ScorePopup> scorePopups = new List<ScorePopup>();
        #endregion

        #region 4. Assets (Fonts & Colors)
        private PrivateFontCollection fontCollection = new PrivateFontCollection();
        private Font fontScore, fontMultiplier, fontCurrentLevel, fontTime, fontLaunch, fontGameOver;
        private Color colorPaddleNormal = Color.FromArgb(36, 162, 255);
        private Color colorPaddleBlink = Color.OrangeRed;
        #endregion

        public Form1()
        {
            InitializeComponent();
            InitializeFormSettings();
            LoadFonts();
            SetupGameLayout(); // Calculate positions

            // Initialize Game
            originalPaddleWidth = currentPaddleWidth;
            StartLevel(1);

            InitializeTimer();
        }

        private void InitializeFormSettings()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            // Event Subscriptions
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void InitializeTimer()
        {
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        // --- Main Game Loop (The Heart of the Game) ---
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            UpdateVisualEffects();

            if (isGameOver) return;
            if (isPaused) { Invalidate(); return; }

            double deltaTime = gameTimer.Interval / 1000.0;

            // Update Timers
            if (!ballReadyToShoot)
            {
                elapsedSeconds += deltaTime;
                timeSinceColorChange += deltaTime;
            }

            // Update Entities
            UpdateBrickColors();
            UpdatePaddleMovement();
            UpdatePowerUps();
            UpdateBalls();
            UpdatePopups();
            UpdatePaddleEffects();

            //Check Level Completion
            CheckLevelProgression();

            Invalidate(); // Trigger Repaint
        }

        // --- Rendering Logic ---
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            g.Clear(Color.Black);

            DrawPlayArea(g);
            DrawBricks(g);
            DrawPaddle(g);
            DrawBalls(g);

            // Draw transient effects
            foreach (var p in powerUps) p.Draw(g);
            foreach (var pop in scorePopups) pop.Draw(g);

            DrawHUD(g);
            DrawOverlays(g); // Game Over, Launch Text, etc.
        }

        #region Logic: Updates & Physics

        private void UpdateBrickColors()
        {
            if (timeSinceColorChange < 2.0) return;

            foreach (var brick in bricks.Where(b => b.IsVisible))
            {
                brick.BrickColor = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256));
            }
            timeSinceColorChange = 0;
        }

        private void UpdatePaddleMovement()
        {
            if (leftPressed && paddleX > playAreaRect.Left)
                paddleX -= BasePaddleSpeed;
            if (rightPressed && paddleX < playAreaRect.Right - currentPaddleWidth)
                paddleX += BasePaddleSpeed;
        }

        private void UpdateBalls()
        {
            // Iterate backwards to allow removal
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                var ball = balls[i];

                if (ballReadyToShoot)
                {
                    ball.X = (int)(paddleX + currentPaddleWidth / 2 - BallRadius);
                    ball.Y = paddleY - 50;
                    continue;
                }

                ball.UpdatePosition();
                HandleBallCollisions(ball, i);
            }
        }

        private void HandleBallCollisions(Ball ball, int ballIndex)
        {
            // Wall Collisions
            if (ball.X <= playAreaRect.Left) { ball.X = playAreaRect.Left; ball.VX = -ball.VX; }
            if (ball.X + ball.Radius * 2 >= playAreaRect.Right) { ball.X = playAreaRect.Right - ball.Radius * 2; ball.VX = -ball.VX; }
            if (ball.Y <= playAreaRect.Top) { ball.Y = playAreaRect.Top; ball.VY = -ball.VY; }

            // Floor Collision (BALL REMOVED)
            if (ball.Y > playAreaRect.Bottom)
            {
                balls.RemoveAt(ballIndex);
                if (balls.Count == 0) TriggerGameOver();
                return;
            }

            //Brick Collisions
            foreach (var brick in bricks.Where(b => b.IsVisible))
            {
                if (BallHitsRect(ball, new Rectangle(brick.X, brick.Y, brick.Width, brick.Height)))
                {
                    brick.IsVisible = false;
                    ball.InvertVerticalVelocity();
                    HandleScoring(ball, brick);
                    TrySpawnPowerUp(brick.X + brick.Width / 2, brick.Y);
                    break; // Hit one brick per frame max
                }
            }

            // Paddle Collision
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, currentPaddleWidth, 20);
            if (BallHitsRect(ball, paddleRect))
            {
                HandlePaddleBounce(ball);
            }
        }

        private void HandleScoring(Ball ball, Brick brick) // Called when a brick is hit
        {
            ball.BrickStreak++; 
            ball.Multiplier = Math.Max(1, ball.BrickStreak);
            int points = 10 * ball.Multiplier;
            score += points;

            scorePopups.Add(new ScorePopup(brick.X + brick.Width / 2, brick.Y, points));
            if (ball.Multiplier > 1)
                scorePopups.Add(new ScorePopup(brick.X + brick.Width / 2 + 50, brick.Y, $"x{ball.Multiplier}"));
        }

        // Adjust ball velocity based on where it hits the paddle
        private void HandlePaddleBounce(Ball ball)
        {
            ball.BrickStreak = 0;
            ball.Multiplier = 1;

            // Calculate angle based on where it hit the paddle
            double ballCenter = ball.X + ball.Radius;
            double paddleCenter = paddleX + currentPaddleWidth / 2.0;
            double hitPos = (ballCenter - paddleCenter) / (currentPaddleWidth / 2.0); // -1 (Left) to 1 (Right)

            double speed = 9.0;
            double maxX = speed * 0.75;
            double newVX = hitPos * maxX;

            // Enforce minimum horizontal angle so it doesn't get stuck vertically
            if (Math.Abs(newVX) < 2.0) newVX = newVX < 0 ? -2.0 : 2.0;

            ball.VX = newVX;
            ball.VY = -Math.Sqrt(speed * speed - newVX * newVX); // Ensure consistent speed
        }

        // --Power-Up Logic
        private void UpdatePowerUps()
        {
            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                var p = powerUps[i];
                p.UpdatePosition();

                if (p.Y > playAreaRect.Bottom)
                {
                    powerUps.RemoveAt(i);
                    continue;
                }

                Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, currentPaddleWidth, 20);
                if (paddleRect.IntersectsWith(new Rectangle(p.X, p.Y, p.Width, p.Height)))
                {
                    ActivatePowerUp(p.Type);
                    powerUps.RemoveAt(i);
                }
            }
        }
        // --Attempt to spawn a power-up at the given coordinates
        private void TrySpawnPowerUp(int x, int y)
        {
            // 1. Set a chance (e.g., 0.15 = 15% chance)
            double dropChance = 0.15;

            if (rand.NextDouble() < dropChance)
            {
                // 2. Pick a random type from your Enum
                // This gets all values from your PowerUpType enum and picks one
                var types = Enum.GetValues(typeof(PowerUpType));
                PowerUpType randomType = (PowerUpType)types.GetValue(rand.Next(types.Length));

                // 3. Create the powerup (centered on the brick)
                // Note: Adjust width/height (20, 20) if your PowerUp class requires it
                powerUps.Add(new PowerUp(x, y, randomType));
            }
        }

        // --Update and remove expired score popups
        private void UpdatePopups()
        {
            for (int i = scorePopups.Count - 1; i >= 0; i--)
            {
                scorePopups[i].Update();
                if (!scorePopups[i].IsAlive) scorePopups.RemoveAt(i);
            }
        }

        // --Check if all bricks are destroyed to progress to next level
        private void CheckLevelProgression()
        {
            if (bricks.All(b => !b.IsVisible))
            {
                currentLevel = (currentLevel >= 3) ? 1 : currentLevel + 1;
                StartLevel(currentLevel);
            }
        }
        // --Update paddle effects like extender duration and blinking
        private void UpdatePaddleEffects()
        {
            if (paddleExtenderTicksLeft > 0)
            {
                paddleExtenderTicksLeft--;

                // Blinking logic when effect is running out
                if (paddleExtenderTicksLeft < 62)
                {
                    isPaddleBlinking = true;
                    paddleBlinkCounter++;
                }

                if (paddleExtenderTicksLeft == 0)
                {
                    currentPaddleWidth = originalPaddleWidth;
                    isPaddleBlinking = false;
                }
            }
        }

        // --Update visual effects like border color cycling
        private void UpdateVisualEffects()
        {
            borderHue = (borderHue + 1.5f) % 360f;
        }
        #endregion

        #region Logic: Drawing Helpers

        // --Draw the main play area with dynamic border color
        private void DrawPlayArea(Graphics g)
        {
            Color borderColor = ColorFromHSV(borderHue, 1.0f, 1.0f);
            using (Pen borderPen = new Pen(borderColor, 12))
            {
                g.DrawRectangle(borderPen, playAreaRect);
            }
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(22, 22, 40)))
            {
                g.FillRectangle(bgBrush, playAreaRect);
            }
        }

        // --Draw all visible bricks
        private void DrawBricks(Graphics g)
        {
            // Optimization: Reuse the pen
            using (Pen outlinePen = new Pen(Color.Black, 2))
            {
                foreach (var brick in bricks.Where(b => b.IsVisible))
                {
                    Rectangle r = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);
                    using (SolidBrush bBrush = new SolidBrush(brick.BrickColor))
                    {
                        g.FillRectangle(bBrush, r);
                    }
                    g.DrawRectangle(outlinePen, r);
                }
            }
        }

        // --Draw the paddle with potential blinking effect
        private void DrawPaddle(Graphics g)
        {
            Rectangle r = new Rectangle((int)paddleX, paddleY, currentPaddleWidth, 20);
            Color c = (isPaddleBlinking && (paddleBlinkCounter / 8) % 2 == 0) ? colorPaddleBlink : colorPaddleNormal;

            using (var b = new SolidBrush(c))
            using (var p = new Pen(Color.Blue, 2))
            {
                g.FillRectangle(b, r);
                g.DrawRectangle(p, r);
            }
        }

        // --Draw all balls
        private void DrawBalls(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.Red))
            using (Pen p = new Pen(Color.White, 2))
            {
                foreach (var ball in balls)
                {
                    Rectangle r = new Rectangle(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
                    g.FillEllipse(b, r);
                    g.DrawEllipse(p, r);
                }
            }
        }

        // --Draw Heads-Up Display (Score, Time, Level)
        private void DrawHUD(Graphics g)
        {
            int hudY = playAreaRect.Top - HudHeightOffset;

            // Title
            string title = "BrickBreaker";
            float titleX = playAreaRect.Left + (playAreaRect.Width - g.MeasureString(title, fontScore).Width) / 2;
            g.DrawString(title, fontScore, Brushes.White, titleX, playAreaRect.Top - 100);

            // Stats
            string timeStr = $"{(int)elapsedSeconds / 60:D2}:{(int)elapsedSeconds % 60:D2}";
            DrawStatBox(g, $"Score: {score}", fontScore, playAreaRect.Left, hudY);
            DrawStatBox(g, timeStr, fontTime, playAreaRect.Right - 150, hudY); // Adjusted X for alignment

            // Level
            string lvlStr = $"Level {currentLevel}";
            Font safeFont = fontCurrentLevel ?? SystemFonts.DefaultFont;
            g.DrawString(lvlStr, safeFont, Brushes.Black, playAreaRect.Left + 252, hudY + 8); // Shadow
            g.DrawString(lvlStr, safeFont, Brushes.White, playAreaRect.Left + 250, hudY + 6);
        }

        // --Draw a stat box with background and border
        private void DrawStatBox(Graphics g, string text, Font font, int x, int y)
        {
            SizeF size = g.MeasureString(text, font);
            int padding = 10;
            Rectangle box = new Rectangle(x, y, (int)size.Width + padding * 2, (int)size.Height + padding * 2);

            using (Brush bg = new SolidBrush(Color.FromArgb(0, 0, 20)))
            using (Pen border = new Pen(Color.LightGray, 2))
            {
                g.FillRectangle(bg, box);
                g.DrawRectangle(border, box);
            }
            g.DrawString(text, font, Brushes.White, x + padding, y + padding);
        }

        // --Draw overlays like Game Over screen or launch instructions
        private void DrawOverlays(Graphics g)
        {
            if (isGameOver)
            {
                using (SolidBrush dim = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                    g.FillRectangle(dim, ClientRectangle);

                string txt = "Game Over! Press SPACE to restart";
                SizeF sz = g.MeasureString(txt, fontGameOver);
                g.DrawString(txt, fontGameOver, Brushes.Red,
                    (ClientSize.Width - sz.Width) / 2,
                    (ClientSize.Height - sz.Height) / 2);
            }
            else if (ballReadyToShoot && !isPaused)
            {
                string txt = "Press UP ARROW to launch";
                SizeF sz = g.MeasureString(txt, fontLaunch);
                g.DrawString(txt, fontLaunch, Brushes.White,
                    (ClientSize.Width - sz.Width) / 2,
                    paddleY - 80);
            }
        }

        #endregion

        #region Logic: Game Management (Start, Restart, Resize)

        // --Start a new level with specified brick count
        private void StartLevel(int level)
        {
            currentLevel = level;
            bricks.Clear();
            balls.Clear();
            powerUps.Clear();
            scorePopups.Clear();

            int bricksToSpawn = level switch
            {
                1 => 15, // Sparse layout
                2 => 35, // Moderate density
                3 => 60, // Full grid                                           ///TODO: ADD MORE LEVELS LATER
                _ => 15 // Fallback
            };

            SpawnBricks(bricksToSpawn);
            ResetBallAndPaddle();
        }

        // --Spawn a specified number of bricks at random positions
        private void SpawnBricks(int count)
        {
            // Create list of all possible coordinates
            var slots = new List<Point>();
            for (int r = 0; r < InitialBrickRows; r++)
                for (int c = 0; c < InitialBrickCols; c++)
                    slots.Add(new Point(c, r));

            // Calculate board offset based on current layout
            int startX = playAreaRect.Left + PlayAreaMargin;
            int startY = playAreaRect.Top + PlayAreaMargin;

            for (int i = 0; i < count && slots.Count > 0; i++)
            {
                int idx = rand.Next(slots.Count);
                Point slot = slots[idx];

                bricks.Add(new Brick
                {
                    X = startX + slot.X * BrickXSpacing,
                    Y = startY + slot.Y * BrickYSpacing,
                    Width = BrickWidth,
                    Height = BrickHeight,
                    IsVisible = true,
                    BrickColor = Color.FromArgb(rand.Next(50, 255), rand.Next(50, 255), rand.Next(50, 255))
                });

                slots.RemoveAt(idx);
            }
        }

        // --Reset ball and paddle to starting positions
        private void ResetBallAndPaddle()
        {
            currentPaddleWidth = originalPaddleWidth;
            paddleX = playAreaRect.Left + (playAreaRect.Width - currentPaddleWidth) / 2.0;

            balls.Clear();
            balls.Add(new Ball(
                x: (int)(paddleX + currentPaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 0, vy: 0,
                radius: BallRadius
            ));
            ballReadyToShoot = true;
        }

        // --Activate the effect of a collected power-up
        private void ActivatePowerUp(PowerUpType type)
        {
            if (type == PowerUpType.Multiball && balls.Count > 0)
            {
                var main = balls[0];
                balls.Add(new Ball(main.X, main.Y, 6, -6, main.Radius));
                balls.Add(new Ball(main.X, main.Y, -6, -6, main.Radius));
            }
            else if (type == PowerUpType.PaddleExtender)
            {
                currentPaddleWidth += 50;
                paddleExtenderTicksLeft = 300;
                isPaddleBlinking = false;
            }
        }

        // --Handle game over state
        private void TriggerGameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            gameTimer?.Stop();

            if (!gameFinishedRaised)
            {
                gameFinishedRaised = true;
                GameFinished?.Invoke(this, score);
            }
            Invalidate();
        }

        // --Restart the game from level 1
        private void RestartGame()
        {
            score = 0;
            elapsedSeconds = 0;
            isGameOver = false;
            gameFinishedRaised = false;
            ballReadyToShoot = true;

            StartLevel(1);
            gameTimer.Start();
            Invalidate();
        }

        // --Setup or adjust game layout on window resize
        private void SetupGameLayout()
        {
            // Calculate game board dimensions
            int boardW = (InitialBrickCols - 1) * BrickXSpacing + BrickWidth;
            int boardH = (InitialBrickRows - 1) * BrickYSpacing + BrickHeight + PaddleAreaHeight;

            // Center in window
            int startX = (ClientSize.Width - boardW) / 2;
            int startY = (ClientSize.Height - boardH) / 2;

            Rectangle newRect = new Rectangle(
                startX - PlayAreaMargin,
                startY - PlayAreaMargin,
                boardW + PlayAreaMargin * 2,
                boardH + PlayAreaMargin
            );

            // Move entities if resizing
            if (bricks.Count > 0 && playAreaRect.Width > 0)
            {
                int dx = newRect.X - playAreaRect.X;
                int dy = newRect.Y - playAreaRect.Y;

                foreach (var b in bricks) { b.X += dx; b.Y += dy; }
                foreach (var b in balls) { b.X += dx; b.Y += dy; }
                foreach (var p in powerUps) { p.X += dx; p.Y += dy; }
                paddleX += dx;
            }
            else
            {
                // Initial Setup
                paddleX = newRect.Left + (newRect.Width - currentPaddleWidth) / 2.0;
            }

            playAreaRect = newRect;
            paddleY = playAreaRect.Bottom - 20 - PaddleBottomMargin;

            // Clamp Paddle
            if (paddleX < playAreaRect.Left) paddleX = playAreaRect.Left;
            if (paddleX > playAreaRect.Right - currentPaddleWidth) paddleX = playAreaRect.Right - currentPaddleWidth;
        }

        // --Toggle between fullscreen and windowed mode
        private void ToggleFullscreen()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                FormBorderStyle = FormBorderStyle.FixedSingle;
                WindowState = FormWindowState.Normal;
                Size = new Size(1000, 900);
                CenterToScreen();
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                Bounds = Screen.PrimaryScreen.Bounds;
            }
            SetupGameLayout();
            Invalidate();
        }
        #endregion

        #region Input Handling

        // --Handle key presses for game control
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Application.Exit();
            if (e.KeyCode == Keys.F) ToggleFullscreen();
            if (e.KeyCode == Keys.P) { isPaused = !isPaused; Invalidate(); }

            if (isGameOver && e.KeyCode == Keys.Space) RestartGame();

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = true;

            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.W) && ballReadyToShoot)
            {
                var b = balls.FirstOrDefault();
                if (b != null) { b.VX = 0; b.VY = -7; }
                ballReadyToShoot = false;
            }
        }

        // --Handle key releases for paddle movement
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = false;
        }

        #endregion

        #region Utilities (Fonts, Physics, Colors)

        // --Load custom fonts or fallback to system fonts
        private void LoadFonts()
        {
            string path = Path.Combine(Application.StartupPath, "PressStart2P-Regular.ttf");
            if (File.Exists(path))
            {
                fontCollection.AddFontFile(path);
                FontFamily family = fontCollection.Families[0];
                fontScore = new Font(family, 12, FontStyle.Regular);
                fontMultiplier = new Font(family, 12, FontStyle.Regular);
                fontCurrentLevel = new Font(family, 12, FontStyle.Regular);
                fontTime = new Font(family, 12, FontStyle.Regular);
                fontLaunch = new Font(family, 10, FontStyle.Regular);
                fontGameOver = new Font(family, 24, FontStyle.Bold);
            }
            else
            {
                // Fallbacks
                fontScore = new Font("Consolas", 18, FontStyle.Bold);
                fontMultiplier = new Font("Consolas", 18, FontStyle.Bold);
                fontCurrentLevel = new Font("Arial", 12, FontStyle.Bold);
                fontTime = new Font("Consolas", 18, FontStyle.Bold);
                fontLaunch = new Font("Arial", 16, FontStyle.Bold);
                fontGameOver = new Font("Arial", 20, FontStyle.Bold);
            }
        }

        // --Check if a ball intersects with a rectangle (brick or paddle)
        private bool BallHitsRect(Ball ball, Rectangle rect)
        {
            return ball.X + ball.Radius * 2 >= rect.X &&
                   ball.X <= rect.X + rect.Width &&
                   ball.Y + ball.Radius * 2 >= rect.Y &&
                   ball.Y <= rect.Y + rect.Height;
        }

        // --Convert HSV color to RGB Color
        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60.0 - Math.Floor(hue / 60.0);
            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => Color.FromArgb(255, v, t, p),
                1 => Color.FromArgb(255, q, v, p),
                2 => Color.FromArgb(255, p, v, t),
                3 => Color.FromArgb(255, p, q, v),
                4 => Color.FromArgb(255, t, p, v),
                _ => Color.FromArgb(255, v, p, q)
            };
        }
        #endregion
    }
}