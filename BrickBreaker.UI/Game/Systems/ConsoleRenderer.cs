using BrickBreaker.Game.Models;
using BrickBreaker.UI.Game.Models;
using System.Text;
using static BrickBreaker.Game.Models.Constants;

namespace BrickBreaker.UI.Game.Renderer
{
    public class ConsoleRenderer
    {
        // This is the entire Render() method, moved from BrickBreakerGame
        public void Render(
            int lives,
            int score,
            int currentLevel,
            int hitMultiplier,
            bool isPaused,
            bool[,] bricks,
            int paddleX,
            int paddleWidth,
            List<Ball> balls,
            List<PowerUp> powerUps,
            List<ScorePop> scorePops)
        {
            const int paddleY = H - 2;

            Console.ResetColor();

            // Status row
            Console.SetCursorPosition(2, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Lives: {lives,2}  Score: {score,7}  Level: {currentLevel + 1,2}      ");
            Console.ResetColor();

            // Multiplier
            Console.SetCursorPosition(W + 5, 0);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Multiplier: x{hitMultiplier,2}    ");
            Console.ResetColor();

            Console.SetCursorPosition(2, H + 5);
            Console.Write("Press 'N' for next track, 'P' to pause/resume music");


            var sb = new StringBuilder((W + 1) * (H + 2));
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
                    if (y == paddleY && x >= paddleX && x < paddleX + paddleWidth) ch = '█';

                    foreach (var ball in balls)
                    {
                        if (x == ball.X && y == ball.Y)
                            ch = ball.IsMultiball ? '*' : '●';
                    }
                    foreach (var pu in powerUps)
                    {
                        if (x == pu.X && y == pu.Y)
                        {
                            // Now shows 'M' for MultiBall and 'E' for Expand
                            ch = pu.Type == PowerUpType.MultiBall ? 'M' : 'E';
                        }
                    }
                    foreach (var pop in scorePops)
                    {
                        string scoreText = $"+{pop.Score}";
                        if (y == pop.Y && x >= pop.X && x < pop.X + scoreText.Length)
                        {
                            ch = scoreText[x - pop.X];
                        }
                    }

                    sb.Append(ch);
                }
                sb.Append('│').Append('\n');
            }
            sb.Append('└'); sb.Append('─', W - 2); sb.Append('┘');

            Console.SetCursorPosition(0, 1);
            Console.Write(sb.ToString());

            if (isPaused)
            {
                Console.SetCursorPosition(W / 2 - 3, 1);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("PAUSED ");
                Console.ResetColor();
            }
        }
    }
}