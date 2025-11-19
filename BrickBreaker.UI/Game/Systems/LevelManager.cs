namespace BrickBreaker.Game.Systems                   // Namespace for system/game-management classes
{
    // Manages all logic related to game levels (loading, switching, and managing bricks)
    public class LevelManager
    {
        // Stores all preset levels as a list of 2D brick arrays
        private readonly List<bool[,]> _levels = new List<bool[,]>();

        // Current state of bricks for the level being played
        public bool[,] Bricks { get; private set; } = default!;

        // Index of the current level in the _levels list
        public int CurrentLevelIndex { get; private set; } = 0;

        // Constructor: initializes level data and loads level 0 as default
        public LevelManager()
        {
            InitLevels();           // Set up all waypoints/levels
            LoadLevel(0);           // Load the first level on game start
        }

        // Checks if all bricks are cleared (no true values left in Bricks array)
        public bool AllBricksCleared()
        {
            foreach (var brick in Bricks)    // Loops through all brick slots
            {
                if (brick) return false;      // If any brick remains, not cleared
            }
            return true;                      // Returns true if all bricks are gone
        }

        // Loads the specified level into the Bricks property (by copying its brick layout)
        public void LoadLevel(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;   // Sets which level is now active
            // CLONE ensures that when you change Bricks, you don't change _levels (because 2D arrays are reference types)
            Bricks = (bool[,])_levels[levelIndex].Clone();
        }

        // Tries to progress to the next level, returns true if succeeded, false if no more levels
        public bool TryLoadNextLevel()
        {
            if (CurrentLevelIndex + 1 < _levels.Count)     // Checks if another level exists
            {
                LoadLevel(CurrentLevelIndex + 1);          // Loads next level
                return true;                               // Successful
            }
            return false;                                  // No more levels left
        }

        // Initializes the set of available levels (hardcoded layouts)
        private void InitLevels()
        {
            _levels.Clear();   // Removes any existing level definitions

            // Level 1 - simple pattern
            _levels.Add(new bool[12, 4]         // Adds a 12x4 grid for level 1
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

            // Level 2 - zig-zag columns for medium coverage
            _levels.Add(new bool[12, 5]
            {
                { false, true, false, true, false },
                { true, false, true, true, true },
                { true, false, false, false, true },
                { false, true, true, false, false },
                { false, true, false, true, false },
                { true, false, false, true, true },
                { true, false, true, false, false },
                { false, true, true, false, false },
                { false, true, false, true, true },
                { true, false, false, true, true },
                { true, false, true, false, false },
                { false, true, false, false, false }
            });

            // Level 3 - diamond bands widening the play area
            _levels.Add(new bool[12, 6]
            {
                { false, false, true, false, false, false },
                { false, true, true, true, false, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, false },
                { false, true, true, true, false, false },
                { false, false, true, false, false, false },
                { false, false, false, false, false, false },
                { false, true, false, true, false, false },
                { true, true, true, true, true, false },
                { true, true, true, true, true, true },
                { false, true, true, true, false, true },
                { false, false, true, false, false, false }
            });

            // Level 4 - dense ladder before the full wall
            _levels.Add(new bool[12, 6]
            {
                { true, true, true, true, true, true },
                { true, false, true, true, false, true },
                { false, false, true, true, true, false },
                { true, true, true, false, true, true },
                { true, true, false, true, true, true },
                { true, true, false, true, true, true },
                { true, true, true, true, true, true },
                { true, true, true, true, true, true },
                { true, true, false, false, false, true },
                { false, false, true, true, true, false },
                { true, false, true, true, false, true },
                { true, true, true, true, true, true }
            });

            // Level 5 - hard (solid block of bricks)
            _levels.Add(new bool[12, 6]
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
                { true, true, true, true, true, true }
            });
        }
    }
}
