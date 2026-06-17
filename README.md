# TrekBattle

## Overview

TrekBattle is a strategy and combat game inspired by the original console CLI Star Trek-style games from the late 1980s.

## Goals

- Build a simple, readable game interface that is easy to navigate.
- Simulate a galaxy as a 12x12 grid of systems, with each system also represented as a 12x12 grid.
- Support ship combat, travel, scanning, turn-based pacing, and power management.
- Keep the game feel classic while using modern .NET and web tooling.
- Let anyone play without needing an account or authentication.
- Persist game state by anonymous session GUID so players can return later.
- Provide an app-generated human-friendly resume code alongside the session GUID.
- Store resume codes in PascalCase while allowing case-insensitive player entry.
- Allow players to enter an existing resume code to restore a previous session.
- Restore the exact saved game state when a player resumes.
- Let the player use a galaxy map, long range scan, warp jump, and End Turn flow during strategic play.

## Key Features

- Galaxy travel map
- Turn-based galaxy navigation
- Current system scan view
- Long range scan view
- Ship power allocation across shields, torpedoes, phasers, impulse drive, warp drive, and scanners
- Enemy ship encounters, planets, and bases

## Applications or Deliverables

- Game client
- Game simulation and rules engine
- Angular SPA front end
- ASP.NET Core API backend

## Notes

The project now has the first strategic navigation slice implemented. The galaxy map, LRS scan, warp jump, fog of war, and turn counter are established, and the next gameplay slice can build on that stateful foundation.
