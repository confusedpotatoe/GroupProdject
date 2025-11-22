using BrickBreaker.Entities;
using BrickBreaker.Utilities;


namespace BrickBreaker
{
    public class GameEngine
    {
        // --- Data ---
        private Random rand = new Random();

        public List<Ball> Balls { get; private set; } = new List<Ball>();
        public List<Brick> Bricks { get; private set; } = new List<Brick>();
        public List<PowerUp> PowerUps { get; private set; } = new List<PowerUp>();
        public List<ScorePopup> ScorePopups { get; private set; } = new List<ScorePopup>();

        public int Score { get; private set; }
        public int CurrentLevel { get; private set; } = 1;

        // --- Paddle State ---
        public int CurrentPaddleWidth { get; private set; } = 100; // Default width
        public bool IsPaddleBlinking { get; private set; } = false;
        private int originalPaddleWidth = 100;
        private int paddleExtenderTicksLeft = 0;

        // --- Events ---
        public event EventHandler GameOver;
        public event EventHandler<int> ScoreChanged;

        // --- Main Loop ---
        public void Update(double deltaTime, Rectangle playArea, double paddleX, int paddleY)
        {
            // A. Handle Effects (Timer for paddle extender)
            UpdatePaddleEffects();

            // Update Balls
            for (int i = Balls.Count - 1; i >= 0; i--)
            {
                var ball = Balls[i];
                ball.UpdatePosition();
                // Use CurrentPaddleWidth property
                HandleBallCollisions(ball, i, playArea, paddleX, paddleY, CurrentPaddleWidth);
            }
            for (int i = ScorePopups.Count - 1; i >= 0; i--)
            {
                ScorePopups[i].Update();
                if (!ScorePopups[i].IsAlive)
                {
                    ScorePopups.RemoveAt(i);
                }
            }

            // Update PowerUps
            UpdatePowerUps(paddleX, paddleY);

            // D. Check Level Completion
            if (Bricks.All(b => !b.IsVisible))
            {
                StartLevel(CurrentLevel + 1, playArea);
            }
        }

        // --- Physics Logic ---
        private void HandleBallCollisions(Ball ball, int ballIndex, Rectangle playArea, double paddleX, int paddleY, int paddleWidth)
        {
            // Wall Collisions
            if (ball.X <= playArea.Left) { ball.X = playArea.Left; ball.VX = -ball.VX; }
            if (ball.X + ball.Radius * 2 >= playArea.Right) { ball.X = playArea.Right - ball.Radius * 2; ball.VX = -ball.VX; }
            if (ball.Y <= playArea.Top) { ball.Y = playArea.Top; ball.VY = -ball.VY; }

            // Floor Collision
            if (ball.Y > playArea.Bottom)
            {
                Balls.RemoveAt(ballIndex);
                if (Balls.Count == 0) GameOver?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Brick Collisions
            foreach (var brick in Bricks.Where(b => b.IsVisible))
            {
                Rectangle brickRect = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);

                if (GameUtils.CheckCollision(ball, brickRect))
                {
                    brick.IsVisible = false;
                    ball.InvertVerticalVelocity();
                    HandleScoring(ball, brick);
                    ScoreChanged?.Invoke(this, Score);

                    // FIX: Do not activate powerup here. Just Try to SPAWN one.
                    TrySpawnPowerUp(brick.X + brick.Width / 2, brick.Y);

                    break;
                }
            }

            // Paddle Collision
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, paddleWidth, 20);
            if (GameUtils.CheckCollision(ball, paddleRect))
            {
                HandlePaddleBounce(ball, paddleX, paddleWidth);
            }
        }

        // --- PowerUp Logic ---

        private void TrySpawnPowerUp(int x, int y)
        {
            // 15% Chance to drop
            if (rand.NextDouble() < 1.0)
            {
                // Pick random type
                var types = Enum.GetValues(typeof(PowerUpType));
                PowerUpType randomType = (PowerUpType)types.GetValue(rand.Next(types.Length));
                PowerUps.Add(new PowerUp(x, y, randomType));
            }
        }

        private void UpdatePowerUps(double paddleX, int paddleY)
        {
            // Define the paddle hitbox using the CURRENT width
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, CurrentPaddleWidth, 20);

            for (int i = PowerUps.Count - 1; i >= 0; i--)
            {
                var p = PowerUps[i];
                p.UpdatePosition();

                // Check if PowerUp hits Paddle
                Rectangle powerUpRect = new Rectangle(p.X, p.Y, p.Width, p.Height);
                if (paddleRect.IntersectsWith(powerUpRect))
                {
                    ActivatePowerUp(p.Type); // <--- This triggers the effect!
                    PowerUps.RemoveAt(i);    // Remove from screen
                }
                else if (p.Y > 1000) // Remove if it falls off screen
                {
                    PowerUps.RemoveAt(i);
                }
            }
        }

        private void ActivatePowerUp(PowerUpType type)
        {
            // 1. Handle Multiball
            if (type == PowerUpType.Multiball && Balls.Count > 0)
            {
                var mainBall = Balls[0]; // Copy the main ball

                // Spawn 2 new balls moving in opposite directions
                Balls.Add(new Ball(mainBall.X, mainBall.Y, 6, -6, mainBall.Radius));
                Balls.Add(new Ball(mainBall.X, mainBall.Y, -6, -6, mainBall.Radius));
            }
            // 2. Handle Paddle Extender
            else if (type == PowerUpType.PaddleExtender)
            {
                CurrentPaddleWidth += 50;  // Grow Paddle
                paddleExtenderTicksLeft = 600; // Last for ~10 seconds
                IsPaddleBlinking = false;
            }
        }
        private void UpdatePaddleEffects()
        {
            if (paddleExtenderTicksLeft > 0)
            {
                paddleExtenderTicksLeft--;

                // Blink when running out
                if (paddleExtenderTicksLeft < 60)
                {
                    IsPaddleBlinking = (paddleExtenderTicksLeft / 4) % 2 == 0;
                }

                if (paddleExtenderTicksLeft == 0)
                {
                    CurrentPaddleWidth = originalPaddleWidth;
                    IsPaddleBlinking = false;
                }
            }
        }
            
        private void HandleScoring(Ball ball, Brick brick)
        {
            ball.BrickStreak++;
            ball.Multiplier = Math.Max(1, ball.BrickStreak);

            int points = 10 * ball.Multiplier;
            Score += points;
            ScoreChanged?.Invoke(this, Score);

            // Add the visual popup (+10)
            ScorePopups.Add(new ScorePopup(brick.X + brick.Width / 2, brick.Y, points));

            // Add multiplier popup if streak is active (x2, x3)
            if (ball.Multiplier > 1)
            {
                ScorePopups.Add(new ScorePopup(brick.X + brick.Width / 2 + 40, brick.Y - 20, $"x{ball.Multiplier}"));
            }
        }

        // --- Helpers & Math ---

        private void HandlePaddleBounce(Ball ball, double paddleX, int paddleWidth)
        {
            ball.BrickStreak = 0;
            ball.Multiplier = 1;

            double ballCenter = ball.X + ball.Radius;
            double paddleCenter = paddleX + paddleWidth / 2.0;
            double hitPos = (ballCenter - paddleCenter) / (paddleWidth / 2.0);

            double speed = 9.0;
            double maxX = speed * 0.75;
            double newVX = hitPos * maxX;

            if (Math.Abs(newVX) < 2.0) newVX = newVX < 0 ? -2.0 : 2.0;

            ball.VX = newVX;
            ball.VY = -Math.Sqrt(speed * speed - newVX * newVX);
        }

        public void StartLevel(int level, Rectangle playArea)
        {
            CurrentLevel = level > 3 ? 1 : level;

            Bricks.Clear();
            Balls.Clear();
            PowerUps.Clear();
            ScorePopups.Clear();

            // Reset paddle
            CurrentPaddleWidth = originalPaddleWidth;
            paddleExtenderTicksLeft = 0;

            int bricksToSpawn = CurrentLevel switch { 1 => 15, 2 => 35, 3 => 60, _ => 15 };

            SpawnBricks(bricksToSpawn, playArea);
            ResetBall(playArea);
        }

        private void SpawnBricks(int count, Rectangle playArea)
        {
            var slots = new List<Point>();
            for (int r = 0; r < GameConstants.InitialBrickRows; r++)
                for (int c = 0; c < GameConstants.InitialBrickCols; c++)
                    slots.Add(new Point(c, r));

            int startX = playArea.Left + GameConstants.PlayAreaMargin;
            int startY = playArea.Top + GameConstants.PlayAreaMargin;

            for (int i = 0; i < count && slots.Count > 0; i++)
            {
                int idx = rand.Next(slots.Count);
                Point slot = slots[idx];

                Bricks.Add(new Brick
                {
                    X = startX + slot.X * GameConstants.BrickXSpacing,
                    Y = startY + slot.Y * GameConstants.BrickYSpacing,
                    Width = GameConstants.BrickWidth,
                    Height = GameConstants.BrickHeight,
                    IsVisible = true,
                    BrickColor = Color.FromArgb(rand.Next(50, 255), rand.Next(50, 255), rand.Next(50, 255))
                });
                slots.RemoveAt(idx);
            }
        }
        public void ShiftWorld(int dx, int dy)
        {
            // Shift Balls
            foreach (var b in Balls) { b.X += dx; b.Y += dy; }

            // Shift Bricks
            foreach (var b in Bricks) { b.X += dx; b.Y += dy; }

            // Shift PowerUps
            foreach (var p in PowerUps) { p.X += dx; p.Y += dy; }

            // Shift Score Popups
            foreach (var s in ScorePopups) { s.Shift(dx, dy); }
            // Note: You will need to add a simple .Shift(x,y) method to ScorePopup class too, 
            // or just access s.X/s.Y directly if the setters are public.
        }

        private void ResetBall(Rectangle playArea)
        {
            Balls.Add(new Ball(
                x: playArea.Left + playArea.Width / 2,
                y: playArea.Bottom - 100,
                vx: 0, vy: 0,
                radius: GameConstants.BallRadius
            ));
        }
    }
}