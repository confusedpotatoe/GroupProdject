# BrickBreaker
![CI](https://github.com/confusedpotatoe/GroupProdject/actions/workflows/ci.yml/badge.svg)

BrickBreaker is a console-based take on the classic arcade game.


## Features

- Login/registration flow with persisted users
- Local leaderboard with timestamps plus a per-user “best score” summary
- BrickBreaker gameplay loop with paddle controls and simple brick layouts
- Terminal UI implementation (Spectre.Console-based visuals are in progress)
- Soundtrack playback via `NAudio`

## Projects & layout

```
BrickBreaker/
├── BrickBreaker.sln           .NET 9.0 solution
├── BrickBreaker.Core/         Shared models + domain logic
│   ├── Models/                User, score, and gameplay entities
│   └── Logic/                 Auth + leaderboard services
├── BrickBreaker.Storage/      Persistence abstractions
│   ├── FilePathProvider.cs    Reads config + resolves data locations
│   ├── UserStore.cs           Local JSON store (currently active)
│   ├── LeaderboardStore.cs    Local JSON store (currently active)
│   └── Properties/appsettings.json
├── BrickBreaker.UI/           Console host
│   ├── Program.cs             State machine for menus/gameplay
│   ├── Game/                  Game engine + rendering
│   └── Ui/                    Console menu/dialog abstractions
└── README.md
```

## Getting started

```bash
# Restore dependencies
dotnet restore

# Run the console UI (starts in login/registration menu)
dotnet run --project BrickBreaker.UI

# Optional: build everything, useful for CI parity
dotnet build BrickBreaker.sln
```

## Data migration to Supabase PostgreSQL

We are currently migrating user + leaderboard data from the JSON files above to
Supabase-hosted PostgreSQL. 

## Flowchart

<img width="1626" height="493" alt="Flowchart" src="https://github.com/user-attachments/assets/6643ef6c-470c-4cf0-b2f9-c19385b91c9d" />

## Class diagram

<img width="788" height="756" alt="Class diagram" src="https://github.com/user-attachments/assets/61565f48-db45-4d2b-b7d3-4128a7e54e80" />
