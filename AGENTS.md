# Agent Rules — DomiNox

## Avant de coder

1. Lire `docs/architecture.md` pour identifier la couche concernée.
2. Chercher si un pattern similaire existe déjà (`PatternDetector`, `DomiNexEffectEngine`, `ScoreCalculator`…).
3. Préférer l'extension propre à la duplication.
4. Ne pas modifier des fichiers non liés à la tâche.

## Règles absolues

### Scoring
- Aucune règle de scoring dans `Scripts/UI`. Les views affichent, elles ne calculent pas.
- `ScoreCalculator` est la seule source de vérité du pipeline `Count × Mult`.
- Tout bonus de Count/Mult d'un DomiNex passe par `DomiNexEffectEngine.ApplyScoringEffects`.

### DomiNex
- Ne jamais écrire `if (Run.DomiNexInventory.Contains("un_id"))` dans `GameFlowController` ou une scene.
- Tout comportement post-scoring d'un DomiNex (`ChancePatternLevelUp`, `ChanceDestroySelf`) se déclare via le trigger `PostScoring` dans `DomiNexRegistry.cs` et est exécuté par `DomiNexEffectEngine.RollPostScoringEffects` + `ApplyPostScoringResults`.
- Toute propriété passive d'un DomiNex (ex : plancher de crédit) se déclare via le trigger `Passive` dans `DomiNexRegistry.cs` et est lue par une méthode dédiée de `DomiNexEffectEngine`.
- Ajouter un nouveau DomiNex = 1 ligne dans `DomiNexRegistry.cs` + des effets déclaratifs. Aucun autre fichier à modifier sauf si le comportement nécessite un nouveau `DomiNexEffectType`.

### Boss
- Les règles de boss ne s'appliquent jamais depuis une scène Unity ni un composant UI.
- Les vérifications boss (`IsBannedByBoss`, `IsLockedByBoss`) restent dans `GameFlowController`, pas dans les views.

### GameFlowController
- Rôle : orchestrateur de flux. Il reçoit les intentions du joueur et délègue aux services (`ScoreCalculator`, `DomiNexEffectEngine`, `JackpotMeterService`, etc.).
- Il ne contient pas de règles métier. Pas de `if` sur des IDs de contenu, pas de formules de scoring, pas de calculs d'économie inline.
- Limite : si `GameFlowController` dépasse **400 lignes**, extraire la responsabilité concernée dans un service dédié (ex: `JackpotController`, `ShopController`).

### Contenu
- Tout contenu (DomiNex, boss, patterns) est défini dans un registry centralisé (`DomiNexRegistry`, `BossRegistry`, `PatternCatalog`).
- Aucun ID de contenu en dur hors de son registry d'appartenance.

## Avant chaque commit

```
1. Le projet compile sans erreur dans Unity.
2. Tous les tests EditMode passent (Unity Test Runner).
3. Aucun Console.Log / Debug.Log de debug dans le code runtime (sauf `Scripts/Core/DevMode.cs`).
4. Aucun nombre magique non nommé dans la présentation (utiliser `GameConstants`).
```

## Comment ajouter un nouveau DomiNex

1. Ajouter une ligne `Define(...)` dans `DomiNexRegistry.cs`.
2. Si l'effet est au scoring : utiliser `Scoring(DomiNexEffectType.X, ...)`. Si le type n'existe pas encore, l'ajouter dans `DomiNexEffectType.cs` et implémenter le `case` dans `DomiNexEffectEngine.ApplyScoringEffect`.
3. Si l'effet est post-scoring (chance de se déclencher après validation) : utiliser `PostScoring(DomiNexEffectType.ChancePatternLevelUp ou ChanceDestroySelf, ...)`.
4. Si l'effet est une propriété passive (modifie une limite, un seuil) : utiliser `Passive(DomiNexEffectType.X, ...)` et ajouter la lecture dans `DomiNexEffectEngine`.
5. Si aucun trigger existant ne convient : ajouter le trigger dans `DomiNexTrigger.cs`, le type dans `DomiNexEffectType.cs`, le handler dans `DomiNexEffectEngine`, et les tests correspondants.
6. Ajouter un test dans `DomiNoxEngineTests.cs` ou `PatternDetectorDesignTests.cs`.

## Comment ajouter un nouveau boss

1. Ajouter une entrée dans `BossRegistry.DemoBosses`.
2. Si le boss nécessite une nouvelle règle (`BossRuleType`) : ajouter la valeur d'enum, implémenter le comportement dans `GameFlowController.ApplyBossLevelStart` et/ou `ScoreCalculator.Calculate`, et tester.
3. Ne jamais appliquer les effets de boss depuis une scène ou un composant UI.

## Comment ajouter un nouveau pattern

1. Ajouter l'ID dans `PatternNames.cs`.
2. Ajouter la définition dans `PatternCatalog`.
3. Implémenter la détection dans `PatternDetector` (Value ou Design).
4. Ajouter les combos dans `PatternComboCatalog` si nécessaire.
5. Ajouter des tests dans `PatternDetectorDesignTests.cs`.

## Definition of Done

- Code compilant.
- Tests ajoutés ou mis à jour.
- Aucun ID de contenu codé en dur hors de son registry.
- Aucun `Debug.Log` restant hors `DevMode`.
- Documentation mise à jour si le comportement public change.
