using BrickBreaker.Game.Models;
using BrickBreaker.UI.Game.Infrastructure;
using BrickBreaker.Game.Systems;
using BrickBreaker.UI.Game.Infrastructure;
using BrickBreaker.UI.Game.Models;
using BrickBreaker.UI.Game.Renderer;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static BrickBreaker.Game.Models.Constants;

namespace BrickBreaker.Game
{
    public sealed class BrickBreakerGame : IGame
    {
        // System Handlers
        private readonly CollisionHandler _collisionHandler = new CollisionHandler();
        private readonly ConsoleRenderer _renderer = new ConsoleRenderer();
        private readonly LevelManager _levelManager = new LevelManager();

        // Game State
        private bool _paused = false;
        private bool _prevSpaceDown = false;
        private bool _prevMusicPauseDown = false;
        private bool running;
        private int lives = 3;
        private int paddleX, paddleY;
        private bool waitingForLaunch = true;
        private int ballTick = 0;
        private int paddleTick = 0;
        private int powerUpTick = 0;
        private readonly Random _random = new Random();
        private readonly Stopwatch gameTimer = new Stopwatch();

        // Game Object Lists
        private readonly List<Ball> balls = new List<Ball>();
        private readonly List<PowerUp> powerUps = new List<PowerUp>();
        private readonly List<ScorePop> scorePops = new List<ScorePop>();
        private int hitMultiplier = 0;
        private int score;
        private int _paddleWidth = PaddleW; // <-- NEW: Use this for current width
        private int _paddleExtendTimer = 0;


        private readonly IKeyboard _keyboard;
        private readonly IGameAudio _audio;

        public BrickBreakerGame(IKeyboard keyboard, IGameAudio audio)
        {
            _keyboard = keyboard;
            _audio = audio;
        }

        public BrickBreakerGame()
        : this(new Win32Keyboard(), new NaudioGameAudio())
        {
        }

        public int Run()
        {
            var sw = new Stopwatch();
            var targetDt = TimeSpan.FromMilliseconds(1000.0 / 60.0); // ~16.67ms for 60fps

            Init();
            sw.Start();
            gameTimer.Reset();
            gameTimer.Start();

            _audio.StartMusic();

            try { Console.CursorVisible = false; } catch { }
            Console.OutputEncoding = Encoding.UTF8;
            Console.TreatControlCAsInput = true;

            var last = sw.Elapsed;

            while (running)
            {
                var now = sw.Elapsed;
                while (now - last >= targetDt)
                {
                    Input();
                    Update();
                    last += targetDt;
                }

                // RENDER CALL
                _renderer.Render(
                    lives, score, _levelManager.CurrentLevelIndex, hitMultiplier, _paused,
                    _levelManager.Bricks,
                    paddleX,
                    _paddleWidth,
                    balls,
                    powerUps,
                    scorePops);

                var sleep = targetDt - (sw.Elapsed - now);
                if (sleep > TimeSpan.Zero) Thread.Sleep(sleep);
            }

            gameTimer.Stop();
            _audio.StopMusic();

            try { Console.SetCursorPosition(0, H + 1); Console.CursorVisible = true; } catch { }
            Console.WriteLine($"Game time: {gameTimer.Elapsed:mm\\:ss\\.ff}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(W / 2 - 5, H / 2);
            Console.Write("GAME OVER!");
            Console.ResetColor();
            return score;
        }

        void Init()
        {
            // LevelManager handles its own init
            paddleX = (W - PaddleW) / 2;
            paddleY = H - 2;
            _paddleWidth = PaddleW; // NEW: Reset paddle width
            _paddleExtendTimer = 0; // NEW: Reset timer
            ResetBallOnPaddle();

            powerUps.Clear();
            scorePops.Clear();

            running = true;
            score = 0;
            lives = 3;
            hitMultiplier = 0;
            ballTick = 0;
        }

        void ResetBallOnPaddle()
        {
            balls.Clear();
            // Use the *default* width for placing the ball
            balls.Add(new Ball(paddleX + PaddleW / 2, paddleY - 1, 0, 0));
            waitingForLaunch = true;
        }

        void Input()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);

            // Musikstyrning
            if (_keyboard.IsNPressed()) _audio.Next();

            bool pauseMusicDown = _keyboard.IsPPressed();
            if (pauseMusicDown && !_prevMusicPauseDown)
                _audio.Pause();
            _prevMusicPauseDown = pauseMusicDown;



            // Hantera paus
            bool spaceDown = _keyboard.IsSpacePressed();
            if (spaceDown && !_prevSpaceDown)
                _paused = !_paused;
            _prevSpaceDown = spaceDown;

            // Hantera ESC för att avsluta
            if (_keyboard.IsEscapePressed())
                running = false;

            if (_paused) return;

            // Hantera launch (UP)
            bool upDown = _keyboard.IsUpPressed();
            if (waitingForLaunch && upDown)
            {
                if (balls.Count > 0)
                {
                    balls[0].SetHorizontalVelocity(1, 0);
                    balls[0].SetVerticalVelocity(-1);
                }
                waitingForLaunch = false;
            }

            // Flytta paddeln vänster/höger
            paddleTick++;
            int speed = 2;
            if (paddleTick % 2 == 0)
            {
                if (_keyboard.IsLeftPressed())
                    paddleX = Math.Max(1, paddleX - speed);
                if (_keyboard.IsRightPressed())
                    paddleX = Math.Min(W - _paddleWidth - 1, paddleX + speed);
            }
        }





        void Update()
        {
            if (_paused) return;

            // This block should *only* handle the timer logic
            if (_paddleExtendTimer > 0)
            {
                _paddleExtendTimer--;
                if (_paddleExtendTimer == 0)
                {
                    int widthDifference = _paddleWidth - PaddleW;
                    _paddleWidth = PaddleW; // Reset to default
                    paddleX += widthDifference / 2; // Shift back
                    paddleX = Math.Clamp(paddleX, 1, W - _paddleWidth - 1);
                }
            }


            // Update score pop-ups
            for (int i = scorePops.Count - 1; i >= 0; i--)
            {
                scorePops[i].Duration--;
                if (scorePops[i].Duration <= 0)
                    scorePops.RemoveAt(i);
            }

            ballTick++;
            if (ballTick % 7 != 0) return;


            if (!waitingForLaunch)
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
                        balls.RemoveAt(i);
                        continue;
                    }

                    if (brickHit && hitInfo != null && _levelManager.Bricks[hitInfo.BrickCol, hitInfo.BrickRow])
                    {
                        // Brick was hit remove it
                        _levelManager.Bricks[hitInfo.BrickCol, hitInfo.BrickRow] = false;
                        hitMultiplier++;
                        int addedScore = 10 * hitMultiplier;
                        score += addedScore;

                        // Finalize the ScorePop data
                        hitInfo.Score = addedScore;
                        hitInfo.X = 1 + (hitInfo.BrickCol * (W - 2)) / _levelManager.Bricks.GetLength(0);
                        hitInfo.Y = TopMargin + 1 + hitInfo.BrickRow;
                        scorePops.Add(hitInfo);

                        // Spawn power-up
                        if (_random.NextDouble() < 0.2)
                        {
                            PowerUpType dropType = (_random.Next(2) == 0)
                                ? PowerUpType.MultiBall
                                : PowerUpType.PaddleExpand;
                            powerUps.Add(new PowerUp(ball.X, ball.Y, dropType));
                        }
                    }
                    else if (ball.Y == paddleY - 1) // Hit the paddle
                    {
                        hitMultiplier = 0;
                    }
                }
            }

            if (balls.Count == 0)
            {
                lives--;
                if (lives > 0) ResetBallOnPaddle();
                else running = false;
                return;
            }

            UpdatePowerUps();

            // Level moved to another file!
            if (_levelManager.AllBricksCleared())
            {
                if (!_levelManager.TryLoadNextLevel())
                    running = false; // No more levels
                else
                    ResetBallOnPaddle(); // Reset for new level
            }

            // This is a "local function" and its definition is correct here
            void UpdatePowerUps()
            {
                powerUpTick++;
                if (powerUpTick % 3 != 0) return;

                for (int i = powerUps.Count - 1; i >= 0; i--)
                {
                    var pu = powerUps[i];
                    pu.Y++;


                    if (pu.Y == paddleY && pu.X >= (paddleX - 1) && pu.X < (paddleX + _paddleWidth + 1))
                    {
                        // Call the new PowerUpLogic method
                        PowerUpLogic.ActivatePowerUp(
                            pu,
                            balls,
                            ref paddleX,
                            ref _paddleWidth,
                            ref _paddleExtendTimer,
                            paddleY);

                        powerUps.RemoveAt(i);
                    }
                    else if (pu.Y > paddleY)
                    {
                        powerUps.RemoveAt(i);
                    }
                }
            }
        }
    }
}
