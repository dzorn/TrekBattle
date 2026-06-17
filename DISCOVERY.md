# Discovery

## Project Description

TrekBattle is a strategy and combat game inspired by the original console CLI Star Trek-style games from the late 1980s.

## Ubiquitous Language

- Galaxy: The top-level 12x12 map of star systems.
- System: A 12x12 local grid where the player can navigate, scan, and fight.
- Krutz: Enemy ships dispersed throughout the galaxy.
- LRS: Long range scan used to inspect nearby galaxy sectors.
- Warp jump: The only supported movement in the current galaxy feature slice.
- End Turn: The control that completes a turn even when no action or movement was taken.
- Ship Systems: Shields, torpedoes, phasers, impulse drive, warp drive, and scanners.
- Power Distribution: Player-controlled allocation of ship power to improve system efficiency.

## Applications

- Angular SPA game client
- ASP.NET Core API backend
- Game simulation and rules engine

## Modules

- Galaxy map and navigation
- System map and encounters
- Ship systems and power allocation
- Combat and enemy AI
- UI and mode switching
- Turn-based galaxy pacing

## Tables

- Galaxy sectors
- Star systems
- Ships
- Planets
- Bases
- Encounters
- Power allocations

## Features

- 12x12 galaxy grid
- 12x12 system grid
- Galaxy travel map
- Fog of war and visited sectors
- Current system scan view
- Long range scan view
- Turn counter and end-turn flow
- Mode switching between views
- Ship power allocation
- Enemy ships, planets, and bases
- Simple graphics and simple navigation

## Requirements

- Requirement 1: Keep the interface simple to use and easy to read.
- Requirement 2: Support both strategic travel and tactical system-level play.
- Requirement 3: Limit enemy ship density to two per system.
- Requirement 4: Support planets and bases in some systems.
- Requirement 5: Allow anonymous play without authentication.
- Requirement 6: Generate a session GUID at game start, show it to the player, and use it to restore saved state later.
- Requirement 7: Provide an app-generated human-friendly resume code for returning players.
- Requirement 8: Resume codes are stored in PascalCase but may be entered case-insensitively.
- Requirement 9: The startup screen must allow a player to enter an existing resume code to restore a previous session.
- Requirement 10: Resuming a session must restore the exact saved game state.
- Requirement 11: Galaxy navigation uses turn-based play with LRS, warp jump, fog of war, and end-turn pacing.
- Requirement 12: Sector scan counts currently initialize to `0` until galaxy generation is added in a later feature.

## Use Cases

- Use Case 1: Player inspects the galaxy map and chooses a system to travel to.
- Use Case 2: Player scans the current system and nearby sectors.
- Use Case 3: Player allocates ship power to optimize combat or travel.
- Use Case 4: Player engages Krutz ships in a system.
- Use Case 5: Player saves the session GUID and later resumes the same game state.
- Use Case 6: Player uses a resume code to return to an existing game session.
- Use Case 7: Player scans nearby sectors, warps to a destination, and ends the turn.

## Actors

- Player
- Krutz enemy ships
- Planetary bodies and bases as world objects

## URLs and Links

- Original inspiration: classic console Star Trek-style gameplay
- Reference material: TBD

## Attachments

- Attachment 1: TBD
- Attachment 2: TBD

## Change Log

- 2026-06-03: Initialized the DevCraft project context with provisional values and discovery placeholders.
- 2026-06-03: Captured the core game concept, grid structure, ship systems, scan modes, and encounter rules.
- 2026-06-17: Added the turn-based galaxy navigation slice with fog of war, LRS, warp jump, and turn tracking.
