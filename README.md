# BrickBreaker
![CI](https://github.com/confusedpotatoe/GroupProdject/actions/workflows/ci.yml/badge.svg)

<img width="1626" height="493" alt="image" src="https://github.com/user-attachments/assets/6643ef6c-470c-4cf0-b2f9-c19385b91c9d" />


<img width="788" height="756" alt="image" src="https://github.com/user-attachments/assets/61565f48-db45-4d2b-b7d3-4128a7e54e80" />

## Repository structure

```
BrickBreaker/                   ← repo root (also the project root)
│
├── BrickBreaker.sln            ← solution file
├── BrickBreaker.csproj         ← project file (Console App)
│
├── Program.cs                 ← entry point + menus/state
│
├── Models/                    ← data-only types
│   ├── User.cs
│   └── ScoreEntry.cs
│
├── Logic/                     ← operations on models
│   ├── Auth.cs
│   └── Leaderboard.cs
│
├── Storage/                   ← persistence (JSON)
│   ├── UserStore.cs
│   └── LeaderboardStore.cs
│
├── Game/                      ← gameplay
│   ├── IGame.cs
│   └── BrickBreakerGame.cs
│
├── data/                      ← committed samples
│   ├── users.sample.json
│   └── leaderboard.sample.json
│
├── .gitignore                 ← ignore runtime artifacts
└── README.md
```
