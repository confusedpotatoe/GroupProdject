using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BrickBreaker.Game
{
    /*
    IGame defines a contract: “there is a Run() method that returns a score". 
    BrickBreakerGame : IGame means the class fulfills that contract. 

    Result: Program can call Run() without knowing how the game works or what class implements it.
    */

    // Sealed class means it cannot be inherited from.

    

    public sealed class BrickBreakerGame : IGame
    {
        private Stopwatch gameTimer = new Stopwatch(); // Timer to track game duration
        // ---------------- config / state

        // Width/height of the play area and paddle size we render in the console.
        const int W = 60, H = 24;
        const int PaddleW = 9, TopMargin = 2;

        // The paddle’s position in the console grid.
        int paddleX, paddleY;

        // The ball’s position and velocity.
        int ballX, ballY, dy;
        double vx, vxCarry;

        // bool[,] is a multidimensional array.
        // This 2D array allows indexing: bricks[column, row].
        bool[,] bricks = default!; // Tha default value is null, and we suppress null warnings with !

        // A flag that indicates whether the game loop should continue running.
        bool running;

        //A counter used to control ball speed.
        int ballTick;

        int score;

        // Windows function used to ask "is this key currently pressed?"
        [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);
        static bool IsKeyDown(int vKey) => (GetAsyncKeyState(vKey) & 0x8000) != 0;
        const int VK_LEFT = 0x25, VK_RIGHT = 0x27, VK_ESCAPE = 0x1B;

        // ---------------- public entry
        public int Run()
        {
            var sw = new Stopwatch(); // Stopwatch to measure elapsed time
            var targetDt = TimeSpan.FromMilliseconds(33); // Target delta time for ~30 FPS

            // Prepare game state and placement of objects.
            Init();

            sw.Start(); // Start the stopwatch to measure elapsed time
            gameTimer.Reset();
            gameTimer.Start(); // Start the game timer



            // All of the Console calls are wrapped in try/catch so the game still runs
            // even if the terminal does not support a specific feature.
            try { Console.CursorVisible = false; } catch { }
            Console.OutputEncoding = Encoding.UTF8;
            Console.TreatControlCAsInput = true;
            try { Console.SetWindowSize(Math.Max(Console.WindowWidth, W + 2), Math.Max(Console.WindowHeight, H + 2)); } catch { }

            var last = sw.Elapsed;

            while (running)
            {
                // Run input + update steps often enough to match our target frame time.
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
            gameTimer.Stop(); // Stop the game timer
            try { Console.SetCursorPosition(0, H + 1); Console.CursorVisible = true; } catch { }
            

            Console.WriteLine($"Game time: {gameTimer.Elapsed:mm\\:ss\\.ff}");
            return score;
        }

        


        // ---------------- init
        void Init()
        {
            // Start the paddle in the middle of the bottom row.
            paddleX = (W - PaddleW) / 2;
            paddleY = H - 2;

            // Ball begins near the center moving up-right.
            ballX = W / 2; ballY = H / 2; vx = 1; vxCarry = 0; dy = -1;
            bricks = new bool[10, 5];

            // Fill the brick grid with active bricks (true = brick still exists).
            for (int c = 0; c < bricks.GetLength(0); c++)
                for (int r = 0; r < bricks.GetLength(1); r++)
                    bricks[c, r] = true;

            running = true;
            ballTick = 0;
            score = 0;
        }

        // ---------------- input
        void Input()
        {
            // Throw away buffered key presses so we only look at the current frame.
            while (Console.KeyAvailable) Console.ReadKey(true);
            int speed = 2;
            if (IsKeyDown(VK_LEFT)) paddleX = Math.Max(1, paddleX - speed);
            if (IsKeyDown(VK_RIGHT)) paddleX = Math.Min(W - PaddleW - 1, paddleX + speed);
            if (IsKeyDown(VK_ESCAPE)) running = false;
        }

        // ---------------- update
        void Update()
        {
            // slow ball: run logic 1/3 ticks
            ballTick++;
            if (ballTick % 3 != 0) return;

            int dxStep = ConsumeHorizontalStep();
            int nx = ballX + dxStep;
            int ny = ballY + dy;

            // walls (horizontal)
            if (nx <= 1 || nx >= W - 2)
            {
                ReverseHorizontalVelocity();
                dxStep = -dxStep;
                nx = ballX + dxStep;
            }

            // walls (top)
            if (ny <= TopMargin)
            {
                dy = -dy;
                ny = ballY + dy;
            }

            // paddle
            if (dy > 0 &&
                ny >= paddleY &&
                nx >= paddleX && nx < paddleX + PaddleW)
            {
                dy = -dy;

                int hitPos = Math.Clamp(nx - paddleX, 0, PaddleW - 1);
                ApplyPaddleBounce(hitPos);
                dxStep = 0; // new direction handled by future steps

                ny = paddleY - 1;
            }

            // per-axis brick collision
            // X-axis
            if (nx != ballX)
            {
                var (hitX, cx, rx) = BrickAt(nx, ballY);
                if (hitX)
                {
                    bricks[cx, rx] = false;
                    score += 10;
                    ReverseHorizontalVelocity();
                    dxStep = -dxStep;
                    nx = ballX + dxStep;
                }
            }
            // Y-axis
            if (ny != ballY)
            {
                var (hitY, cy, ry) = BrickAt(nx, ny);
                if (hitY)
                {
                    bricks[cy, ry] = false;
                    score += 10;
                    dy = -dy;
                    ny = ballY + dy;
                }
            }

            ballX = nx;
            ballY = ny;

            // lose if ball exits bottom
            if (ballY >= H - 1) { running = false; return; }
            if (AllBricksCleared()) running = false;
        }

        int ConsumeHorizontalStep()
        {
            vxCarry += vx;
            int step = 0;
            while (vxCarry >= 1)
            {
                vxCarry -= 1;
                step++;
            }
            while (vxCarry <= -1)
            {
                vxCarry += 1;
                step--;
            }
            return step;
        }

        void ReverseHorizontalVelocity()
        {
            vx = -vx;
            vxCarry = -vxCarry;
        }

        void ApplyPaddleBounce(int hitPos)
        {
            double halfWidth = (PaddleW - 1) / 2.0;
            double offset = (hitPos - halfWidth) / halfWidth; // range -1..1

            // Arkanoid-style: edge hits drive shallow angles, center is nearly vertical.
            const double maxSpeed = 2.4;
            double shaped = Math.Sign(offset) * Math.Pow(Math.Abs(offset), 0.65);
            vx = Math.Clamp(shaped * maxSpeed, -maxSpeed, maxSpeed);

            if (Math.Abs(vx) < 0.25)
                vx = (ballX < W / 2) ? -0.25 : 0.25;
            vxCarry = 0;
        }

        // ---------------- collision helpers
        (bool hit, int c, int r) BrickAt(int x, int y)
        {
            // Convert world coordinates (x, y) to indices inside the brick array.
            int cols = bricks.GetLength(0), rows = bricks.GetLength(1);
            int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;
            if (y < brickTop || y >= brickBottom) return (false, -1, -1);

            int r = y - brickTop;
            int c = (x - 1) * cols / (W - 2); // same as Render()
            c = Math.Clamp(c, 0, cols - 1);

            return (bricks[c, r], c, r);
        }

        bool AllBricksCleared()
        {
            // If any brick is still true, the level is not finished yet.
            int cols = bricks.GetLength(0);
            int rows = bricks.GetLength(1);
            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    if (bricks[c, r]) return false;
            return true;
        }
      


        // ---------------- render
        void Render()
        {
            // Build the entire frame in a StringBuilder and draw it in one go.
            var sb = new StringBuilder((W + 1) * (H + 1));
            sb.Append('┌'); sb.Append('─', W - 2); sb.Append('┐').Append('\n');
            for (int y = 1; y < H - 1; y++)
            {
                sb.Append('│');
                for (int x = 1; x < W - 1; x++)
                {
                    char ch = ' ';

                    // bricks
                    int cols = bricks.GetLength(0), rows = bricks.GetLength(1);
                    int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;
                    if (y >= brickTop && y < brickBottom)
                    {
                        int r = y - brickTop;
                        int c = (x - 1) * cols / (W - 2);
                        if (bricks[c, r]) ch = '█';
                    }

                    // paddle
                    if (y == paddleY && x >= paddleX && x < paddleX + PaddleW) ch = '█';

                    // ball
                    if (x == ballX && y == ballY) ch = '●';
                    sb.Append(ch);
                }
                sb.Append('│').Append('\n');
            }
            sb.Append('└'); sb.Append('─', W - 2); sb.Append('┘');

            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
        }
    }
}
