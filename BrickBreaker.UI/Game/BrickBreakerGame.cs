using BrickBreaker.Game.Models;                    // Import main game models (Ball, PowerUp, ScorePop, etc.)
using BrickBreaker.UI.Game.Infrastructure;         // Import game infrastructure interfaces (IGame, IKeyboard, IGameAudio)
using BrickBreaker.Game.Systems;                   // Import game system classes (LevelManager, CollisionHandler, PowerUpLogic)
using BrickBreaker.UI.Game.Models;                 // Import UI/game-specific models (PowerUpType, etc.)
using BrickBreaker.UI.Game.Renderer;               // Import the renderer to handle console output
using System.Diagnostics;                          // Import diagnostic tools (Stopwatch)
using System.Text;                                 // Import string-building utilities
using static BrickBreaker.Game.Models.Constants;   // Allow direct use of constants (W, H, TopMargin, PaddleW)

namespace BrickBreaker.Game                        // Main game namespace
{
    // Main class that runs the BrickBreaker game logic
    public sealed class BrickBreakerGame : IGame
    {
        // System Handlers
        private readonly CollisionHandler _collisionHandler = new CollisionHandler(); // Handles collisions and ball movement
        private readonly ConsoleRenderer _renderer = new ConsoleRenderer();          // Handles console rendering
        private readonly LevelManager _levelManager = new LevelManager();            // Manages level states and bricks

        // Game State
        private bool _paused = false;                    // Game is paused
        private bool _prevSpaceDown = false;             // Tracks spacebar state for pause toggle
        private bool _prevMusicPauseDown = false;        // Tracks 'P' key state for music pause toggle
        private bool running;                            // Main loop controller
        private int lives = 3;                           // Player lives
        private int paddleX, paddleY;                    // Paddle position
        private bool waitingForLaunch = true;            // Is the ball waiting for launch?
        private int ballTick = 0;                        // Tick counter for ball movement timing
        private int paddleTick = 0;                      // Tick counter for paddle movement
        private int powerUpTick = 0;                     // Tick counter for power-up movement
        private readonly Random _random = new Random();  // Random number generator for spawns/events
        private readonly Stopwatch gameTimer = new Stopwatch(); // Tracks elapsed game time

        // Game Object Lists
        private readonly List<Ball> balls = new List<Ball>();        // List of balls in play
        private readonly List<PowerUp> powerUps = new List<PowerUp>();// List of power-ups currently dropping
        private readonly List<ScorePop> scorePops = new List<ScorePop>();// List of score animations/popups

        private int hitMultiplier = 0;                   // Multiplier for consecutive hits
        private int score;                               // Player score
        private int _paddleWidth = PaddleW;              // Current paddle width (can expand)
        private int _paddleExtendTimer = 0;              // Timer for paddle expansion duration
        private const int PaddleExtendWarningFrames = 60; // Frames before shrink where paddle blinks
        private const int PaddleWarnBlinkInterval = 12;  // Frames per blink toggle

        private readonly IKeyboard _keyboard;            // Keyboard input handler
        private readonly IGameAudio _audio;              // Audio/music handler

        // Main constructor: allows injection of keyboard and audio handlers (for testability)
        public BrickBreakerGame(IKeyboard keyboard, IGameAudio audio)
        {
            _keyboard = keyboard;
            _audio = audio;
        }

        // Default constructor: uses Windows keyboard and NAudio audio handler
        public BrickBreakerGame()
        : this(new Win32Keyboard(), new NaudioGameAudio())
        {
        }

        // Main game loop: initializes, starts music, and runs update/draw
        public int Run()
        {
            var sw = new Stopwatch();                                            // Local stopwatch for frame timing
            var targetDt = TimeSpan.FromMilliseconds(1000.0 / 60.0);            // Target time per frame (60fps)

            Init();                                                             // Initialize all game state
            sw.Start();
            gameTimer.Reset();
            gameTimer.Start();

            _audio.StartMusic();                                                // Begin music playback

            try { Console.CursorVisible = false; } catch { }                    // Hide console cursor for clean rendering
            Console.OutputEncoding = Encoding.UTF8;                             // Enable Unicode output
            Console.TreatControlCAsInput = true;                                // Read Ctrl+C as input, not interrupt

            var last = sw.Elapsed;

            while (running)                                                     // Main game loop
            {
                var now = sw.Elapsed;
                while (now - last >= targetDt)                                  // Keep update cadence at roughly 60fps
                {
                    Input();                                                    // Process inputs and controls
                    Update();                                                   // Handle game logic and physics
                    last += targetDt;
                }

                // RENDER CALL: draw the game frame to the console
                bool paddleWarningBlinkOn = ShouldDrawPaddleWarning();

                _renderer.Render(
                    lives, score, _levelManager.CurrentLevelIndex, hitMultiplier, _paused,
                    _levelManager.Bricks,
                    paddleX,
                    _paddleWidth,
                    paddleWarningBlinkOn,
                    balls,
                    powerUps,
                    scorePops);

                var sleep = targetDt - (sw.Elapsed - now);                      // Calculate how long to sleep until next frame
                if (sleep > TimeSpan.Zero) Thread.Sleep(sleep);                 // Frame delay
            }

            gameTimer.Stop();
            _audio.StopMusic();                                                 // Stop music when the game ends

            try { Console.SetCursorPosition(0, H + 1); Console.CursorVisible = true; } catch { } // Show cursor again and move it down
            Console.WriteLine($"Game time: {gameTimer.Elapsed:mm\\:ss\\.ff}");  // Print elapsed game time
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(W / 2 - 5, H / 2);
            Console.Write("GAME OVER!");                                        // Show "GAME OVER" message at center
            Console.ResetColor();
            return score;                                                       // Return the final score
        }

        // Initializes or resets the game state variables and level
        void Init()
        {
            // LevelManager handles its own init
            paddleX = (W - PaddleW) / 2;                     // Center the paddle
            paddleY = H - 2;                                 // Set paddle to near the bottom
            _paddleWidth = PaddleW;                          // Reset paddle width to default
            _paddleExtendTimer = 0;                          // Reset timer for expansion
            ResetBallOnPaddle();                             // Place ball on paddle at start

            powerUps.Clear();                                // Remove any leftover power-ups
            scorePops.Clear();                               // Remove any leftover score popups

            running = true;                                  // Set running flag
            score = 0;                                       // Reset score
            lives = 3;                                       // Reset lives
            hitMultiplier = 0;                               // Reset multiplier
            ballTick = 0;                                    // Reset ball timer
        }

        // Resets the ball to be placed on the paddle (for a new life or start)
        void ResetBallOnPaddle()
        {
            balls.Clear();                                   // Remove all balls
            balls.Add(new Ball(paddleX + PaddleW / 2, paddleY - 1, 0, 0)); // Place a new ball on paddle
            waitingForLaunch = true;                         // Set flag for need to launch
        }

        // Processes all keyboard input each frame
        void Input()
        {
            while (Console.KeyAvailable) Console.ReadKey(true); // Clear input buffer for smooth handling

            // Music controls
            if (_keyboard.IsNPressed()) _audio.Next();              // 'N' key: next track

            bool pauseMusicDown = _keyboard.IsPPressed();           // 'P' key: toggle pause/resume music
            if (pauseMusicDown && !_prevMusicPauseDown)
                _audio.Pause();                                     // Only on edge transition
            _prevMusicPauseDown = pauseMusicDown;

            // Game pause handling
            bool spaceDown = _keyboard.IsSpacePressed();            // Spacebar: toggle game pause
            if (spaceDown && !_prevSpaceDown)
                _paused = !_paused;
            _prevSpaceDown = spaceDown;

            // ESC key: quit the game loop
            if (_keyboard.IsEscapePressed())
                running = false;

            if (_paused) return;                                    // Do not process further input if game is paused

            // Launch the ball (UP arrow)
            bool upDown = _keyboard.IsUpPressed();
            if (waitingForLaunch && upDown)
            {
                if (balls.Count > 0)
                {
                    balls[0].SetHorizontalVelocity(1, 0);           // Set ball velocity to start movement
                    balls[0].SetVerticalVelocity(-1);
                }
                waitingForLaunch = false;
            }

            // Paddle movement (Left/Right arrows) with throttling
            paddleTick++;
            int speed = 2;                                          // How much to move paddle per tick
            if (paddleTick % 2 == 0)                                // Move paddle every other tick for smoothness
            {
                if (_keyboard.IsLeftPressed())
                    paddleX = Math.Max(1, paddleX - speed);         // Move left, clamp to left edge
                if (_keyboard.IsRightPressed())
                    paddleX = Math.Min(W - _paddleWidth - 1, paddleX + speed); // Move right, clamp to right edge

                // Keep the pre-launch ball seated over the paddle center as the player repositions
                if (waitingForLaunch && balls.Count > 0)
                {
                    var tetheredBall = balls[0];
                    int restingX = paddleX + _paddleWidth / 2;
                    tetheredBall.SetPosition(restingX, paddleY - 1);
                    tetheredBall.SetHorizontalVelocity(0, 0);
                    tetheredBall.SetVerticalVelocity(0);
                }
            }
        }

        // Determines if the paddle should be drawn in a warning blink state
        bool ShouldDrawPaddleWarning()
        {
            if (_paddleExtendTimer <= 0 || _paddleWidth <= PaddleW)
            {
                return false;
            }

            if (_paddleExtendTimer > PaddleExtendWarningFrames)
            {
                return false;
            }

            // Blink with a slower cadence during the warning window.
            return ((_paddleExtendTimer / PaddleWarnBlinkInterval) % 2) == 0;
        }

        // Handles all game state updates for each frame
        void Update()
        {
            if (_paused) return;                                    // Do not update anything if paused

            // Handle timer for paddle width expansion (reset when timer ends)
            if (_paddleExtendTimer > 0)
            {
                _paddleExtendTimer--;
                if (_paddleExtendTimer == 0)
                {
                    int widthDifference = _paddleWidth - PaddleW;
                    _paddleWidth = PaddleW;                         // Restore default width
                    paddleX += widthDifference / 2;                 // Shift X so center stays the same
                    paddleX = Math.Clamp(paddleX, 1, W - _paddleWidth - 1); // Clamp to visible screen
                }
            }

            // Update on-screen score popups (fade out)
            for (int i = scorePops.Count - 1; i >= 0; i--)
            {
                scorePops[i].Duration--;
                if (scorePops[i].Duration <= 0)
                    scorePops.RemoveAt(i);                          // Remove expired score popups
            }

            ballTick++;
            if (ballTick % 7 != 0) return;                          // Only update balls every 7 ticks for pacing

            if (!waitingForLaunch)                                   // If ball is in play
            {
                for (int i = balls.Count - 1; i >= 0; i--)
                {
                    var ball = balls[i];
                    var (isRemoved, brickHit) = _collisionHandler.UpdateBall(
                        ball,
                        _levelManager.Bricks,
                        paddleX,
                        paddleY,
                        _paddleWidth,
                        out BrickHitInfo? hitInfo);

                    if (isRemoved)
                    {
                        balls.RemoveAt(i);                          // Remove ball if lost (fell out at bottom)
                        continue;
                    }

                    // Brick hit handling: scoring, multiplier, score popup, power-up spawning
                    if (brickHit && hitInfo != null && _levelManager.Bricks[hitInfo.BrickCol, hitInfo.BrickRow])
                    {
                        _levelManager.Bricks[hitInfo.BrickCol, hitInfo.BrickRow] = false;      // Remove the brick
                        hitMultiplier++;                                  // Boost hit multiplier
                        int addedScore = 10 * hitMultiplier;              // Calculate score bonus
                        score += addedScore;

                        // Prepare and add a visual ScorePop
                        hitInfo.Score = addedScore;
                        hitInfo.X = 1 + (hitInfo.BrickCol * (W - 2)) / _levelManager.Bricks.GetLength(0);
                        hitInfo.Y = TopMargin + 1 + hitInfo.BrickRow;
                        scorePops.Add(hitInfo);

                        // Spawn a power-up with 20% chance
                        if (_random.NextDouble() < 0.2)
                        {
                            PowerUpType dropType = (_random.Next(2) == 0)
                                ? PowerUpType.MultiBall
                                : PowerUpType.PaddleExpand;
                            powerUps.Add(new PowerUp(ball.X, ball.Y, dropType));
                        }
                    }
                    else if (ball.Y == paddleY - 1) // Hit the paddle (reset multiplier)
                    {
                        hitMultiplier = 0;
                    }
                }
            }

            if (balls.Count == 0)                                     // No balls left
            {
                lives--;                                              // Lose a life
                if (lives > 0) ResetBallOnPaddle();                   // Reset for next life if any remain
                else running = false;                                 // Game over
                return;
            }

            UpdatePowerUps();                                         // Update all power-up positions and activations

            // If all bricks are cleared, progress to next level or end game
            if (_levelManager.AllBricksCleared())
            {
                if (!_levelManager.TryLoadNextLevel())
                    running = false; // End game: no more levels
                else
                    ResetBallOnPaddle(); // Start new level: reset ball
            }

            // Handles movement and activation of all power-ups (local function)
            void UpdatePowerUps()
            {
                powerUpTick++;
                if (powerUpTick % 3 != 0) return;                     // Only move power-ups every 3 ticks

                for (int i = powerUps.Count - 1; i >= 0; i--)
                {
                    var pu = powerUps[i];
                    pu.Y++;                                           // Move power-up down

                    // If caught by paddle
                    if (pu.Y == paddleY && pu.X >= (paddleX - 1) && pu.X < (paddleX + _paddleWidth + 1))
                    {
                        PowerUpLogic.ActivatePowerUp(                 // Activate collected power-up
                            pu,
                            balls,
                            ref paddleX,
                            ref _paddleWidth,
                            ref _paddleExtendTimer,
                            paddleY);

                        powerUps.RemoveAt(i);                         // Remove caught power-up
                    }
                    else if (pu.Y > paddleY)
                    {
                        powerUps.RemoveAt(i);                         // Remove missed power-up after it falls off
                    }
                }
            }
        }
    }
}
