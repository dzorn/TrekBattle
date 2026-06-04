# Feature Design: Game Startup

## Type

Feature

## Feature Name

**Game Startup**

## Status

In Design

## Work Item

- Work Item Source: None
- Work Item ID: N/A
- Work Item Title: N/A
- Work Item Summary: First feature for TrekBattle. Establish the Angular client, ASP.NET Core Minimal API, Aspire orchestration, SQL Server resource, and the game startup flow.

## Overview

Game Startup is the first playable slice of TrekBattle. It will let a player enter a name and ship name, create an anonymous game session, and transition into a launch screen that explains the mission and shows the player's recovery details.

This feature also establishes the application foundation:

- Angular client application
- ASP.NET Core Minimal API project
- Aspire AppHost for orchestration
- SQL Server resource for game state persistence

## Goals

- Let a player start a new game by entering a player name and ship name.
- Create an anonymous saved session with a player-entered resume code that can be resumed later.
- Show a launch screen with the mission description and recovery information.
- Set up the initial application stack for the project.
- Store session state in SQL Server through the API.

## Non-Goals

- Full combat gameplay
- Galaxy travel beyond the startup flow
- Authentication or account management
- Multiplayer support
- Complex visual effects or advanced animation

## Current State

The repository contains discovery documentation and no application code. The target application stack has been chosen, but the Angular client, API project, Aspire host, and database resource do not yet exist.

## Target State

The repository will contain a working Angular client, a minimal ASP.NET Core API, an Aspire AppHost with SQL Server orchestration, and a startup flow where a player can create a new session, choose a recovery code, and proceed to the launch screen.

## Requirements

- Requirement 1: The player must be able to enter a player name and ship name.
- Requirement 2: Starting a game must create an anonymous session record with a GUID and a player-entered human-friendly resume code.
- Requirement 3: The launch screen must show the mission description and the recovery code.
- Requirement 4: The recovery code must be usable later to restore the exact saved game state.
- Requirement 5: The solution must include an Angular client, a Minimal API, an Aspire AppHost, and SQL Server resource wiring.
- Requirement 6: The API must remain a single-project application with a very basic minimal API design.

## Brainstorming Notes

- The player does not authenticate.
- Session identity should be generated on game start and shown to the user immediately.
- The recovery code should be human-friendly and player-entered, while the GUID remains the underlying machine key.
- Resume must restore the exact saved state, not a checkpoint approximation.
- The UI should feel modern Angular rather than terminal-only, but still stay simple and readable.
- The project should use Aspire to manage local orchestration and the first infrastructure resource should be SQL Server.

## Codebase Analysis

- The repository was initially empty and then bootstrapped with DevCraft context documents.
- A local git repository now exists on `main`.
- No application projects, solution files, or source code have been added yet.

## New And Modified Views / Pages

- `Game Start` - `New`
  - Collect player name and ship name.
  - Collect the player-entered resume code.
  - Start a new anonymous game session.
  - Transition to the launch screen after creation.
- `Game Launch` - `New`
  - Show the mission description.
  - Show the session recovery code and session identity details.
  - Present the next-step entry point into the game.

## Module User Stories

### Client Setup and Game Start

#### Story 1: Start a New Game

- As a player
- I want to enter my name and ship name
- So that I can begin a new TrekBattle session

##### Happy Path Tests

- Player enters a valid name and ship name and starts a new game successfully.
- Player sees the launch screen after the session is created.

##### Edge Case Tests

- Player submits names with leading or trailing whitespace and the UI normalizes or validates them consistently.
- Player uses long but valid names and the form still behaves correctly.

##### Negative Tests

- Player leaves required fields blank and sees validation feedback.
- Player submits invalid input and the game does not start.

#### Story 2: View Launch Details

- As a player
- I want to see the mission and my recovery details after startup
- So that I understand the objective and can return later

##### Happy Path Tests

- Launch screen displays the mission description after a successful start.
- Launch screen shows the recovery code for the session.

##### Edge Case Tests

- Recovery code display remains readable on a narrow viewport.
- The launch screen still shows the correct mission and session details after refresh or revisit.

##### Negative Tests

- Launch screen does not hide the recovery code when the user needs it to resume later.
- Invalid or missing session data does not produce a broken launch screen.

### Infrastructure and Persistence

#### Story 1: Create the Orchestrated App Stack

- As a developer
- I want the Angular client, API, and Aspire host wired together
- So that the game can run locally with the correct infrastructure model

##### Happy Path Tests

- AppHost starts the client, API, and SQL Server resource successfully.
- The client can reach the API through the declared Aspire wiring.

##### Edge Case Tests

- AppHost resource naming stays consistent across the host and application projects.
- Local orchestration works when the API is restarted.

##### Negative Tests

- The API does not depend on hardcoded local ports.
- The solution does not require authentication for the startup path.

#### Story 2: Persist Session State

- As a player
- I want my game setup saved in SQL Server
- So that I can return later using my recovery code

##### Happy Path Tests

- A newly created session is written to SQL Server.
- A saved session can be retrieved using the recovery code and restores the same values.

##### Edge Case Tests

- The recovery code lookup works after the session is reloaded from persistence.
- Exact state is retained when the same session is reopened later.

##### Negative Tests

- An unknown recovery code does not restore a session.
- Corrupt or missing session data is handled gracefully.

## Plan

Describe how the feature will be implemented in phases.

### Phase 1: Solution and Orchestration Foundation

- Create the solution structure and initial projects for the Angular client, ASP.NET Core Minimal API, Aspire AppHost, service defaults, and shared Aspire constants.
- Configure SQL Server as the first Aspire-managed infrastructure resource.
- Establish the wiring needed for the client and API to run locally together.
- Expected outcome: the application stack can be launched locally in a basic but coherent form.

### Phase 2: Game Startup Flow

- Implement the game start screen in Angular.
- Collect player name and ship name.
- Create the session through the API and transition to the launch screen.
- Show the mission description and recovery code on the launch screen.
- Expected outcome: a player can create a session and see the details needed to return later.

### Phase 3: Session Persistence and Resume

- Persist the startup session to SQL Server.
- Implement resume lookup using the session GUID and recovery code.
- Restore the exact saved state when a session is resumed.
- Expected outcome: a player can return to the same saved game state later using the recovery code.

### Final Phase: Validation And Completion

- Prove the completed implementation end to end.
- Run all relevant automated tests successfully.
- Run Playwright and review screenshots for UI-affecting work.
- Confirm the feature is ready for handoff.

## Tasks

Define the implementation tasks for each phase. Tasks should be written as markdown checkboxes so progress can be tracked directly in the spec.
Test tasks must be written as individual test cases, not broad statements like "add coverage" or "test the workflow."

### Phase 1: Solution and Orchestration Foundation

#### Application Development

- [ ] Create the root solution and initial project structure for the Angular client, ASP.NET Core API, Aspire AppHost, service defaults, and shared Aspire constants.
- [ ] Configure Aspire AppHost wiring for the client, API, and SQL Server resource.
- [ ] Add the initial SQL Server resource and database wiring in the AppHost.
- [ ] Set up the basic solution references so the projects can run together locally.

#### Tests

- [ ] Verify the AppHost starts the declared resources successfully.
- [ ] Verify the API is reachable through Aspire wiring rather than hardcoded local ports.
- [ ] Verify the client project loads in the development stack.

### Phase 2: Game Startup Flow

#### Application Development

- [ ] Build the Angular game start form for player name and ship name entry.
- [ ] Build the Angular game start form for player name, ship name, and player-entered resume code entry.
- [ ] Build the Angular launch screen that shows the mission and recovery details.
- [ ] Implement the minimal API endpoints needed to create a new session.
- [ ] Return the session GUID and human-friendly recovery code from the API.
- [ ] Navigate the client from the start form to the launch screen after successful session creation.

#### Tests

- [ ] Verify a player can submit valid names and a player-entered resume code to create a new session.
- [ ] Verify required-field validation appears when the player leaves the name fields empty.
- [ ] Verify required-field validation appears when the player leaves the resume code field empty.
- [ ] Verify the launch screen displays the mission description after startup.
- [ ] Verify the launch screen displays the recovery code after startup.
- [ ] Verify the startup flow remains readable on a standard desktop viewport.

### Phase 3: Session Persistence and Resume

#### Application Development

- [ ] Persist the created game session to SQL Server.
- [ ] Add API support for looking up a saved session by recovery code or session GUID.
- [ ] Restore the exact saved state when a player resumes a session.
- [ ] Ensure the recovery code is human-friendly, player-entered, and stable for later use.

#### Tests

- [ ] Verify a newly created session is written to SQL Server.
- [ ] Verify the saved session can be restored using the recovery code.
- [ ] Verify the restored session matches the exact previously saved state.
- [ ] Verify an unknown recovery code does not restore a session.
- [ ] Verify the session still resumes correctly after the app is reloaded.

### Final Phase: Validation And Completion

#### Application Validation

- [ ] Run the relevant build/compile validation successfully.
- [ ] Run all relevant automated tests successfully.
- [ ] Run any required launch/run validation successfully.

#### UI Validation

- [ ] Run Playwright coverage for every UI-affecting change when the project supports it.
- [ ] Verify the expected rendered state after each meaningful UI action.
- [ ] Verify persisted state again after reload or revisit when persistence matters.
- [ ] Capture and review final screenshots of the affected UI before handoff.

## Acceptance Criteria

- [ ] A player can start a new game by entering a player name and ship name.
- [ ] Starting a game creates an anonymous session with a GUID and a player-entered human-friendly recovery code.
- [ ] The launch screen shows the mission description and recovery details.
- [ ] The session can be resumed later using the recovery code.
- [ ] The resumed session restores the exact saved state.
- [ ] The Angular client, minimal API, Aspire AppHost, and SQL Server resource are all set up and wired together.

## Testing

- Unit tests required
- UI tests required when applicable
- Each implementation phase should include explicit test tasks, not only coding tasks
- Test tasks should be listed as individual named test cases rather than broad coverage statements
- The final phase should always prove build, automated-test, and UI-validation completion before handoff
- Story-level tests should be grouped per module when practical
- For each story, capture happy path tests, edge case tests, and negative tests
