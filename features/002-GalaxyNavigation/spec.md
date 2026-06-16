# Feature Design: Galaxy Navigation

## Type

Feature

## Feature Name

**Galaxy Navigation**

## Status

Draft

## Work Item

- Work Item Source: None
- Work Item ID: N/A
- Work Item Title: N/A
- Work Item Summary: Introduce the galaxy-level travel and map experience that lets a player inspect the galaxy, choose a destination, and move the ship between systems.

## Overview

Galaxy Navigation is the first gameplay feature beyond startup. The galaxy map is the primary view screen, and the player’s ship begins in a random sector on the 12x12 grid. The player can see the current ship position on the map, use a navigation console with a miniature galaxy grid, and click another valid grid location to initiate a jump.

The galaxy map uses fog of war until a sector has been visited. A Long Range Scan (LRS) reveals or updates the surrounding 3x3 area centered on the ship, which means the ship’s current sector plus the 8 adjacent sectors when they are in bounds. The edges of the 12x12 galaxy are hard boundaries for both movement and scanning.

The navigation console uses a jump range of 5, which produces an 11x11 navigation grid centered on the ship. Because ship classes are not being defined yet, this range is the default for the feature. Scan symbols are displayed as counts for the current sector using `K`, `P`, and `B` markers, such as `K0`, `P2`, and `B1`. Each sector tracks enemy count, planet count, base count, and whether it has been visited.

This feature focuses on the galaxy layer only. It does not introduce tactical system combat, ship power allocation, or detailed encounter resolution. The goal is to establish the strategic movement loop that connects startup to the broader game.

## Goals

- Let a player view the 12x12 galaxy map from the active session.
- Show the player’s current galaxy position clearly.
- Let the player inspect nearby galaxy information using the LRS concept.
- Let the player choose a destination system or sector and travel there.
- Persist the updated galaxy position in the active session state.
- Keep the navigation UI simple, readable, and easy to use.

## Non-Goals

- Tactical system navigation
- Combat resolution
- Ship system power allocation
- Docking, repairs, or detailed base interaction
- Multiplayer or shared galaxy state
- Advanced animation or complex visual effects
- Ship-class-specific travel limits or alternate navigation grid sizes

## Current State

The project already has discovery documentation for galaxy travel and a completed startup feature. Galaxy navigation itself is not yet implemented and has no dedicated design spec.

## Target State

The repository will contain a galaxy navigation experience that allows a player to open the galaxy map, inspect the current area, select a destination, and travel to another system while preserving the session state.

## Requirements

- Requirement 1: The galaxy map must represent the galaxy as a 12x12 grid.
- Requirement 2: The player’s ship must begin at a random valid sector on game start.
- Requirement 3: The UI must show the player’s current location on the galaxy map.
- Requirement 4: The navigation console must include a miniature galaxy grid with the ship’s current location displayed.
- Requirement 5: The player must be able to click another valid grid location to initiate a jump.
- Requirement 6: A Long Range Scan (LRS) must reveal or update the surrounding 3x3 area centered on the ship, when in bounds.
- Requirement 7: The galaxy map must use fog of war until a sector has been visited.
- Requirement 8: Traveling and scanning must respect the 12x12 galaxy edge as a hard boundary.
- Requirement 9: The navigation console must use a default jump range of 5, represented as an 11x11 grid centered on the ship.
- Requirement 10: Scan symbols must show sector counts using `K`, `P`, and `B` markers.
- Requirement 11: Each sector must track enemy count, planet count, base count, and visited state.
- Requirement 12: Traveling must update the saved session state to the new location.
- Requirement 13: The interface must stay simple and readable on a typical desktop viewport.
- Requirement 14: The feature must preserve the existing anonymous session model.

## Brainstorming Notes

- The galaxy view should feel like the strategic layer of the game, not the tactical combat grid.
- A concise map legend will likely be more useful than decorative visuals.
- The current location should stand out more than everything else on the map.
- Nearby scan data should help the player understand where to travel next without overwhelming the screen.
- Travel should return the player to a stable map state after arrival.
- If travel rules need to become more sophisticated later, this feature should still provide a clean base layer to build on.
- Future ship classes may get different travel limits and smaller visible navigation grids, but that is out of scope for this feature.

## Assumptions

- The player begins a game in a random valid sector within the galaxy bounds.
- The initial version will allow travel to a chosen galaxy destination without introducing combat or encounter branching during the move itself.
- Long-range scan information will be shown as galaxy-level context, not as tactical system detail.
- The player’s current location is the primary state change to preserve for this feature.
- If later rules require travel costs, those can be added as a follow-on feature rather than blocking the first version of navigation.
- LRS reveals the ship’s current sector plus adjacent sectors only when they exist inside the 12x12 grid.
- The default jump range of 5 implies an 11x11 navigation grid centered on the ship.
- Sector scan summaries should use the form `K#`, `P#`, and `B#`, where the number is the count for that sector.

## New And Modified Views / Pages

- `Galaxy Map` - `New`
  - Show the 12x12 galaxy grid.
  - Highlight the current location.
  - Show fog of war for unvisited sectors.
  - Surface nearby scan context.
  - Let the player choose a destination.
- `Navigation Console` - `New`
  - Show the 11x11 navigation grid.
  - Display the ship’s current location on the miniature grid.
  - Provide the click target for jump selection.
  - Show sector scan counts using `K`, `P`, and `B` symbols.
- `Travel Result` - `New`
  - Confirm the ship has arrived at the selected destination.
  - Show the updated location and any navigation summary.

## Module User Stories

### Client Navigation

#### Story 1: View the Galaxy Map

- As a player
- I want to see the galaxy map and my current location
- So that I can decide where to travel next

##### Happy Path Tests

- Player opens the galaxy map and sees the 12x12 grid.
- Player can identify the current position on the map.
- Player starts in a random valid sector and sees that location marked.

##### Edge Case Tests

- The galaxy map remains readable on a narrower desktop viewport.
- The current location remains visible when the map is refreshed.
- Fog of war remains in place for unvisited sectors.

##### Negative Tests

- The map does not hide the player location when scan data is missing.
- Invalid session state does not produce a broken map view.
- The ship does not appear outside the galaxy bounds.

#### Story 2: Inspect Nearby Galaxy Information

- As a player
- I want to inspect nearby galaxy information
- So that I can make an informed travel choice

##### Happy Path Tests

- Player can view nearby scan context from the galaxy screen.
- Player can distinguish nearby systems or sectors from unexplored or empty space.
- Player can see the 3x3 area revealed by LRS when the scan is in bounds.
- Player can see sector scan counts using `K`, `P`, and `B` symbols.

##### Edge Case Tests

- The scan display stays usable when multiple nearby items are present.
- The scan display remains clear after moving to a different location.
- Scanning at the edge only reveals sectors that exist inside the 12x12 grid.

##### Negative Tests

- Missing scan data does not prevent the galaxy map from rendering.
- The navigation screen does not require tactical system data to load.
- LRS does not reveal sectors outside the galaxy boundary.

#### Story 3: Travel to a Destination

- As a player
- I want to select a destination and travel there
- So that I can move through the galaxy

##### Happy Path Tests

- Player selects a valid destination and arrives at the new location.
- Player sees confirmation that the ship’s position changed.
- Player clicks a valid destination on the miniature grid to initiate jump selection.
- Player can select destinations anywhere within the 11x11 jump range when the location is in bounds.

##### Edge Case Tests

- Player changes destination selection before confirming travel.
- Player can revisit the galaxy map after arrival and see the updated position.
- A destination at the edge of the grid remains selectable if it is in bounds.

##### Negative Tests

- Player cannot travel outside the bounds of the galaxy.
- Invalid destinations do not update the saved session state.
- Clicking outside the galaxy grid does not initiate travel.

### Persistence and Session State

#### Story 1: Save the New Location

- As a player
- I want the game to remember my new galaxy position
- So that my session stays consistent after travel

##### Happy Path Tests

- The selected destination is written back to the session state.
- Reloading the session shows the same updated galaxy position.
- The set of visited sectors persists with the session state.
- Sector counts persist with the session state.

##### Edge Case Tests

- Travel state remains correct after a browser refresh.
- A resumed session opens at the last known galaxy location.
- Previously scanned sectors remain revealed after returning to the map.

##### Negative Tests

- Failed travel does not overwrite the previously saved position.
- Corrupt session data is handled gracefully.
- A failed scan does not mark out-of-bounds sectors as visited.

## Plan

Describe how the feature will be implemented in phases.

### Phase 1: Galaxy Map Foundation

- Define the galaxy map screen and navigation state needed to render the 12x12 grid.
- Add the current-location presentation, fog of war, and scan context display.
- Establish the client-side interaction model for choosing a destination from the 11x11 navigation console.
- Expected outcome: the galaxy map can be viewed as a strategic navigation screen.

### Phase 2: Travel Flow

- Implement destination selection and travel confirmation.
- Update the session’s galaxy position when travel succeeds.
- Show a simple travel result or arrival state after movement completes.
- Expected outcome: the player can move from one galaxy location to another.

### Phase 3: Persistence and Validation

- Persist the updated galaxy location in the active session state.
- Persist the visited-sector state for fog of war.
- Persist the sector counts for K, P, and B.
- Implement LRS reveal and update logic for the surrounding 3x3 area centered on the ship.
- Prevent invalid out-of-bounds travel.
- Confirm that a resumed session restores the last known galaxy position.
- Expected outcome: travel and scan state survive reloads and session restoration.

### Final Phase: Validation And Completion

- Prove the completed feature end to end.
- Run the relevant automated tests successfully.
- Verify the map and travel flow in the browser for layout and usability.
- Confirm the feature is ready for handoff.

## Tasks

Define the implementation tasks for each phase. Tasks should be written as markdown checkboxes so progress can be tracked directly in the spec.
Test tasks must be written as individual test cases, not broad statements like "add coverage" or "test the workflow."

### Phase 1: Galaxy Map Foundation

#### Application Development

- [ ] Create the galaxy map view and its supporting state model.
- [ ] Render the galaxy as a 12x12 grid.
- [ ] Show the player’s current location clearly on the map.
- [ ] Add fog of war for unvisited sectors.
- [ ] Add a nearby scan/context panel for galaxy-level navigation.
- [ ] Add an 11x11 navigation grid with the ship’s current location displayed.
- [ ] Show sector scan counts using `K`, `P`, and `B` markers.
- [ ] Add a destination selection interaction for the navigation console.

#### Tests

- [ ] Verify the galaxy map renders a 12x12 grid.
- [ ] Verify the current location is visible on the map.
- [ ] Verify the ship starts in a random valid sector.
- [ ] Verify fog of war hides unvisited sectors.
- [ ] Verify the nearby scan/context panel renders when data is present.
- [ ] Verify sector scan counts render using `K`, `P`, and `B` markers.

### Phase 2: Travel Flow

#### Application Development

- [ ] Implement destination confirmation for galaxy travel.
- [ ] Update the session state after a successful travel action.
- [ ] Show an arrival or travel result state after movement completes.
- [ ] Restrict movement to in-bounds galaxy destinations.
- [ ] Restrict destination selection to the 11x11 jump range.

#### Tests

- [ ] Verify a valid destination updates the current location.
- [ ] Verify the arrival state reflects the new galaxy position.
- [ ] Verify a player can return to the galaxy map after travel.
- [ ] Verify a click outside the galaxy grid does not initiate travel.
- [ ] Verify a destination outside the 11x11 jump range does not initiate travel.

### Phase 3: Persistence and Validation

#### Application Development

- [ ] Persist the updated galaxy location in the session record.
- [ ] Persist the visited-sector state for fog of war.
- [ ] Persist the sector counts for each sector.
- [ ] Reject out-of-bounds destinations.
- [ ] Implement LRS reveal and update logic for the surrounding 3x3 area centered on the ship.
- [ ] Restore the last known galaxy position when a session resumes.

#### Tests

- [ ] Verify an out-of-bounds destination is rejected.
- [ ] Verify a travel failure does not change the saved location.
- [ ] Verify a resumed session restores the latest galaxy position.
- [ ] Verify a refresh keeps the saved galaxy location intact.
- [ ] Verify LRS reveals only the in-bounds surrounding sectors.
- [ ] Verify LRS does not reveal sectors outside the 12x12 galaxy boundary.
- [ ] Verify the 11x11 navigation grid centers on the ship.
- [ ] Verify sector counts persist after reload.

### Final Phase: Validation And Completion

#### Validation

- [ ] Run the relevant automated test suite for galaxy navigation.
- [ ] Verify the galaxy navigation flow in the browser.
- [ ] Confirm the feature documentation matches the implemented behavior.
