using BrickBreaker.Game.Models;
using BrickBreaker.UI.Game.Models;
using System;
using static BrickBreaker.Game.Models.Constants; // Or your Constants namespace

namespace BrickBreaker.Game.Systems
{
    // (BrickHitInfo class remains the same...)
    public class BrickHitInfo : ScorePop
    {
        public int BrickCol { get; set; }
        public int BrickRow { get; set; }
    }

    public class CollisionHandler
    {
        // --- NEW ApplyPaddleBounce ---
        // (This now accepts paddleWidth)
        public void ApplyPaddleBounce(Ball ball, int hitPos, int paddleWidth)
        {
            double halfWidth = (paddleWidth - 1) / 2.0;
            double normalizedOffset = (hitPos - halfWidth) / halfWidth;

            const double maxSpeed = 2.4;
            double shapedSpeed = Math.Sign(normalizedOffset) * Math.Pow(Math.Abs(normalizedOffset), 0.65);

            double newVx = Math.Clamp(shapedSpeed * maxSpeed, -maxSpeed, maxSpeed);

            const double minAbsSpeed = 0.25;
            if (Math.Abs(newVx) < minAbsSpeed)
                newVx = (ball.X < W / 2) ? -minAbsSpeed : minAbsSpeed;

            ball.SetHorizontalVelocity(newVx, 0);
        }

        // --- BrickAt (No change, this is fine) ---
        public (bool hit, int c, int r) BrickAt(bool[,] bricks, int x, int y)
        {
            // ... (our existing BrickAt logic is correct)
            int cols = bricks.GetLength(0), rows = bricks.GetLength(1);
            int brickTop = TopMargin + 1, brickBottom = TopMargin + 1 + rows;

            if (y < brickTop || y >= brickBottom) return (false, -1, -1);

            int r = y - brickTop;
            int c = (x - 1) * cols / (W - 2);
            c = Math.Clamp(c, 0, cols - 1);

            return (bricks[c, r], c, r);
        }

        // --- REPLACED UpdateBall method ---
        // (This implements the new X/Y separated logic)
        public (bool isRemoved, bool brickHit) UpdateBall(
            Ball ball,
            bool[,] bricks,
            int paddleX,
            int paddleY,
            int paddleWidth, // <-- Pass in current paddle width
            out BrickHitInfo? hitInfo)
        {
            hitInfo = null;
            bool brickHit = false;

            // 1. Calculate Horizontal Movement
            ball.UpdateAndGetDx(out int dxStep);
            int nx = ball.X + dxStep;
            int ny = ball.Y + ball.Dy;

            // 2. Resolve Wall Collisions
            if (nx <= 1) // Left wall
            {
                ball.InvertHorizontalVelocity();
                nx = 2; // Pin to boundary
            }
            else if (nx >= W - 2) // Right wall
            {
                ball.InvertHorizontalVelocity();
                nx = W - 3; // Pin to boundary
            }

            if (ny <= TopMargin) // Top wall
            {
                ball.InvertVerticalVelocity();
                ny = TopMargin + 1; // Pin to boundary
            }

            // 3. Resolve Paddle Collision
            if (ball.Dy > 0 && ny >= paddleY && nx >= paddleX && nx < paddleX + paddleWidth)
            {
                ball.InvertVerticalVelocity();
                int hitPos = Math.Clamp(nx - paddleX, 0, paddleWidth - 1);
                ApplyPaddleBounce(ball, hitPos, paddleWidth); // Pass width

                ny = paddleY - 1; // Pin to top of paddle
                ball.SetPosition(ball.X, ny); // Update Y, keep current X
                return (false, false);
            }

            // 4. Resolve Brick Collisions (X-axis)
            bool brickHitX = false;
            if (nx != ball.X)
            {
                var (hitX, cx, rx) = BrickAt(bricks, nx, ball.Y);
                if (hitX)
                {
                    hitInfo = new BrickHitInfo { BrickCol = cx, BrickRow = rx, Duration = 30 };
                    ball.InvertHorizontalVelocity();
                    nx = ball.X; // Pin to old X
                    brickHitX = true;
                    brickHit = true;
                }
            }

            // 5. Resolve Brick Collisions (Y-axis)
            // (Uses the *potentially modified* nx from step 4)
            if (ny != ball.Y)
            {
                var (hitY, cy, ry) = BrickAt(bricks, nx, ny);
                if (hitY)
                {
                    if (!brickHitX) // Only create new hitInfo if X didn't hit
                    {
                        hitInfo = new BrickHitInfo { BrickCol = cy, BrickRow = ry, Duration = 30 };
                    }
                    ball.InvertVerticalVelocity();
                    ny = ball.Y; // Pin to old Y
                    brickHit = true;
                }
            }

            // 6. Final position update
            ball.SetPosition(nx, ny);

            // 7. Check for out-of-bounds
            if (ball.Y >= H - 1)
                return (true, brickHit);

            return (false, brickHit);
        }

        internal (bool isRemoved, bool brickHit) UpdateBall(Ball ball, bool[,] bricks, int paddleX, int paddleY, out BrickHitInfo? hitInfo)
        {
            throw new NotImplementedException();
        }
    }
}