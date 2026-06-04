# DomiNox Architecture

## Folder Roles

- `Assets/_Project/Scripts/Core`: global constants and orchestration helpers.
- `Assets/_Project/Scripts/Data`: future data contracts and ScriptableObject-ready models.
- `Assets/_Project/Scripts/Dominoes`: domino definitions, instances, bag, hand, and factory.
- `Assets/_Project/Scripts/Grid`: logical grid state and placement models.
- `Assets/_Project/Scripts/Patterns`: centralized pattern detection.
- `Assets/_Project/Scripts/Scoring`: `Count x Mult` scoring service and result model.
- `Assets/_Project/Scripts/Run`: run and level state plus prototype flow controller.
- `Assets/_Project/Scripts/UI`: Unity views only. They display state and send player intents.
- `Assets/_Project/Scripts/Utilities`: shared low-level utilities and editor-only builders.

## Separation Rules

The domain model is independent from Unity UI. `GridState` owns placement validity. `PatternDetector` owns pattern detection. `ScoreCalculator` owns scoring. UI views never compute scores or decide win/loss.

Managers/controllers orchestrate dependencies and forward intent. They do not embed scoring rules, boss rules, DomiNex rules, or future shop logic.

## Future Growth

Phase 1 keeps empty folders for DomiNex, bosses, consumables, floors, prefabs, art, and audio so future phases can add data and handlers without mixing concerns.
