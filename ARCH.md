# Architecture

## Purpose

Track the chosen architecture and important design decisions.

## Selected Architecture

- Primary Architecture: Angular SPA with ASP.NET Core API
- Supporting Architecture: .NET game simulation core and shared domain models

## Modules or Layers

- Module 1: Angular client
- Module 2: ASP.NET Core API
- Module 3: Game simulation and domain logic

## Integration Notes

- External systems: None identified yet
- Async processing: Likely minimal at first
- Data boundaries: Client talks to API; game state and rules should live server-side
- Session model: Anonymous session GUID acts as the persistence key for restoring game state, with a human-friendly resume code as a user-facing alias; resumes must restore the exact saved state

## Notes

The project will likely use .NET plus one or more web front ends, but the final structure has not been selected yet.
