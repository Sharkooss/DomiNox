# DomiNex

The first DomiNex implementation is data-driven and centralized under `Assets/_Project/Scripts/Dominex`.

## Current Scope

- 45 DomiNex definitions are registered in `DomiNexRegistry`.
- The prototype shop can offer DomiNex from the registry and adds bought DomiNex to the active inventory.
- Supported hooks currently include run start, level start, and scoring.
- Future hooks are declared as data but intentionally do nothing until the matching system exists.

## Architecture Rules

- UI displays active DomiNex but never applies their rules.
- `DomiNexEffectEngine` is the only place that applies DomiNex effects.
- `ScoreCalculator` delegates DomiNex scoring modifiers to the effect engine.
- New DomiNex should be added as definitions in the registry or future ScriptableObject data, not as scattered conditionals.
