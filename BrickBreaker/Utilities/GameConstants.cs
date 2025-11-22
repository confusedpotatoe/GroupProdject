using System;

namespace BrickBreaker.Utilities
{
    public static class GameConstants
    {
        // Window Layout
        public const int PlayAreaMargin = 2;
        public const int HudHeightOffset = 50;
        public const int PaddleBottomMargin = 10;

        // Gameplay Constants
        public const int InitialBrickRows = 7;
        public const int InitialBrickCols = 10;
        public const int BrickWidth = 60;
        public const int BrickHeight = 25;
        public const int BrickXSpacing = 70;
        public const int BrickYSpacing = 30;
        public const int PaddleAreaHeight = 400;

        // Physics
        public const double BasePaddleSpeed = 13;
        public const int BallRadius = 7;
    }
}