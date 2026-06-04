# DomiNox

DomiNox is a roguelike scoring prototype inspired by Balatro, built around domino placement. The player draws a hand, places dominoes on a grid, scores with `Count x Mult`, and tries to beat a quota.

Current version: Phase 1 Core Prototype.

## Open The Project

Open this folder with Unity `6000.4.10f1` or a compatible Unity 6 editor.

## Launch The Prototype

Open `Assets/_Project/Scenes/MainMenu.unity` and press Play. Click `Jouer` to load `Assets/_Project/Scenes/Game.unity`.

## Playable Now

- Draw a starting hand of 7 dominoes.
- Select or drag a domino from the hand.
- Click dominoes in hand to select them for discard.
- Drop dominoes on a logical 6x6 grid with placement preview.
- Press `A` to rotate the dragged or selected domino 90 degrees left.
- Press `E` to rotate the dragged or selected domino 90 degrees right.
- Reset placed dominoes back to hand.
- Use `Discard selection` up to 3 times to replace selected hand dominoes.
- Prototype DomiNex loadout is active for testing modular scoring effects.
- Validate the level and compare the score to quota 80.
- See basic pattern detection and scoring breakdown.

## Phase 1 Placement Rules

- The first domino can be placed anywhere valid on the grid.
- Later dominoes must touch an existing domino orthogonally with at least one matching adjacent number.
- Out-of-grid and overlapping placements are refused.

## Not Implemented Yet

- Shop, DomiNex, consumables, bosses, full floors, save system, collection, daily challenge, final art, animation polish, audio, and advanced domino connection rules.
