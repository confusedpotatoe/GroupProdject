# BrickBreaker
![CI](https://github.com/confusedpotatoe/GroupProdject/actions/workflows/ci.yml/badge.svg)  [![codecov](https://codecov.io/gh/<user>/<repo>/branch/main/graph/badge.svg)](https://codecov.io/gh/<user>/<repo>)


BrickBreaker is a .NET 9 console remake of the classic paddle-and-bricks arcade game.  
This project includes Spectre.Console menus, a frame-based renderer, soundtrack playback via `NAudio`, and Supabase/PostgreSQL storage for players and leaderboards.

## Overview

- **Two gameplay modes** – log in for tracked scores or jump into Quick Play without an account.
- **State-machine UI** – Spectre.Console drives login, registration, leaderboard, and gameplay menus with progress indicators and dialogs.
- **Feature-complete engine** – multi-level brick layouts, a 60 FPS loop, paddle physics, multi-ball + paddle-extend power-ups, pause/music controls, and animated score pops.
- **Hosted leaderboard** – usernames, passwords, and timestamped scores are persisted to Supabase PostgreSQL when a connection string is configured.
- **Offline fallback** – if the connection string is missing, the app still runs with disabled stores and warns that auth/leaderboard actions are skipped.
- **Automated tests** – unit tests cover core auth + leaderboard flows using the abstractions in `BrickBreaker.Core`.

## Project layout

```
BrickBreaker/
├── BrickBreaker.sln             Solution root (net9.0)
├── BrickBreaker.Core/           Domain models + services (Auth, Leaderboard, abstractions)
├── BrickBreaker.Storage/        Supabase/PostgreSQL stores + configuration helpers
│   ├── StorageConfiguration.cs  Resolves Supabase connection strings
│   ├── UserStore.cs             Npgsql-backed implementation
│   ├── LeaderboardStore.cs      Npgsql-backed implementation
│   ├── Disabled*.cs             Null-object stores for offline play
│   └── Properties/appsettings.json
├── BrickBreaker.UI/             Console host, menus, renderer, audio assets
│   ├── Program.cs               App state machine (login → gameplay)
│   ├── Game/                    Engine, systems, renderer, assets
│   └── Ui/                      Spectre.Console menus/dialogs
├── BrickBreaker.Tests/          xUnit tests for Auth + Leaderboard logic
└── README.md
```

## Run the game locally

Prerequisites: .NET 9 SDK and (optionally) access to the Supabase/PostgreSQL instance referenced below.

```bash
# Restore all projects
dotnet restore

# Run the console UI (starts at the login/quick-play screen)
dotnet run --project BrickBreaker.UI

# Optional: build every project or run the unit tests
dotnet build BrickBreaker.sln
dotnet test BrickBreaker.sln
```

### Configure Supabase/PostgreSQL

The UI checks for a connection string at startup:

1. Update `BrickBreaker.Storage/Properties/appsettings.json` **or** set an environment variable named `Supabase` / `ConnectionString:Supabase`.
2. Provide a standard Npgsql connection string, for example:

```json
{
  "ConnectionString": {
    "Supabase": "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true"
  }
}
```

When present, registration/login, leaderboard submissions, per-user best scores, and the Top-10 table all use the hosted database. When absent, the UI clearly warns that those features are disabled but gameplay/Quick Play still works.

### Controls & tips

- `←` / `→` move the paddle, `Space` toggles pause.
- `N` skips to the next soundtrack track, `P` toggles music pause.
- Quick Play skips authentication and does not attempt to submit scores.

## Tests

Run `dotnet test BrickBreaker.sln` to execute the xUnit suite. Tests rely on the storage abstractions, so they run without real database access.

## Flowchart

<img width="1231" height="491" alt="image" src="https://github.com/user-attachments/assets/bce43212-df89-4663-b631-87512729f4f6" />

## Class diagram

<img width="970" height="763" alt="image" src="https://github.com/user-attachments/assets/0eba64c1-322c-4232-abb4-f43ab4d0ddb0" />

