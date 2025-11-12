using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using BrickBreaker.Game;
using BrickBreaker.Logics;
using NAudio.Wave;

namespace BrickBreaker.Game
{
    public sealed class BrickBreakerGame : IGame
    {
        bool _paused = false;
        bool _prevSpaceDown = false;

        private IWavePlayer? soundtrackPlayer;
        private AudioFileReader? soundtrackReader;
        private bool musicActive = false;
        private EventHandler<StoppedEventArgs>? playbackStoppedHandler;

        string[] playlist = new string[]
        {
            "Assets/Sounds/Backbeat.mp3",   // first song
            "Assets/Sounds/Arpent.mp3"      // second song
        };
        int currentTrack = 0;

        private Stopwatch gameTimer = new Stopwatch();

        const int W = 60, H = 24;
        const int PaddleW = 9, TopMargin = 2;

        int paddleX, paddleY;
        bool[,] bricks = default!;
        bool running;
        int ballTick;
        int score;
        int lives = 3;

        Random random = new Random();
        List<Ball> balls = new List<Ball>();
        List<PowerUp> powerUps = new List<PowerUp>();
        List<bool[,]> levels = new List<bool[,]>();
        int currentLevel = 0;

        bool waitingForLaunch = true;

        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
        static bool IsKeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;
        const int VK_LEFT = 0x25, VK_RIGHT = 0x27, VK_ESCAPE = 0x1B;

        // --- Score pop-up and multiplier ---
        int hitMultiplier = 0;
        class ScorePop
        {
            public int X, Y, Score;
            public int Duration; // frames remaining to display
        }
        List<ScorePop> scorePops = new List<ScorePop>();

        private void StartMusic()
        {
            musicActive = true;
            soundtrackReader = new AudioFileReader(playlist[currentTrack]);
            soundtrackPlayer = new WaveOutEvent();

            playbackStoppedHandler = (s, e) =>
            {
                if (!musicActive) return;
                currentTrack++;
                if (currentTrack >= playlist.Length) currentTrack = 0;
                soundtrackReader?.Dispose();
                soundtrackReader = new AudioFileReader(playlist[currentTrack]);
                soundtrackPlayer.Init(soundtrackReader);
                soundtrackPlayer.Play();
            };

            soundtrackPlayer.PlaybackStopped += playbackStoppedHandler;
            soundtrackPlayer.Init(soundtrackReader);
            soundtrackPlayer.Play();
        }

        private void StopMusic()
        {
            musicActive = false;
            if (soundtrackPlayer != null)
            {
                if (playbackStoppedHandler != null)
                    soundtrackPlayer.PlaybackStopped -= playbackStoppedHandler;

                soundtrackPlayer.Stop();
                soundtrackPlayer.Dispose();
                soundtrackPlayer = null;
            }

            if (soundtrackReader != null)
            {
                soundtrackReader.Dispose();
                soundtrackReader = null;
            }
        }

        public int Run()
        {
            var sw = new Stopwatch();
            var targetDt = TimeSpan.FromMilliseconds(33);

            Console.SetCursorPosition(2, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Lives: {lives}  Score: {score}  Level: {currentLevel + 1}");
            Console.ResetColor();

            Init();

            sw.Start();
            gameTimer.Reset();
            gameTimer.Start();
            StartMusic();

            try { Console.CursorVisible = false; } catch { }
            Console.OutputEncoding = Encoding.UTF8;
            Console.TreatControlCAsInput = true;
            try { Console.SetWindowSize(Math.Max(Console.WindowWidth, W + 2), Math.Max(Console.WindowHeight, H + 2)); } catch { }

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
                Render();
                var sleep = targetDt - (sw.Elapsed - now);
                if (sleep > TimeSpan.Zero) Thread.Sleep(sleep);
            }
            gameTimer.Stop();
            try { Console.SetCursorPosition(0, H + 1); Console.CursorVisible = true; } catch { }
            StopMusic();

            Console.WriteLine($"Game time: {gameTimer.Elapsed:mm\\:ss\\.ff}");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(W / 2 - 5, H / 2);
            Console.Write("GAME OVER!");
            Console.ResetColor();
            return score;
        }

        void InitLevels()
        {
            levels.Clear();
            // Level 1 - all bricks
            levels.Add(new bool[10, 5]
            {
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true },
                { true, true, true, true, true }
            });
            // Level 2 - alternate pattern
            levels.Add(new bool[10, 5]
            {
                { true, false, true, false, true },
                { false, true, false, true, false },
                { true, false, true, false, true },
                { false, true, false, true, false },
                { true, false, true, false, true },
                { false, true, false, true, false },
                { true, false, true, false, true },
                { false, true, false, true, false },
                { true, false, true, false, true },
                { false, true, false, true, false }
            });
        }
        void LoadLevel(int levelIndex)
        {
            currentLevel = levelIndex;
            bricks = levels[levelIndex];
            paddleX = (W - PaddleW) / 2;
            paddleY = H - 2;
            ResetBallOnPaddle();
            // Score/lives are kept
            hitMultiplier = 0; // Reset multiplier on new level
            scorePops.Clear(); // Remove any leftover pops on level load
        }

        void ResetBallOnPaddle()
        {
            balls.Clear();
            balls.Add(new Ball(paddleX + PaddleW / 2, paddleY - 1, 0, 0));
            waitingForLaunch = true;
        }

        void Init()
        {
            InitLevels();
            LoadLevel(0);
            waitingForLaunch = true;
            balls.Clear();
            balls.Add(new Ball(W / 2, H / 2, 1, -1));

            paddleX = (W - PaddleW) / 2;
            paddleY = H - 2;

            balls.Clear();
            powerUps.Clear();
            balls.Add(new Ball(W / 2, H / 2, 1, -1));
            bricks = new bool[10, 5];

            for (int c = 0; c < bricks.GetLength(0); c++)
                for (int r = 0; r < bricks.GetLength(1); r++)
                    bricks[c, r] = true;

            running = true;
            ballTick = 0;
            score = 0;
            hitMultiplier = 0;
            scorePops.Clear();
        }

        void Input()
        {
            while (Console.KeyAvailable) Console.ReadKey(true);

            bool spaceDown = IsKeyDown((int)ConsoleKey.Spacebar);

            if (spaceDown && !_prevSpaceDown)
                _paused = !_paused;

            _prevSpaceDown = spaceDown;

            if (IsKeyDown(VK_ESCAPE))
                running = false;

            if (_paused)
                return;

            int speed = 2;
            if (IsKeyDown(VK_LEFT))
                paddleX = Math.Max(1, paddleX - speed);
            if (IsKeyDown(VK_RIGHT))
                paddleX = Math.Min(W - PaddleW - 1, paddleX + speed);
        }

        int powerUpTick = 0;

        void Update()
        {
            if (_paused)
                return;

            // Update score pop-ups (remove expired)
            for (int i = scorePops.Count - 1; i >= 0; i--)
            {
                scorePops[i].Duration--;
                if (scorePops[i].Duration <= 0)
                    scorePops.RemoveAt(i);
            }

            ballTick++;
            if (ballTick % 3 != 0) return;

            // --- Balls update & collisions ---
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                var ball = balls[i];
                ball.VxCarry += ball.Vx;
                int dxStep = 0;
                while (ball.VxCarry >= 1) { ball.VxCarry -= 1; dxStep++; }
                while (ball.VxCarry <= -1) { ball.VxCarry += 1; dxStep--; }
                int nx = ball.X + dxStep;
                int ny = ball.Y + ball.Dy;

                // Walls (horizontal)
                if (nx <= 1 || nx >= W - 2)
                {
                    ball.Vx = -ball.Vx;
                    ball.VxCarry = -ball.VxCarry;
                    dxStep = -dxStep;
                    nx = ball.X + dxStep;
                }
                // Walls (top)
                if (ny <= TopMargin)
                {
                    ball.Dy = -ball.Dy;
                    ny = ball.Y + ball.Dy;
                }

                // Paddle
                if (ball.Dy > 0 && ny >= paddleY && nx >= paddleX && nx < paddleX + PaddleW)
                {
                    ball.Dy = -ball.Dy;
                    int hitPos = Math.Clamp(nx - paddleX, 0, PaddleW - 1);
                    ApplyPaddleBounce(ball, hitPos);
                    dxStep = 0;
                    ny = paddleY - 1;
                    hitMultiplier = 0; // Reset multiplier
                }

                // Brick collision X-axis
                if (nx != ball.X)
                {
                    var (hitX, cx, rx) = BrickAt(nx, ball.Y);
                    if (hitX)
                    {
                        bricks[cx, rx] = false;
                        hitMultiplier++;
                        int addedScore = 10 * hitMultiplier;
                        score += addedScore;

                        int popX = 1 + (cx * (W - 2)) / bricks.GetLength(0);
                        int popY = TopMargin + 1 + rx;
                        scorePops.Add(new ScorePop { X = popX, Y = popY, Score = addedScore, Duration = 30 });
                        ball.Vx = -ball.Vx;
                        ball.VxCarry = -ball.VxCarry;
                        dxStep = -dxStep;
                        nx = ball.X + dxStep;

                        // 50% chance to spawn MultiBall power-up
                        if (random.NextDouble() < 0.5)
                            powerUps.Add(new PowerUp(nx, ball.Y, PowerUpType.MultiBall));
                    }
                }

                // Brick collision Y-axis
                if (ny != ball.Y)
                {
                    var (hitY, cy, ry) = BrickAt(nx, ny);
                    if (hitY)
                    {
                        bricks[cy, ry] = false;
                        hitMultiplier++;
                        int addedScore = 10 * hitMultiplier;
                        score += addedScore;

                        int popX = 1 + (cy * (W - 2)) / bricks.GetLength(0);
                        int popY = TopMargin + 1 + ry;
                        scorePops.Add(new ScorePop { X = popX, Y = popY, Score = addedScore, Duration = 30 });

                        ball.Dy = -ball.Dy;
                        ny = ball.Y + ball.Dy;

                        // 50% chance to spawn MultiBall power-up
                        if (random.NextDouble() < 0.5)
                            powerUps.Add(new PowerUp(nx, ball.Y, PowerUpType.MultiBall));
                    }
                }

                ball.X = nx;
                ball.Y = ny;
                if (ball.Y >= H - 1) balls.RemoveAt(i);
            }

            if (balls.Count == 0)
            {
                running = false;
                return;
            }

            UpdatePowerUps();

            // --- Level progression: check if all bricks cleared ---
            if (AllBricksCleared())
            {
                if (currentLevel + 1 < levels.Count)
                    LoadLevel(currentLevel + 1);
                else
                    running = false;
            }
        }

        void UpdatePowerUps()
        {
            powerUpTick++;
            if (powerUpTick % 3 != 0) return;

            for (int i = powerUps.Count - 1; i >= 0; i--)
            {
                var pu = powerUps[i];
                pu.Y++;
                if (pu.Y == paddleY && pu.X >= paddleX && pu.X < paddleX + PaddleW)
                {
                    PowerUpLogic.ActivatePowerUp(pu, balls, paddleX, paddleY);
                    powerUps.RemoveAt(i);
                }
                else if (pu.Y > paddleY)
                {
                    powerUps.RemoveAt(i);
                }
            }
        }

        (bool hit, int c, int r) BrickAt(int x, int y)
        {
            int cols = bricks.GetLength(0), rows = bricks.GetLength(1);
            int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;
            if (y < brickTop || y >= brickBottom) return (false, -1, -1);

            int r = y - brickTop;
            int c = (x - 1) * cols / (W - 2);
            c = Math.Clamp(c, 0, cols - 1);
            return (bricks[c, r], c, r);
        }

        bool AllBricksCleared()
        {
            int cols = bricks.GetLength(0);
            int rows = bricks.GetLength(1);
            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    if (bricks[c, r]) return false;
            return true;
        }

        void ApplyPaddleBounce(Ball ball, int hitPos)
        {
            double halfWidth = (PaddleW - 1) / 2.0;
            double offset = (hitPos - halfWidth) / halfWidth;
            const double maxSpeed = 2.4;
            double shaped = Math.Sign(offset) * Math.Pow(Math.Abs(offset), 0.65);
            ball.Vx = Math.Clamp(shaped * maxSpeed, -maxSpeed, maxSpeed);

            if (Math.Abs(ball.Vx) < 0.25)
                ball.Vx = (ball.X < W / 2) ? -0.25 : 0.25;
            ball.VxCarry = 0;
        }

        void Render()
        {
            Console.ResetColor();

            var sb = new StringBuilder((W + 1) * (H + 1));
            sb.Append('┌'); sb.Append('─', W - 2); sb.Append('┐').Append('\n');

            for (int y = 1; y < H - 1; y++)
            {
                sb.Append('│');
                for (int x = 1; x < W - 1; x++)
                {
                    char ch = ' ';

                    int cols = bricks.GetLength(0), rows = bricks.GetLength(1);
                    int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;
                    if (y >= brickTop && y < brickBottom)
                    {
                        int r = y - brickTop;
                        int c = (x - 1) * cols / (W - 2);
                        if (bricks[c, r]) ch = '█';
                    }

                    if (y == paddleY && x >= paddleX && x < paddleX + PaddleW) ch = '█';

                    foreach (var ball in balls)
                    {
                        if (x == ball.X && y == ball.Y)
                            ch = ball.IsMultiball ? '*' : '●';
                    }

                    foreach (var pu in powerUps)
                    {
                        if (x == pu.X && y == pu.Y)
                        {
                            ch = 'M';
                        }
                    }

                    sb.Append(ch);
                }
                sb.Append('│').Append('\n');
            }
            sb.Append('└'); sb.Append('─', W - 2); sb.Append('┘');
            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());

            // --- Render score pop-ups ---
            foreach (var pop in scorePops)
            {
                try
                {
                    Console.SetCursorPosition(pop.X, pop.Y);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"+{pop.Score}");
                    Console.ResetColor();
                }
                catch { }
            }

            if (_paused)
            {
                Console.SetCursorPosition(W / 2 - 3, 0);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("PAUSED");
                Console.ResetColor();
            }
        }
    }
}
