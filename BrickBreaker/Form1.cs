using System.ComponentModel;
using System.Drawing.Text;

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

        // --- Font Resources ---
        private PrivateFontCollection pfc = new PrivateFontCollection();
        private Font fontcurrentLevel;
        private Font fontScore;
        private Font fontMultiplier;
        private Font fontGameOver;
        private Font fontTime;
        private Font fontLaunch;

        // --- Graphics Resources ---
        private Pen brickBorderPen = new Pen(Color.DarkGray, 1); // Pen for brick borders

        // --- Ball constants ---
        private const int BallRadius = 7;                // Radius of the ball
        private bool ballReadyToShoot = true; // Indicates if ball is waiting on paddle to be shot

        // --- Paddle constants ---
        private int PaddleWidth = 100;                    // Width of the paddle
        private const int PaddleHeight = 20;              // Height of the paddle
        private const double PaddleSpeed = 13;            // Speed at which paddle moves
        private int originalPaddleWidth;                   // Store original paddle width for power-up resets
        private const int PaddleAreaHeight = 400;         // Height of area below bricks for paddle/ball space


        // --- Paddle blinking effect variables ---
        private bool isPaddleBlinking = false;
        private int paddleBlinkCounter = 0;
        private Color normalPaddleColor = Color.FromArgb(36, 162, 255);
        private Color blinkPaddleColor = Color.OrangeRed;
        private int paddleExtenderTicksLeft = 0; // Number of ticks left for paddle extender effect

        // -- Paddle movement variables ---
        private double paddleX;                              // Current X position of the paddle (floating point for smooth movement)
        private int paddleY;                                 // Fixed Y position of paddle
        private bool leftPressed, rightPressed;             // Whether left or right keys are currently pressed

        // --- Brick constants ---
        private const int BrickRows = 7;                   // Number of brick rows
        private const int BrickCols = 10;                  // Number of brick columns
        private const int BrickWidth = 60;                 // Width of each brick
        private const int BrickHeight = 25;                // Height of each brick
        private int BrickStartX;                            // calculated in constructor
        private int BrickStartY;                            // calculated in constructor
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
        private int currentLevel = 1;                      // Current level
        private bool gameFinishedRaised = false;            // Ensures GameFinished fires only once
        private double elapsedSeconds = 0;                  // Total elapsed time in seconds

        // --- Game state ---
        private System.Windows.Forms.Timer gameTimer;      // Timer controlling game update ticks
        private List<Ball> balls = new List<Ball>();       // List of balls in play (for multiball)
        private List<Brick> bricks;                         // List of bricks in the level
        private List<PowerUp> powerUps = new List<PowerUp>();// List of active powerups falling
        private List<ScorePopup> scorePopups = new List<ScorePopup>(); // List of score popup animations
        private Random rand = new Random();                 // Random number generator for colors/powerups
        private float borderHue = 0f;



        // Constructor: Initialize game form and components
        public Form1()
        {
            InitializeComponent();

            LoadPixelFont();

            // Default start in Fullscreen
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            this.MaximizeBox = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.DoubleBuffered = true;

            // Initialize lists
            bricks = new List<Brick>();

            // --- CALL THE NEW LAYOUT METHOD ---
            SetupGameLayout();

            // Initialize Paddle Width
            originalPaddleWidth = PaddleWidth;
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;

            StartLevel(1);

            // Timer Setup
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // Events
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void LoadPixelFont()
        {
            // The file name must match EXACTLY what you dragged into the project
            string fontFileName = "PressStart2P-Regular.ttf";

            // Combine with the path where the .exe runs
            string fontPath = Path.Combine(Application.StartupPath, fontFileName);

            if (File.Exists(fontPath))
            {
                // Add font to collection
                pfc.AddFontFile(fontPath);

                // Create fonts using the loaded family (Index 0 is the font we just added)
                FontFamily pixelFamily = pfc.Families[0];

                // Adjust sizes (Pixel fonts usually need to be smaller than Arial to look good)
                fontScore = new Font(pixelFamily, 12, FontStyle.Regular);
                fontMultiplier = new Font(pixelFamily, 12, FontStyle.Regular);
                fontcurrentLevel = new Font(pixelFamily, 12, FontStyle.Regular);
                fontTime = new Font(pixelFamily, 12, FontStyle.Regular);
                fontLaunch = new Font(pixelFamily, 10, FontStyle.Regular);
                fontGameOver = new Font(pixelFamily, 24, FontStyle.Bold);
            }
            else
            {
                // FALLBACK: If the file isn't found, use standard fonts so the game doesn't crash
                MessageBox.Show("Font file not found! Using default fonts.");
                fontScore = new Font("Consolas", 18, FontStyle.Bold);
                fontMultiplier = new Font("Consolas", 18, FontStyle.Bold);
                fontTime = new Font("Consolas", 18, FontStyle.Bold);
                fontLaunch = new Font("Arial", 16, FontStyle.Bold);
                fontGameOver = new Font("Arial", 20, FontStyle.Bold);
            }
        }

       
        // Paint event handler to render the game elements each frame
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // makes pixel fonts look crisp instead of blurry
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;

            g.Clear(Color.Black);

            // 1. Draw Play Area
            // Hue = borderHue, Saturation = 1.0 (full), Value = 1.0 (bright)
            Color animatedBorderColor = ColorFromHSV(borderHue, 1.0f, 1.0f);
            using (Pen borderPen = new Pen(animatedBorderColor, 12))
                g.DrawRectangle(borderPen, playAreaRect);
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(22, 22, 40)))
                g.FillRectangle(bgBrush, playAreaRect);

            // 2. Draw Game Objects
            DrawBricks(g);
            DrawPaddle(g);
            DrawBalls(g);

            foreach (var p in powerUps) p.Draw(g);
            foreach (var popup in scorePopups) popup.Draw(g);

            // 3. Draw HUD (Score, Multiplier, Time) IN BOXES
            int minutes = (int)elapsedSeconds / 60, seconds = (int)elapsedSeconds % 60;
            string timeStr = $"{minutes:D2}:{seconds:D2}";

            // Calculate Y position for the HUD boxes (slightly above the play area)
            int hudY = playAreaRect.Top - 50;

            // Draw Score Box
            DrawStatBox(g, $"Score: {score}", fontScore, Brushes.White, playAreaRect.Left, hudY);

            // Draw Multiplier Box
            DrawStatBox(g, $" x{scoreMultiplier}", fontMultiplier, Brushes.White, playAreaRect.Left + 200, hudY);

            // Draw Time Box (Aligned to the right side of play area)
            // We calculate width roughly to align it right, or just use your specific offset
            DrawStatBox(g, timeStr, fontTime, Brushes.White, playAreaRect.Left + 590, hudY);

            string levelStr = $"Level {currentLevel}";

            // Let's adjust X slightly to ensure it doesn't overlap
            float levelX = playAreaRect.Left + 300;
            float levelY = hudY + 6;

            // CRITICAL STEP 1: Check if font is loaded (Prevents crash)
            Font safeFont = fontcurrentLevel ?? new Font("Arial", 12, FontStyle.Bold);

            // CRITICAL STEP 2: DRAW THE SHADOW (Offset by 2 pixels)
            // This makes it visible even if it overlaps something
            g.DrawString(levelStr, safeFont, Brushes.Black, levelX + 2, levelY + 2);

            // CRITICAL STEP 3: DRAW THE TEXT
            // Using Cyan so it stands out from the White Score/Time
            g.DrawString(levelStr, safeFont, Brushes.White, levelX, levelY);




            // 4. Draw Game Over / Pause Text
            if (isGameOver)
            {
                string overText = "Game Over! Press SPACE to restart";
                SizeF sz = g.MeasureString(overText, fontGameOver);
                float cx = (ClientSize.Width - sz.Width) / 2, cy = (ClientSize.Height - sz.Height) / 2;

                // Draw a box behind Game Over text too for better visibility
                using (SolidBrush dimBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                {
                    g.FillRectangle(dimBrush, 0, 0, ClientSize.Width, ClientSize.Height);
                }

                g.DrawString(overText, fontGameOver, Brushes.Red, cx, cy);
            }
            else if (ballReadyToShoot && !isPaused)
            {
                string launchText = "Press UP ARROW to launch the ball";
                SizeF textSize = g.MeasureString(launchText, fontLaunch);
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = paddleY - 80;
                g.DrawString(launchText, fontLaunch, Brushes.White, x, y);
            }
        }

        // Helper method to draw text inside a styled box
        private void DrawStatBox(Graphics g, string text, Font font, Brush textBrush, int x, int y)
        {
            // Measure how big the text is
            SizeF textSize = g.MeasureString(text, font);

            // Define Padding (space between text and border)
            int padding = 10;

            // Create the rectangle for the box
            Rectangle boxRect = new Rectangle(x, y, (int)textSize.Width + (padding * 2), (int)textSize.Height + (padding * 2));

            // Draw the Box Background 
            using (Brush boxBg = new SolidBrush(Color.FromArgb(0, 0, 20)))
            {
                g.FillRectangle(boxBg, boxRect);
            }

            // Draw the Box Border (White/Light Gray)
            using (Pen boxPen = new Pen(Color.LightGray, 2))
            {
                g.DrawRectangle(boxPen, boxRect);
            }

            // Draw the Text inside the box
            g.DrawString(text, font, textBrush, x + padding, y + padding);
        }

        // TODO: Implement so medium difficulty has some moving bricks, and hard has some indestructible ones, or more hits aswell, and moving bricks

        // Initializes and starts a level with a specified difficulty
        private void StartLevel(int level)
        {
            currentLevel = level;
            bricks.Clear();
            balls.Clear();
            powerUps.Clear();
            scorePopups.Clear();

            // Determine difficulty (Brick Count)
            int bricksToSpawn = 0;
            switch (currentLevel)
            {
                case 1: bricksToSpawn = 15; break; // Easy
                case 2: bricksToSpawn = 35; break; // Medium
                case 3: bricksToSpawn = 60; break; // Hard
                default: bricksToSpawn = 15; break; // Fallback to easy
            }


            // list of points so we never pick the same spot twice (no overlapping)
            List<Point> availableSlots = new List<Point>();
            for (int row = 0; row < BrickRows; row++)
            {
                for (int col = 0; col < BrickCols; col++)
                {
                    availableSlots.Add(new Point(col, row));
                }
            }

            // Randomly pick slots from the list until we reach the count
            for (int i = 0; i < bricksToSpawn; i++)
            {
                if (availableSlots.Count == 0) break; // Safety check

                // Pick a random index from available slots
                int index = rand.Next(availableSlots.Count);
                Point slot = availableSlots[index];

                // Create the brick at that slot
                bricks.Add(new Brick
                {
                    X = BrickStartX + slot.X * BrickXSpacing,
                    Y = BrickStartY + slot.Y * BrickYSpacing,
                    Width = BrickWidth,
                    Height = BrickHeight,
                    IsVisible = true,
                    BrickColor = Color.FromArgb(rand.Next(50, 255), rand.Next(50, 255), rand.Next(50, 255))
                });

                // Remove this slot so we dont pick it again
                availableSlots.RemoveAt(index);
            }

            //Reset Player Position
            ResetBallAndPaddle();
        }

        // Helper to reset ball/paddle (Moved out so we can reuse it)
        private void ResetBallAndPaddle()
        {
            PaddleWidth = originalPaddleWidth;
            paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;

            balls.Clear();
            balls.Add(new Ball(
                x: (int)(paddleX + PaddleWidth / 2 - BallRadius),
                y: paddleY - 50,
                vx: 0, vy: 0, // Stopped
                radius: BallRadius
            ));
            ballReadyToShoot = true;
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
                    paddleExtenderTicksLeft = 300; // Lasts for 300 ticks (~5 seconds)
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
                    // We still need a new Brush because colors change per brick
                    Rectangle r = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);
                    using (SolidBrush bBrush = new SolidBrush(brick.BrickColor))
                    {
                        g.FillRectangle(bBrush, r);
                    }
                    using (Pen outlinePen = new Pen(Color.Black, 2))
                    {
                        // This draws the border on TOP of the filled color
                        g.DrawRectangle(outlinePen, r);
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
            // --- NEW: Animate Border Color ---
            borderHue += 1.5f; // Adjust this number to change speed (higher = faster)
            if (borderHue >= 360f) borderHue -= 360f;

            if (!isGameOver && !isPaused)
            {
                // Only increase the timer variables IF the ball has been shot
                if (!ballReadyToShoot)
                {
                    elapsedSeconds += gameTimer.Interval / 1000.0;
                    timeSinceColorChange += gameTimer.Interval / 1000.0;
                }
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
                    double ballCenter = ball.X + ball.Radius; // Ball center X
                    double paddleCenter = paddleX + PaddleWidth / 2.0; // Paddle center X
                    double hitPos = (ballCenter - paddleCenter) / (PaddleWidth / 2.0); // -1 (left) to +1 (right)

                    // Physics math
                    double BallSpeed = 9.0; // (Increased slightly for better feel)
                    double maxHorizontal = BallSpeed * 0.75; // Max horizontal speed component

                    double vx = hitPos * maxHorizontal; // New Horizontal Speed based on hit position

                    // --- NEW: PREVENT VERTICAL LOCK ---
                    // If the ball hits dead center, VX becomes 0. We must prevent that.
                    double minHorizontal = 2.0; // Minimum horizontal speed allowed

                    if (Math.Abs(vx) < minHorizontal) // Too vertical
                    {
                        // If its too vertical, change it to the minimum speed.
                        // keep the sign left/right based on where it hit.
                        if (vx < 0) vx = -minHorizontal;
                        else vx = minHorizontal;
                    }
                    // ----------------------------------

                    // Recalculate Vertical Speed (VY) based on the new VX
                    // This ensures the ball always moves at the same total speed
                    double vy = -Math.Sqrt(BallSpeed * BallSpeed - vx * vx);

                    ball.VX = vx; 
                    ball.VY = vy; 

                    brickStreak = 0; // Reset streak
                    scoreMultiplier = 1; // Reset multiplier
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
            bool allBricksDestroyed = true;
            foreach (var b in bricks)
            {
                if (b.IsVisible)
                {
                    allBricksDestroyed = false;
                    break;
                }
            }

            if (allBricksDestroyed)
            {
                currentLevel++;
                if (currentLevel > 3) currentLevel = 1;
                StartLevel(currentLevel);
                Invalidate();
                return;
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
        private void SetupGameLayout() // NEW METHOD TO HANDLE RESIZING AND CENTERING
        {
            // 1. Remember where the game board WAS before resizing
            int oldStartX = BrickStartX;
            int oldStartY = BrickStartY;

            // 2. Get new Window Size
            WindowWidth = this.ClientSize.Width;
            WindowHeight = this.ClientSize.Height;

            // 3. Calculate the total game size (Standard Math)
            int totalGameWidth = (BrickCols - 1) * BrickXSpacing + BrickWidth;
            int totalGameHeight = (BrickRows - 1) * BrickYSpacing + BrickHeight + PaddleAreaHeight;

            // 4. Calculate NEW Start Positions (Centering)
            BrickStartX = (WindowWidth - totalGameWidth) / 2;
            BrickStartY = (WindowHeight - totalGameHeight) / 2;

            // 5. Define the new Play Area Rectangle
            playAreaRect = new Rectangle(
                BrickStartX - PlayAreaMargin,
                BrickStartY - PlayAreaMargin,
                totalGameWidth + PlayAreaMargin * 2,
                totalGameHeight + PlayAreaMargin
            );

            // --- THE FIX: REPOSITION OBJECTS ---

            // Calculate how far the board moved (Delta)
            int dx = BrickStartX - oldStartX;
            int dy = BrickStartY - oldStartY;

            // Only move things if the game has actually started (bricks exist)
            // and if this isn't the very first setup (oldStartX won't be 0 if initialized)
            if (bricks.Count > 0 && oldStartX != 0)
            {
                // Shift Bricks
                foreach (var brick in bricks)
                {
                    brick.X += dx;
                    brick.Y += dy;
                }

                // Shift Balls
                foreach (var ball in balls)
                {
                    ball.X += dx;
                    ball.Y += dy;
                }

                // Shift Powerups
                foreach (var p in powerUps)
                {
                    p.X += dx;
                    p.Y += dy;
                }

                // Shift Paddle X (horizontal move)
                paddleX += dx;
            }
            else
            {
                // If this is the very first start (Constructor), just center paddle
                paddleX = playAreaRect.Left + (playAreaRect.Width - PaddleWidth) / 2.0;
            }

            // Always force Paddle Y to the bottom of the new rectangle
            int paddleBottomMargin = 10;
            paddleY = playAreaRect.Bottom - PaddleHeight - paddleBottomMargin;

            // Safety check to keep paddle inside bounds
            if (paddleX < playAreaRect.Left) paddleX = playAreaRect.Left;
            if (paddleX > playAreaRect.Right - PaddleWidth) paddleX = playAreaRect.Right - PaddleWidth;
        }
        private void ToggleFullscreen()
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                // SWITCH TO WINDOWED MODE
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
                this.Size = new Size(1000, 900); // Set a default window size
                this.CenterToScreen();
            }
            else
            {
                // SWITCH TO FULLSCREEN
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                this.Bounds = Screen.PrimaryScreen.Bounds;
            }

            // CRITICAL: Recalculate the center of the screen
            SetupGameLayout();

            // Force a redraw so the black background fills the new size
            Invalidate();
        }

        // Keyboard handler for key press events
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Exit Game
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }

            // Toggle Fullscreen
            if (e.KeyCode == Keys.F) // You used F in your snippet, so I kept F here
            {
                ToggleFullscreen();
            }

            // --- MOVEMENT (Arrows OR WASD) ---
            // We ONLY set them to TRUE here. Do not set them to false here!
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = true;

            // Restart Game
            if (isGameOver && e.KeyCode == Keys.Space)
            {
                RestartGame();
                ballReadyToShoot = true;
            }

            // Pause
            if (e.KeyCode == Keys.P)
            {
                isPaused = !isPaused;
                Invalidate();
            }

            // Shoot Ball (Arrow Up OR W)
            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.W) && ballReadyToShoot)
            {
                var mainBall = balls.FirstOrDefault();
                if (mainBall != null)
                {
                    mainBall.VX = 0;
                    mainBall.VY = -7;
                    ballReadyToShoot = false;
                }
            }
        }
        

        // Keyboard handler for key release events
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // When the key is RELEASED, we stop moving
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
                leftPressed = false;

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
                rightPressed = false;
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
            gameTimer.Start(); 
            ballReadyToShoot = true;
            StartLevel(1);

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

        // Converts HSV color values to a Color object
        private Color ColorFromHSV(float hue, float saturation, float value) 
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6; // Which sector of the color wheel

            double f = hue / 60.0 - Math.Floor(hue / 60.0); // Fractional part of hue sector

            value = value * 255; // Scale value to 0-255 range
            int v = Convert.ToInt32(value); // Brightness
            int p = Convert.ToInt32(value * (1 - saturation)); // Value with no saturation

            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            else if (hi == 1) return Color.FromArgb(255, q, v, p);
            else if (hi == 2) return Color.FromArgb(255, p, v, t);
            else if (hi == 3) return Color.FromArgb(255, p, q, v);
            else if (hi == 4) return Color.FromArgb(255, t, p, v);
            else return Color.FromArgb(255, v, p, q);
        }
    }
}
