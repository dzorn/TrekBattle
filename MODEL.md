# Data Model

## Purpose

Track the current database and domain model structure for TrekBattle.

## DDL

```sql
-- Galaxy is a 12x12 grid of sectors.
-- Each sector can contain a system, enemy ships, planets, or a base.
-- Each system is a 12x12 tactical grid.
-- Galaxy navigation also tracks visited sectors, turn count, and current ship position.
```

## Notes

- No tables have been defined yet.
- Likely core entities include galaxy sector, star system, ship, enemy ship, planet, base, scan result, power allocation, visited-sector state, and turn state.
- The first implemented galaxy navigation feature persists its map state inside the session record.
