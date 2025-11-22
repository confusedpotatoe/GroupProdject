using BrickBreaker.Utilities;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BrickBreaker
{
    public partial class Form1 : Form
    {
        // --- 1. The Engine ---
        private GameEngine gameEngine = null!;

        public event EventHandler<int>? GameFinished;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool CloseOnGameOver { get; set; }

        public int LatestScore => gameEngine != null ? gameEngine.Score : 0;

        // --- 2. UI State ---
        private System.Windows.Forms.Timer gameTimer;
        private Rectangle playAreaRect;
        private bool isPaused = false;
        private bool isGameOver = false;
        private bool ballReadyToShoot = true;
        private double elapsedSeconds = 0;

        // Paddle (UI Input controls this)
        private double paddleX;
        private int paddleY;
        private bool leftPressed, rightPressed;

        // Visual Effects
        private float borderHue = 0f;

        // Fonts & Colors
        private PrivateFontCollection fontCollection = new PrivateFontCollection();
        private Font fontScore, fontMultiplier, fontCurrentLevel, fontTime, fontLaunch, fontGameOver;
        private Color colorPaddleNormal = Color.FromArgb(36, 162, 255);

        public Form1()
        {
            InitializeComponent();
            InitializeFormSettings();
            LoadFonts();

            // Setup Engine
            gameEngine = new GameEngine();

            // Listen for Events from Engine
            gameEngine.GameOver += (s, e) => TriggerGameOver();
            gameEngine.ScoreChanged += (s, newScore) => { /* Optional: Play Sound */ };

            SetupGameLayout();

            // Start Game
            gameEngine.StartLevel(1, playAreaRect);
            InitializeTimer();
        }

        private void InitializeFormSettings()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.DoubleBuffered = true;
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void InitializeTimer()
        {
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
        }

        // --- Main Loop ---
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isGameOver || isPaused) { Invalidate(); return; }

            double deltaTime = gameTimer.Interval / 1000.0;

            // 1. ALWAYS allow paddle movement and visual effects (so you can aim before shooting)
            UpdatePaddleMovement();
            UpdateVisualEffects();

            // 2. ONLY update Game Logic (Physics & Time) if the ball has been launched
            if (!ballReadyToShoot)
            {
                elapsedSeconds += deltaTime;

                // Update Physics
                gameEngine.Update(deltaTime, playAreaRect, paddleX, paddleY);
            }

            // 3. Handle "Ball Stuck to Paddle" Logic
            // This keeps the ball glued to the paddle before you press Up
            if (ballReadyToShoot && gameEngine.Balls.Count > 0)
            {
                var b = gameEngine.Balls[0];
                b.X = (int)(paddleX + gameEngine.CurrentPaddleWidth / 2 - GameConstants.BallRadius);
                b.Y = paddleY - 40;
                b.VX = 0;
                b.VY = 0;
            }

            Invalidate(); // Draw the screen
        }

        // --- Drawing ---
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            g.Clear(Color.Black);

            DrawPlayArea(g);
            DrawPowerUps(g);
            DrawScorePopups(g);
            DrawBricks(g);
            DrawBalls(g);
            DrawHUD(g);
            DrawOverlays(g);
            DrawPaddle(g);
        }

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

        private void DrawBricks(Graphics g)
        {
            using (Pen outlinePen = new Pen(Color.Black, 2))
            {
                foreach (var brick in gameEngine.Bricks.Where(b => b.IsVisible))
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

        private void DrawBalls(Graphics g)
        {
            using (Brush b = new SolidBrush(Color.Red))
            using (Pen p = new Pen(Color.White, 2))
            {
                foreach (var ball in gameEngine.Balls)
                {
                    Rectangle r = new Rectangle(ball.X, ball.Y, ball.Radius * 2, ball.Radius * 2);
                    g.FillEllipse(b, r);
                    g.DrawEllipse(p, r);
                }
            }
        }

        private void DrawPaddle(Graphics g)
        {
            Rectangle r = new Rectangle((int)paddleX, paddleY, gameEngine.CurrentPaddleWidth, 20);

            // Check if paddle is blinking (visual effect for extender ending)
            Color c = gameEngine.IsPaddleBlinking && (DateTime.Now.Millisecond / 100) % 2 == 0
                      ? Color.OrangeRed
                      : colorPaddleNormal;

            using (var b = new SolidBrush(c))
            using (var p = new Pen(Color.Blue, 2))
            {
                g.FillRectangle(b, r);
                g.DrawRectangle(p, r);
            }
        }

        private void DrawHUD(Graphics g)
        {
            int hudY = playAreaRect.Top - GameConstants.HudHeightOffset;

            // --- TITLE LOGIC ---
            string title = "BRICK BREAKER";
            SizeF titleSize = g.MeasureString(title, fontGameOver);

            // 1. Center Horizontally
            float titleX = (ClientSize.Width - titleSize.Width) / 2;

            // 2. Center Vertically
            float titleY = playAreaRect.Top - 100;
            if (titleY < 0) titleY = 10;

            // Draw Shadow (Gray)
            g.DrawString(title, fontGameOver, Brushes.Gray, titleX + 4, titleY + 4);
            // Draw Text (Cyan)
            g.DrawString(title, fontGameOver, Brushes.Cyan, titleX, titleY);
            // -------------------

            // Stats
            string timeStr = $"{(int)elapsedSeconds / 60:D2}:{(int)elapsedSeconds % 60:D2}";
            DrawStatBox(g, $"Score: {gameEngine.Score}", fontScore, playAreaRect.Left, hudY);
            DrawStatBox(g, timeStr, fontTime, playAreaRect.Right - 110, hudY);

            // Level
            string lvlStr = $"Level {gameEngine.CurrentLevel}";
            Font safeFont = fontCurrentLevel ?? SystemFonts.DefaultFont;
            g.DrawString(lvlStr, safeFont, Brushes.White, playAreaRect.Left + 300, hudY + 6);
        }

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
        private void DrawPowerUps(Graphics g) 
        {
            foreach (var p in gameEngine.PowerUps)
            {
                p.Draw(g, fontLaunch);
            }
        }
        private void DrawScorePopups(Graphics g)
        {
            foreach (var pop in gameEngine.ScorePopups)
            {
                pop.Draw(g);
            }
        }

        // --- Game Updates ---

        private void UpdatePaddleMovement()
        {
            if (leftPressed && paddleX > playAreaRect.Left)
                paddleX -= GameConstants.BasePaddleSpeed;

            if (rightPressed && paddleX < playAreaRect.Right - gameEngine.CurrentPaddleWidth)
                paddleX += GameConstants.BasePaddleSpeed;
        }

        private void UpdateVisualEffects()
        {
            borderHue = (borderHue + 1.5f) % 360f;
        }

        // --- Input Handling ---
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Application.Exit();
            if (e.KeyCode == Keys.F) ToggleFullscreen();
            if (e.KeyCode == Keys.P) { isPaused = !isPaused; Invalidate(); }

            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = true;

            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.W) && ballReadyToShoot)
            {
                if (gameEngine.Balls.Count > 0)
                {
                    gameEngine.Balls[0].VY = -9;
                    ballReadyToShoot = false;
                }
            }

            if (isGameOver && e.KeyCode == Keys.Space) RestartGame();
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = false;
        }

        // --- Helper Methods ---

        private void SetupGameLayout()
        {
            // 1. Calculate the Board Size (This doesn't change)
            int boardW = (GameConstants.InitialBrickCols - 1) * GameConstants.BrickXSpacing + GameConstants.BrickWidth;
            int boardH = (GameConstants.InitialBrickRows - 1) * GameConstants.BrickYSpacing + GameConstants.BrickHeight + GameConstants.PaddleAreaHeight;

            // 2. Keep a copy of the OLD area before we change it
            Rectangle oldRect = playAreaRect;

            // 3. Calculate the NEW area (Centered in window)
            int startX = (ClientSize.Width - boardW) / 2;
            int startY = (ClientSize.Height - boardH) / 2;

            Rectangle newRect = new Rectangle(
                startX - GameConstants.PlayAreaMargin,
                startY - GameConstants.PlayAreaMargin,
                boardW + GameConstants.PlayAreaMargin * 2,
                boardH + GameConstants.PlayAreaMargin
            );

            // 4. Apply the change to the Form's variables
            playAreaRect = newRect;

            // 5. Calculate the difference and shift objects accordingly
            if (oldRect.Width > 0 && oldRect.Height > 0)
            {
                int dx = newRect.X - oldRect.X;
                int dy = newRect.Y - oldRect.Y;

                // Tell Engine to move bricks/balls
                gameEngine.ShiftWorld(dx, dy);

                // Move the Paddle (which is controlled by Form1)
                paddleX += dx;
                paddleY = playAreaRect.Bottom - 40; // Recalculate Y just to be safe
            }
            else
            {
                // Initial Setup (First time run)
                paddleY = playAreaRect.Bottom - 40;
                paddleX = playAreaRect.Left + (playAreaRect.Width - gameEngine.CurrentPaddleWidth) / 2;
            }
        }

        private void TriggerGameOver()
        {
            isGameOver = true;
            gameTimer.Stop();

        }

        private void RestartGame()
        {
            isGameOver = false;
            ballReadyToShoot = true;
            gameEngine.StartLevel(1, playAreaRect);
            gameTimer.Start();
        }

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

        // --- Font Loading ---
        private void LoadFonts()
        {
            string path = Path.Combine(Application.StartupPath, "Assets", "PressStart2P-Regular.ttf");
            if (File.Exists(path))
            {
                fontCollection.AddFontFile(path);
                FontFamily family = fontCollection.Families[0];
                fontScore = new Font(family, 12, FontStyle.Regular);
                fontMultiplier = new Font(family, 12, FontStyle.Regular);
                fontCurrentLevel = new Font(family, 12, FontStyle.Regular);
                fontTime = new Font(family, 12, FontStyle.Regular);
                fontLaunch = new Font(family, 10, FontStyle.Regular);
                fontGameOver = new Font(family, 15, FontStyle.Bold);
            }
            else
            {
                fontScore = new Font("Consolas", 18, FontStyle.Bold);
                fontMultiplier = new Font("Consolas", 18, FontStyle.Bold);
                fontCurrentLevel = new Font("Arial", 12, FontStyle.Bold);
                fontTime = new Font("Consolas", 18, FontStyle.Bold);
                fontLaunch = new Font("Arial", 16, FontStyle.Bold);
                fontGameOver = new Font("Arial", 20, FontStyle.Bold);
            }
        }

        // --- Color RainbowEffect ---
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
    }
}