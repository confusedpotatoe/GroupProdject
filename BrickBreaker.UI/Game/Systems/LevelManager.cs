

namespace BrickBreaker.Game.Systems 
{
    public class LevelManager
    {
        private readonly List<bool[,]> _levels = new List<bool[,]>();
        public bool[,] Bricks { get; private set; } = default!;
        public int CurrentLevelIndex { get; private set; } = 0;

        public LevelManager()
        {
            InitLevels();
            LoadLevel(0);
        }

        // AllBricksCleared logic moved from BrickBreakerGame
        public bool AllBricksCleared()
        {
            foreach (var brick in Bricks)
            {
                if (brick) return false;
            }
            return true;
        }

        // LoadLevel logic moved from BrickBreakerGame
        public void LoadLevel(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;
            // Must CLONE the array, otherwise all levels share the same state
            Bricks = (bool[,])_levels[levelIndex].Clone();
        }

        // Logic for progressing to the next level
        public bool TryLoadNextLevel()
        {
            if (CurrentLevelIndex + 1 < _levels.Count)
            {
                LoadLevel(CurrentLevelIndex + 1);
                return true;
            }
            return false; // No more levels
        }

        // InitLevels logic moved from BrickBreakerGame
        private void InitLevels()
        {
            _levels.Clear();
            // Level 1 - enkel
            _levels.Add(new bool[12, 4]
            {
                { false, true, true, false },
                { true, false, true, true },
                { true, true, false, false },
                { false, false, true, false },
                { true, false, true, false },
                { false, true, false, true },
                { false, true, true, true },
                { true, false, false, true },
                { true, true, false, false },
                { false, false, true, false },
                { true, false, true, false },
                { false, true, false, true }
            });

            // Level 2 - svår
            _levels.Add(new bool[16, 6]
            {
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true }
            });
        }
    }
}