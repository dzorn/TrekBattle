# Governance

## Purpose

Track the rules, controls, approvals, and operational boundaries that govern TrekBattle.

## Product Governance

- Who can use the system: Anyone; anonymous play is allowed
- Who can administer the system: TBD
- Approval requirements: DevCraft specs require explicit approval before implementation
- Business rules: Galaxy is 12x12, each system is 12x12, no more than two enemy ships per system, and some systems may contain planets or bases
- Persistence rule: Create a session GUID at game start, display it to the player, provide an app-generated human-friendly resume code stored in PascalCase, and use either to restore the exact game state later. Player entry should be case-insensitive.

## Security Governance

- Sensitive data concerns: None identified yet
- Authentication requirements: None for player access
- Authorization requirements: TBD
- Audit and logging expectations: TBD

## AI Governance

- What AI may do directly: Draft documentation, analyze the repo, propose plans, and write code only after explicit approval
- What AI may draft only: Specs, plans, task lists, designs, and review notes
- What requires human approval: Any implementation work, especially new features and material changes
- What must never be automated: Bypassing approval gates or mutating approved scope without a new spec

## Engineering Governance

- Branching model: Simple Branching pending git initialization
- Release or deployment controls: TBD
- Testing requirements: Tests are required for completed work; UI work must include browser validation and screenshot review
- Documentation requirements: Project context files and feature specs must stay current

## Operational Governance

- Queue or background process requirements: None identified yet
- Failure handling rules: TBD
- Support or escalation notes: TBD

## Notes

This file will be refined as the product direction, architecture, and delivery workflow become clearer.
