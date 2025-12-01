using BrickBreaker.Entities;
using BrickBreaker.Utilities;

namespace BrickBreaker
{
    public class GameEngine
    {
        // ==========================================
        // --- Data & State Storage ---
        // ==========================================

        // Random number generator for power-up drops, brick colors, and spawn positions.
        private Random rand = new Random();

        // Lists to hold all active game objects. 
        // 'private set' means other classes can read the list, but only GameEngine can replace the list itself.
        public List<Ball> Balls { get; private set; } = new List<Ball>();
        public List<Brick> Bricks { get; private set; } = new List<Brick>();
        public List<PowerUp> PowerUps { get; private set; } = new List<PowerUp>();
        public List<ScorePopup> ScorePopups { get; private set; } = new List<ScorePopup>();

        // ==========================================
        // --- Game State Variables ---
        // ==========================================

        public int Score { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public int HighScore { get; private set; }

        // Calculates ball speed dynamically. 
        // Base speed is 9.0, adds 1.5 speed for every level passed.
        public double CurrentBallSpeed => 9.0 + (CurrentLevel - 1) * 1.5;

        // ==========================================
        // --- Paddle State & Effects ---
        // ==========================================

        public int CurrentPaddleWidth { get; private set; } = 100; // The width used for collision
        public bool IsPaddleBlinking { get; private set; } = false; // Visual cue for power-up ending
        private int originalPaddleWidth = 100; // Backup to restore width after power-up ends
        private int paddleExtenderTicksLeft = 0; // Timer for the paddle size power-up

        // ==========================================
        // --- Events (Communication) ---
        // ==========================================

        // Fired when the last ball drops. '?' means it can be null (no subscribers).
        public event EventHandler? GameOver;
        // Fired whenever points are added (updates the UI label).
        public event EventHandler<int>? ScoreChanged;
        // Fired when a new level is ready (tells the Form to repaint).
        public event EventHandler? LevelLoaded;

        // ==========================================
        // --- Main Game Loop ---
        // ==========================================

        // Called by the Timer in the Form every frame (approx 60 times/sec).
        public void Update(double deltaTime, Rectangle playArea, double paddleX, int paddleY)
        {
            // 1. Manage Timers (e.g., shrink paddle if time is up)
            UpdatePaddleEffects();

            // 2. Update Balls
            // We loop BACKWARDS (Count - 1 down to 0).
            // This is crucial: if we remove a ball from the list, looping forward would crash or skip items.
            for (int i = Balls.Count - 1; i >= 0; i--)
            {
                var ball = Balls[i];

                // Move ball based on its VX/VY
                ball.UpdatePosition();

                // Check if it hit walls, bricks, paddle, or floor
                HandleBallCollisions(ball, i, playArea, paddleX, paddleY, CurrentPaddleWidth);
            }

            // 3. Update Score Popups (the floating text)
            for (int i = ScorePopups.Count - 1; i >= 0; i--)
            {
                ScorePopups[i].Update(); // Moves text up and fades opacity

                // If text is fully transparent, remove it to save memory
                if (!ScorePopups[i].IsAlive)
                {
                    ScorePopups.RemoveAt(i);
                }
            }

            // 4. Update PowerUps (movement and paddle collection)
            UpdatePowerUps(paddleX, paddleY, playArea);

            // 5. Check Level Completion
            // LINQ: Check if "All" bricks satisfy the condition "IsVisible is false"
            if (Bricks.All(b => !b.IsVisible))
            {
                // Load the next level immediately
                StartLevel(CurrentLevel + 1, playArea);
            }
        }

        // ==========================================
        // --- Physics & Collision Logic ---
        // ==========================================

        private void HandleBallCollisions(Ball ball, int ballIndex, Rectangle playArea, double paddleX, int paddleY, int paddleWidth)
        {
            // --- Wall Collisions ---
            // Left Wall: Reset position to edge, reverse horizontal direction
            if (ball.X <= playArea.Left) { ball.X = playArea.Left; ball.VX = -ball.VX; }
            // Right Wall: Reset position (accounting for diameter), reverse horizontal
            if (ball.X + ball.Radius * 2 >= playArea.Right) { ball.X = playArea.Right - ball.Radius * 2; ball.VX = -ball.VX; }
            // Ceiling: Reset position, reverse vertical direction
            if (ball.Y <= playArea.Top) { ball.Y = playArea.Top; ball.VY = -ball.VY; }

            // --- Floor Collision (Death) ---
            if (ball.Y > playArea.Bottom)
            {
                Balls.RemoveAt(ballIndex); // Kill this specific ball

                // If that was the last ball...
                if (Balls.Count == 0)
                {
                    // Check for high score
                    if (Score > HighScore)
                    {
                        HighScore = Score;
                    }

                    // Trigger the GameOver event so the Form can show the "Game Over" screen
                    GameOver?.Invoke(this, EventArgs.Empty);
                }
                return; // Stop processing this ball
            }

            // --- Brick Collisions ---
            // Only check bricks that are currently visible
            foreach (var brick in Bricks.Where(b => b.IsVisible))
            {
                // Create a temporary rectangle for the brick for intersection testing
                Rectangle brickRect = new Rectangle(brick.X, brick.Y, brick.Width, brick.Height);

                // Helper: Check intersection between Ball Circle and Brick Rectangle
                if (GameUtils.CheckCollision(ball, brickRect))
                {
                    brick.IsVisible = false; // "Destroy" brick
                    ball.InvertVerticalVelocity(); // Simple bounce (physics could be improved here)

                    HandleScoring(ball, brick); // Add points and streaks
                    ScoreChanged?.Invoke(this, Score); // Update UI

                    // Try to drop a powerup at the brick's center X
                    TrySpawnPowerUp(brick.X + brick.Width / 2, brick.Y);

                    break; // Ball can only hit one brick per frame, so exit loop
                }
            }

            // --- Paddle Collision ---
            // Create hitbox for paddle
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, paddleWidth, 20);

            // Check collision
            if (GameUtils.CheckCollision(ball, paddleRect))
            {
                // Calculate custom bounce angle logic
                HandlePaddleBounce(ball, paddleX, paddleWidth);
            }
        }

        // ==========================================
        // --- PowerUp Logic ---
        // ==========================================

        private void TrySpawnPowerUp(int x, int y)
        {
            // 20% Chance (0.0 to 1.0)
            if (rand.NextDouble() < 0.2)
            {
                // Get all possible enum values (Multiball, PaddleExtender, etc.)
                PowerUpType[] types = Enum.GetValues<PowerUpType>();
                // Pick a random one
                PowerUpType randomType = types[rand.Next(types.Length)];
                // Add to list
                PowerUps.Add(new PowerUp(x, y, randomType));
            }
        }

        private void UpdatePowerUps(double paddleX, int paddleY, Rectangle playArea)
        {
            Rectangle paddleRect = new Rectangle((int)paddleX, paddleY, CurrentPaddleWidth, 20);

            // Loop backwards to allow removal
            for (int i = PowerUps.Count - 1; i >= 0; i--)
            {
                var p = PowerUps[i];
                p.UpdatePosition(); // Move down

                // Create hitbox for the powerup
                Rectangle powerUpRect = new Rectangle(p.X, p.Y, p.Width, p.Height);

                // 1. Check if Paddle caught it
                if (paddleRect.IntersectsWith(powerUpRect))
                {
                    ActivatePowerUp(p.Type); // Apply the effect
                    PowerUps.RemoveAt(i);    // Remove from world
                }
                // 2. Check if it fell off screen
                else if (p.Y > playArea.Bottom)
                {
                    PowerUps.RemoveAt(i);
                }
            }
        }

        private void ActivatePowerUp(PowerUpType type)
        {
            // --- Multiball Logic ---
            if (type == PowerUpType.Multiball && Balls.Count > 0)
            {
                var mainBall = Balls[0]; // Reference existing ball for position

                // Add 2 new balls. 
                // Note: Hardcoded velocity (6, -6) gives them an instant upward/diagonal spread.
                Balls.Add(new Ball(mainBall.X, mainBall.Y, 6, -6, mainBall.Radius));
                Balls.Add(new Ball(mainBall.X, mainBall.Y, -6, -6, mainBall.Radius));
            }
            // --- Paddle Extender Logic ---
            else if (type == PowerUpType.PaddleExtender)
            {
                CurrentPaddleWidth += 50;  // Increase logic width
                paddleExtenderTicksLeft = 600; // 600 frames @ 60fps = 10 seconds
                IsPaddleBlinking = false; // Reset blinking status
            }
        }

        private void UpdatePaddleEffects()
        {
            // If timer is active...
            if (paddleExtenderTicksLeft > 0)
            {
                paddleExtenderTicksLeft--; // Tick down

                // Start blinking in the last 1 second (60 frames)
                if (paddleExtenderTicksLeft < 60)
                {
                    // Toggle boolean every 4 frames for rapid blinking effect
                    IsPaddleBlinking = (paddleExtenderTicksLeft / 4) % 2 == 0;
                }

                // Time is up
                if (paddleExtenderTicksLeft == 0)
                {
                    CurrentPaddleWidth = originalPaddleWidth; // Shrink back
                    IsPaddleBlinking = false;
                }
            }
        }

        // ==========================================
        // --- Scoring Logic ---
        // ==========================================

        private void HandleScoring(Ball ball, Brick brick)
        {
            ball.BrickStreak++; // Increment streak for this specific ball
            ball.Multiplier = Math.Max(1, ball.BrickStreak); // Multiplier matches streak

            int points = 10 * ball.Multiplier;
            Score += points;
            ScoreChanged?.Invoke(this, Score); // Notify Form to update label

            // Create the "+10" floating text
            ScorePopups.Add(new ScorePopup(brick.X + brick.Width / 2, brick.Y, points));

            // Create the "x2", "x3" text if applicable
            if (ball.Multiplier > 1)
            {
                ScorePopups.Add(new ScorePopup(brick.X + brick.Width / 2 + 40, brick.Y - 20, $"x{ball.Multiplier}"));
            }
        }

        // ==========================================
        // --- Helpers & Physics Math ---
        // ==========================================

        // Adjusts ball bounce angle based on WHERE it hit the paddle
        private void HandlePaddleBounce(Ball ball, double paddleX, int paddleWidth)
        {
            // Reset streak when hitting paddle (combo broken)
            ball.BrickStreak = 0;
            ball.Multiplier = 1;

            // Calculate center points
            double ballCenter = ball.X + ball.Radius;
            double paddleCenter = paddleX + paddleWidth / 2.0;

            // Determine relative hit position:
            // -1.0 = Far left edge, 0.0 = Center, 1.0 = Far right edge
            double hitPos = (ballCenter - paddleCenter) / (paddleWidth / 2.0);

            // Math to keep total speed constant but change angle
            double speed = CurrentBallSpeed;
            double maxX = speed * 0.75; // Cap horizontal speed at 75% of total speed
            double newVX = hitPos * maxX; // Calculate new Horizontal Velocity

            // Prevent ball from going perfectly straight up (stalling game)
            if (Math.Abs(newVX) < 2.0) newVX = newVX < 0 ? -2.0 : 2.0;

            ball.VX = newVX;
            // Calculate Vertical Velocity using Pythagoras: a^2 + b^2 = c^2 (speed^2)
            // VY = -Sqrt(Speed^2 - VX^2). Negative because UP is negative Y.
            ball.VY = -Math.Sqrt(speed * speed - newVX * newVX);
        }

        // ==========================================
        // --- Level Management ---
        // ==========================================

        public void StartLevel(int level, Rectangle playArea)
        {
            // Loop levels 1-5
            CurrentLevel = level > 5 ? 1 : level;

            // Reset score if restarting the whole game
            if (CurrentLevel == 1)
            {
                Score = 0;
                ScoreChanged?.Invoke(this, Score);
            }

            // Clear all entities from previous level
            Bricks.Clear();
            Balls.Clear();
            PowerUps.Clear();
            ScorePopups.Clear();

            // Reset paddle state
            CurrentPaddleWidth = originalPaddleWidth;
            paddleExtenderTicksLeft = 0;

            // Determine brick count based on level
            // "switch expression" (C# 8.0+)
            int bricksToSpawn = CurrentLevel switch { 1 => 15, 2 => 25, 3 => 35, 4 => 45, 5 => 55, _ => 15 };

            SpawnBricks(bricksToSpawn, playArea);
            ResetBall(playArea);

            // Notify Form that setup is done
            LevelLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void SpawnBricks(int count, Rectangle playArea)
        {
            // 1. Create a list of all possible grid coordinates (slots)
            var slots = new List<Point>();
            for (int r = 0; r < GameConstants.InitialBrickRows; r++)
                for (int c = 0; c < GameConstants.InitialBrickCols; c++)
                    slots.Add(new Point(c, r)); // Store grid coordinates (0,0), (0,1), etc.

            // Calculate offset to start drawing
            int startX = playArea.Left + GameConstants.PlayAreaMargin;
            int startY = playArea.Top + GameConstants.PlayAreaMargin;

            // 2. Loop until we have spawned enough bricks OR ran out of slots
            for (int i = 0; i < count && slots.Count > 0; i++)
            {
                // Pick a random index from the available slots
                int idx = rand.Next(slots.Count);
                Point slot = slots[idx];

                // Add the brick
                Bricks.Add(new Brick
                {
                    // Calculate pixel position based on grid slot * spacing
                    X = startX + slot.X * GameConstants.BrickXSpacing,
                    Y = startY + slot.Y * GameConstants.BrickYSpacing,
                    Width = GameConstants.BrickWidth,
                    Height = GameConstants.BrickHeight,
                    IsVisible = true,
                    // Random RGB Color
                    BrickColor = Color.FromArgb(rand.Next(50, 255), rand.Next(50, 255), rand.Next(50, 255))
                });

                // Remove the used slot so we don't place two bricks on top of each other
                slots.RemoveAt(idx);
            }
        }

        // Handles resizing the game window
        public void ShiftWorld(int dx, int dy)
        {
            // Shift every entity by the change in X and Y
            foreach (var b in Balls) { b.X += dx; b.Y += dy; }
            foreach (var b in Bricks) { b.X += dx; b.Y += dy; }
            foreach (var p in PowerUps) { p.X += dx; p.Y += dy; }
            foreach (var s in ScorePopups) { s.Shift(dx, dy); } // Assumes ScorePopup has a Shift helper
        }

        private void ResetBall(Rectangle playArea)
        {
            // Spawn ball in center-bottom
            Balls.Add(new Ball(
                x: playArea.Left + playArea.Width / 2,
                y: playArea.Bottom - 100,
                vx: 0, vy: 0, // IMPORTANT: Spawns with 0 velocity (won't move yet)
                radius: GameConstants.BallRadius
            ));
        }
    }
}