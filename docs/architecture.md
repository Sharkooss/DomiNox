# DomiNox Architecture

## Folder Roles

- `Assets/_Project/Scripts/Core`: constants globales (`GameConstants`), utilitaires bas niveau (`ChanceUtils`).
- `Assets/_Project/Scripts/Dominoes`: définitions de dominos, instances, sac (`DominoBag`), main (`HandState`), factory.
- `Assets/_Project/Scripts/Grid`: état logique de la grille 6×6, validation de placement.
- `Assets/_Project/Scripts/Patterns`: détection centralisée (`PatternDetector`), catalogue, combos, scaling par niveau.
- `Assets/_Project/Scripts/Scoring`: pipeline `Count × Mult` (`ScoreCalculator`), résultats, steps d'animation.
- `Assets/_Project/Scripts/Run`: état de run/niveau (`RunState`, `LevelState`), orchestrateur de flux (`GameFlowController`).
- `Assets/_Project/Scripts/Dominex`: définitions DomiNex, moteur d'effets (`DomiNexEffectEngine`), inventory, registry.
- `Assets/_Project/Scripts/Shop`: génération et logique du shop, packs, prix, reroll.
- `Assets/_Project/Scripts/Jackpot`: meter, spin tickets, récompenses par tiers.
- `Assets/_Project/Scripts/Bosses`: définitions boss, registry, état de niveau boss.
- `Assets/_Project/Scripts/Consumables`: Gem Tiles, inventory, registry.
- `Assets/_Project/Scripts/Persistence`: sauvegarde/chargement de la run en cours (`RunSaveService`, DTO + `RunSaveMapper`) et méta-progression cross-run (`MetaProfileService`). Sérialise des IDs + état mutable, jamais les définitions (reconstruites via les registries).
- `Assets/_Project/Scripts/Objectives`: objectifs de déblocage (`ObjectiveRegistry`, `ObjectiveService`), DomiNex verrouillés par défaut (`DomiNexUnlockService`) et données du carnet de collection (`CollectionService`). Un objectif accompli débloque un DomiNex (et, pour `le_schisme`, ouvre la Boucle 2 au-delà de l'étage 3).
- `Assets/_Project/Scripts/UI`: vues Unity uniquement. Affichent l'état, envoient les intentions du joueur.
- `Assets/_Project/Scripts/Utilities`: utilitaires partagés, `UiFactory`, `DomiNoxSceneBuilder` (editor only).

## Règles de séparation

Le domaine (scoring, patterns, effets) est indépendant de Unity UI.

- `GridState` possède la validation de placement.
- `PatternDetector` possède la détection de patterns.
- `ScoreCalculator` possède le pipeline de scoring.
- `DomiNexEffectEngine` possède tous les comportements DomiNex : scoring, post-scoring et propriétés passives.
- Les views UI n'appellent jamais `ScoreCalculator` directement — elles affichent ce que `GameFlowController` expose.

## GameFlowController

Rôle : orchestrateur fin. Reçoit les actions du joueur, délègue aux services, met à jour l'état de run.

**Ce qu'il ne fait pas :**
- Pas de règles de scoring inline.
- Pas d'IDs de DomiNex codés en dur (les comportements DomiNex passent par `DomiNexEffectEngine`).
- Pas de formules d'économie inline (délèguent à `RunEconomyService`).
- Pas de logique de boss (délèguent à `BossRegistry` et aux méthodes `ApplyBoss*`).

**Limite de taille :** si `GameFlowController` dépasse 400 lignes, extraire la responsabilité qui grossit dans un service dédié (pattern: `JackpotController`, `ShopController`…).

## Pipeline DomiNex — Comment ça marche

```
DomiNexRegistry.cs        → définit les effets (Scoring / PostScoring / Passive / LevelStart / RunStart)
DomiNexEffectEngine.cs    → exécute les effets au bon moment
GameFlowController.cs     → appelle l'engine aux bons moments du flux de run
```

Triggers disponibles :
- `Scoring` : appliqué pendant `ScoreCalculator.Calculate` via `ApplyScoringEffects`
- `PostScoring` : appliqué après validation via `RollPostScoringEffects` + `ApplyPostScoringResults`
- `Passive` : propriété lue à la demande via des méthodes dédiées (`GetMinimumCreditFloor`, etc.)
- `LevelStart` : appliqué au démarrage d'un niveau via `ApplyLevelStart`
- `RunStart` : appliqué au démarrage d'une run via `ApplyRunStart`

## Règle de dépendance

```
UI → GameFlowController → Services (ScoreCalculator, DomiNexEffectEngine, JackpotMeterService…) → Domain (Grid, Patterns, Dominoes)
```

L'infrastructure (save, storage) peut s'adapter aux besoins sans décider des règles de gameplay.

## Tests

Tous les tests sont des EditMode tests dans `Assets/_Project/Tests/EditMode/`.

- `PatternDetectorDesignTests.cs` : patterns, combos, shop, jackpot, bag, modifiers, economy.
- `DomiNoxEngineTests.cs` : comportements post-scoring DomiNex, credit floor, invariants du registry.

Pattern utilisé : réflexion via `AppDomain.CurrentDomain.GetAssemblies()` pour éviter une dépendance d'assembly directe.
