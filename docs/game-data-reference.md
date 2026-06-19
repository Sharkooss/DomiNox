# DomiNox Game Data Reference

Ce fichier recense les valeurs editables actuelles du prototype. A mettre a jour a chaque changement dans `GameConstants`, `PatternCatalog`, `DomiNexRegistry` ou le scoring.

## Run / Level Values

| Valeur | Actuel | Source |
| --- | ---: | --- |
| Domino min value | 0 | `GameConstants.DominoMinValue` |
| Domino max value | 6 | `GameConstants.DominoMaxValue` |
| Grid width | 6 | `GameConstants.GridWidth` |
| Grid height | 6 | `GameConstants.GridHeight` |
| Starting hand size | 7 | `GameConstants.StartingHandSize` |
| Levels per floor | 5 | `GameConstants.LevelsPerFloor` |
| Classic levels per floor | 4 | `GameConstants.ClassicLevelsPerFloor` |
| Quota source | table + fallback | `LevelQuotaService` |
| Starting credits | 10 | `GameConstants.StartingCredits` |
| Starting DomiNex slots | 5 | `GameConstants.StartingDomiNexSlots` |
| Max consumable slots | 2 | `GameConstants.MaxConsumableSlots` |
| Max pattern level | 5 | `GameConstants.MaxPatternLevel` |
| Max placed dominoes | 5 | `GameConstants.PhaseOneMaxPlacedDominoes` |
| Discards | 3 | `GameConstants.PhaseOneDiscards` |
| Win credits | 5 | `GameConstants.LevelWinCredits` |
| Credits per remaining discard | 1 | `GameConstants.CreditsPerRemainingDiscard` |
| Interest step | 5 credits | `GameConstants.InterestCreditStep` |
| Max interest credits | 5 | `GameConstants.MaxInterestCredits` |

## Core Round Rules

Chaque niveau commence avec une main de 7 dominos, une limite de 5 dominos posables et 3 discards disponibles. Il n'y a pas d'action points et pas de bouton Draw. Le joueur peut defausser pour remplacer des dominos, poser jusqu'a la limite, puis valider une seule fois pour gagner ou perdre le niveau.

Le joueur commence avec 5 slots de DomiNex actifs. La limite est stockee dans `RunState.MaxDomiNexSlots` et appliquee au modele au moment de l'achat.

Les DomiNex actifs sont des instances dans des slots explicites via `DomiNexInventory.SlotInstances`. Chaque instance conserve son prix d'achat pour la revente. `DomiNexInventory.Active` reste une vue compacte des slots non vides, lue de gauche a droite par le scoring; aucun tri par rarete, id ou registry n'est applique.

Le joueur peut stocker 2 consommables Gem Tiles. Les Gem Tiles ne prennent pas de slot DomiNex et s'utilisent manuellement depuis la barre haute.

Les consommables sont aussi des instances dans `ConsumableInventory.ActiveInstances`. Une Gem Tile obtenue via Booster Pack a une valeur de vente par defaut de 1 credit.

## Quotas

Les quotas sont centralises dans `LevelQuotaService`. Chaque etage contient 4 niveaux classiques et 1 boss, donc `levelInFloor == 5` correspond toujours au boss.

| Etage | Table 1 | Table 2 | Table 3 | Table 4 | Boss |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 300 | 450 | 600 | 750 | 1200 |
| 2 | 2000 | 3000 | 4000 | 5000 | 7500 |
| 3 | 12000 | 18000 | 26000 | 36000 | 55000 |

Pour les etages non definis, le fallback prend le quota equivalent de l'etage precedent, multiplie par `2.2` pour les tables classiques et `2.3` pour les boss, puis arrondit a une valeur lisible.

Regles d'arrondi:

| Range | Step |
| --- | ---: |
| Sous 10000 | 100 |
| De 10000 a 100000 | 1000 |
| Au-dessus de 100000 | 5000 |

## Scoring Formula

`Final Score = ceil(Count x Mult x FinalScoreMultiplier)`

Flow actuel:

1. Base Count = somme des valeurs des dominos joues.
2. Base Mult = 1.
3. Detection du meilleur Value Pattern par priorite, avec `Tile High` en fallback.
4. Detection du meilleur Design Pattern par priorite si disponible.
5. Application des bonus de patterns avec scaling de niveau.
6. Application des DomiNex additifs.
7. Application des DomiNex multiplicatifs.

Apres validation, chaque pattern detecte est enregistre dans `RunState.PatternUsage`. Les compteurs sont propres a la run et repartent a zero au demarrage d'une nouvelle run.

Les DomiNex sont parcourus dans l'ordre actuel de `DomiNexInventory.Active`, qui correspond a l'ordre visible de gauche a droite. Si le joueur reorder les DomiNex, le scoring suivant utilise ce nouvel ordre.

## Pattern Levels

Les niveaux de patterns sont stockes dans `RunState.PatternLevels` et consultes via `DomiNexScoringContext.PatternLevels`.

Scaling centralise dans `PatternScalingService`:

| Valeur | Formule |
| --- | --- |
| Scaled Count | `round(BaseCount * (1 + 0.55 * (level - 1)))` |
| Scaled Mult | `BaseMult + floor((level - 1) * 1.25)` |

`Space Dominex` a 1 chance sur 4 d'augmenter de 1 le niveau du Value Pattern principal joue apres validation.

## Value Patterns

| Id | Pattern | Requirement | Count | Mult | Priority |
| --- | --- | --- | ---: | ---: | ---: |
| `tile_high` | Tile High | Fallback si aucun autre Value Pattern n'est reconnu. | 5 | 1 | 0 |
| `low_tile` | Low Tile | Tous les dominos joues ont une somme <= 5. | 10 | 2 | 10 |
| `high_tile` | High Tile | Tous les dominos joues ont une somme > 8. | 6 | 2 | 20 |
| `double_tile` | Double Tile | Au moins 2 doubles sont joues. | 10 | 3 | 30 |
| `triple_double` | Triple double | Au moins 3 doubles sont joues. | 20 | 3 | 40 |
| `small_tile_straight` | Small Tile Straight | 3 dominos connectes forment une suite exacte. | 30 | 4 | 50 |
| `long_tile_straight` | Long Tile Straight | 5 dominos connectes forment une suite exacte. | 60 | 7 | 60 |
| `jackpot_7` | Jackpot 7 | Au moins 3 dominos joues ont une somme egale a 7. | 50 | 4 | 70 |

## Design Patterns

| Id | Pattern | Requirement | Count | Mult | Priority |
| --- | --- | --- | ---: | ---: | ---: |
| `big_loop` | Big Loop | Grande boucle fermee et connectee formant un carre 4x4. | 100 | 6 | 100 |
| `tile_line` | Tile Line | Au moins 3 dominos connectes sur une ligne horizontale ou verticale. | 5 | 2 | 20 |
| `cross_tile` | Cross Tile | Au moins 4 dominos forment une vraie croix autour d'un centre clair. | 10 | 2 | 40 |
| `tile_loop` | Tile Loop | Les dominos forment une boucle fermee avec connexions valides. | 30 | 3 | 80 |

Design detection est stricte et basee sur les connexions valides entre dominos, pas sur la simple proximite visuelle.

Priorite de detection:

1. `big_loop`
2. `tile_loop`
3. `christ_cross`
4. `cross_tile`
5. `tile_line`

`Big Loop` est un Design Pattern secret. Il est reconnu quand une loop valide forme strictement le contour d'une bounding box 4x4, avec interieur vide. Il donne `+100 Tile, +6 Mult`, priorite 100, et reste cache tant qu'il n'a pas ete joue une premiere fois pendant la run.

`Tile Loop` est reconnu quand les dominos forment un cycle ferme valide d'au moins 4 dominos. Chaque domino de la boucle doit avoir exactement deux voisins utiles dans le graphe de connexions valides. Une connexion visuelle dont les valeurs ne correspondent pas ne compte pas.

`Cross Tile` est reconnu seulement s'il existe un domino central connecte dans au moins 3 directions distinctes parmi haut, bas, gauche, droite. Une ligne, un angle, une boucle ou une forme compacte sans centre clair ne peut pas etre `Cross Tile`.

`Christ Cross` est un Design Pattern secret. Il est reconnu quand une vraie `Cross Tile` possede un axe vertical dominant: depuis le centre, il faut une branche gauche, une branche droite, une connexion verticale en haut et/ou en bas totalisant au moins 2 dominos, et l'axe vertical en comptant le centre doit etre plus long que l'axe horizontal. Une ligne verticale seule ne peut jamais etre `Christ Cross`. Il donne `+50 Tile, +4 Mult`, priorite 60, et reste cache dans Run Info tant qu'il n'a pas ete joue une premiere fois pendant la run.

| Id | Pattern | Secret | Requirement | Count | Mult | Priority |
| --- | --- | --- | --- | ---: | ---: | ---: |
| `big_loop` | Big Loop | Yes | Grande boucle fermee et connectee formant un carre 4x4. | 100 | 6 | 100 |
| `christ_cross` | Christ Cross | Yes | Croix prolongee avec centre clair et branche verticale etendue. | 50 | 4 | 60 |

Tests de non-regression couverts en EditMode:

1. Une boucle valide detecte `tile_loop`, jamais `cross_tile`.
2. Une boucle avec connexion invalide ne detecte pas `tile_loop`.
3. Une croix minimale detecte `cross_tile`.
4. Une ligne et une forme en L ne detectent pas `cross_tile`.
5. Une croix verticalement prolongee detecte `christ_cross`.

## Pattern Combos

Les combos sont centralises dans `PatternComboCatalog`. Quand le scoring detecte a la fois un Value Pattern et un Design Pattern, le catalogue cherche une combinaison exacte via `TryGetCombo(valuePatternId, designPatternId, out combo)`.

Le combo ne remplace pas les patterns: il ajoute un bonus fixe supplementaire apres les bonus Value et Design scales.

Formule preview combo:

`Combo Count = ScaledValuePatternCount + ScaledDesignPatternCount + ComboCountBonus`

`Combo Mult = ScaledValuePatternMult + ScaledDesignPatternMult + ComboMultBonus`

Pendant l'animation de scoring, l'ordre est: Value Pattern, Design Pattern, Pattern Combo, dominos, boss, DomiNex additifs, DomiNex multiplicatifs, score final, effets post-scoring.

| Combo | Value Pattern | Design Pattern | Bonus Tile | Bonus Mult | Difficulte |
| --- | --- | --- | ---: | ---: | --- |
| High Line | `tile_high` | `tile_line` | 10 | 1 | Easy |
| Low Line | `low_tile` | `tile_line` | 20 | 2 | Easy |
| High Roll Line | `high_tile` | `tile_line` | 25 | 2 | Easy |
| Double Line | `double_tile` | `tile_line` | 35 | 3 | Medium |
| Triple Line | `triple_double` | `tile_line` | 60 | 4 | Medium |
| Small Straight Line | `small_tile_straight` | `tile_line` | 50 | 4 | Medium |
| Long Straight Line | `long_tile_straight` | `tile_line` | 100 | 6 | Hard |
| Jackpot Line | `jackpot_7` | `tile_line` | 80 | 5 | Medium |
| High Cross | `tile_high` | `cross_tile` | 25 | 2 | Medium |
| Low Cross | `low_tile` | `cross_tile` | 40 | 3 | Medium |
| High Roll Cross | `high_tile` | `cross_tile` | 45 | 3 | Medium |
| Double Cross | `double_tile` | `cross_tile` | 70 | 5 | Hard |
| Triple Cross | `triple_double` | `cross_tile` | 110 | 7 | Hard |
| Small Straight Cross | `small_tile_straight` | `cross_tile` | 90 | 6 | Hard |
| Long Straight Cross | `long_tile_straight` | `cross_tile` | 160 | 9 | VeryHard |
| Jackpot Cross | `jackpot_7` | `cross_tile` | 130 | 8 | Hard |
| High Loop | `tile_high` | `tile_loop` | 50 | 4 | Hard |
| Low Loop | `low_tile` | `tile_loop` | 70 | 5 | Hard |
| High Roll Loop | `high_tile` | `tile_loop` | 80 | 5 | Hard |
| Double Loop | `double_tile` | `tile_loop` | 120 | 8 | VeryHard |
| Triple Loop | `triple_double` | `tile_loop` | 180 | 11 | VeryHard |
| Small Straight Loop | `small_tile_straight` | `tile_loop` | 150 | 9 | VeryHard |
| Long Straight Loop | `long_tile_straight` | `tile_loop` | 250 | 14 | Legendary |
| Jackpot Loop | `jackpot_7` | `tile_loop` | 220 | 12 | Legendary |
| High Christ Cross | `tile_high` | `christ_cross` | 60 | 5 | Hard |
| Low Christ Cross | `low_tile` | `christ_cross` | 85 | 6 | Hard |
| High Roll Christ Cross | `high_tile` | `christ_cross` | 95 | 6 | Hard |
| Double Christ Cross | `double_tile` | `christ_cross` | 130 | 9 | VeryHard |
| Triple Christ Cross | `triple_double` | `christ_cross` | 190 | 12 | VeryHard |
| Small Straight Christ Cross | `small_tile_straight` | `christ_cross` | 160 | 10 | VeryHard |
| Long Straight Christ Cross | `long_tile_straight` | `christ_cross` | 280 | 16 | Legendary |
| Jackpot Christ Cross | `jackpot_7` | `christ_cross` | 250 | 15 | Legendary |
| High Big Loop | `tile_high` | `big_loop` | 120 | 8 | VeryHard |
| Low Big Loop | `low_tile` | `big_loop` | 150 | 9 | VeryHard |
| High Roll Big Loop | `high_tile` | `big_loop` | 170 | 9 | VeryHard |
| Double Big Loop | `double_tile` | `big_loop` | 220 | 12 | Legendary |
| Triple Big Loop | `triple_double` | `big_loop` | 300 | 16 | Legendary |
| Small Straight Big Loop | `small_tile_straight` | `big_loop` | 260 | 14 | Legendary |
| Long Straight Big Loop | `long_tile_straight` | `big_loop` | 420 | 22 | Mythic |
| Jackpot Big Loop | `jackpot_7` | `big_loop` | 380 | 20 | Mythic |

Le lookup des combos utilise strictement la paire exacte `ValuePatternId + DesignPatternId`. Il n'existe aucun fallback de `christ_cross` vers `cross_tile`; si le design detecte est `christ_cross`, le combo ne peut jamais etre `High Cross`, `Low Cross` ou `Jackpot Cross`.

## Run Info UI

Le bouton `Run Info` dans le panneau gauche ouvre la popup des patterns. Les onglets Value et Design affichent le niveau actuel de chaque pattern via `RunState.PatternLevels`. Les niveaux commencent a 1 et se mettent a jour a la prochaine ouverture de la popup, notamment apres un level-up de `Space Dominex` ou une Gem Tile.

Run Info n'affiche pas les valeurs base du catalogue si un pattern est niveau 2 ou plus. Pour chaque pattern visible, l'UI recalcule `+Tile, +Mult` avec `PatternScalingService.GetScaledPatternBonus(pattern, currentLevel)`.

L'onglet `Combos` affiche les 24 combos, leur difficulte et leurs bonus fixes. Le bloc pattern du panneau gauche affiche le combo detecte en priorite si une combinaison existe, sinon les patterns detectes normalement.

L'onglet `Floor` affiche l'etage courant avec les 4 tables classiques et le boss. Les quotas viennent de `LevelQuotaService.GetQuotaForGlobalLevel`, donc Run Info utilise la meme source que le gameplay. Chaque ligne indique l'etat `Cleared`, `Current` ou `Upcoming`, le quota, et la regle de la table. Le boss final de l'etage utilise `RunState.CurrentFloorBoss`.

Le sac de la colonne droite est un indicateur compact `Sac` + compteur. La tooltip boss est positionnee pres du label boss et clamp dans l'ecran.

## Visual Feedback / VFX Hooks

La premiere passe de polish visuel reste legere et centralisee dans l'UI:

| Service | Role |
| --- | --- |
| `FloatingTextService` | Textes flottants `Tile`, `Mult`, multiplicateurs, credits, warnings. |
| `FeedbackService` | Pulse, shake, flash ecran leger, petits bursts casino/chips. |

`FeedbackService.ReducedMotion` existe comme hook interne pour reduire les effets plus tard.

Hooks actuels:

1. Hover domino en main: scale up discret et contour clair.
2. Domino selectionne pour discard: couleur jaune/orange et contour plus fort.
3. Drag domino: ghost existant + preview verte/rouge de grille.
4. Scoring domino: pulse, shake, floating `+X Tile`, burst bleu.
5. Scoring Mult: floating `+X Mult`, burst rouge.
6. Multiplicateur final: floating violet/dore et flash leger.
7. Combo: floating `COMBO!`, pulse plus fort, burst dore.
8. Score final: roll-up rapide, puis `CLEARED` ou `FAILED` avec flash/burst distinct.
9. Secret pattern: toast flottant `Secret Pattern Discovered` avec flash violet/dore avant revelation officielle.
10. DomiNex post-scoring: pulse/shake sur la carte; `Gros Michel` affiche un feedback special s'il casse.
11. Messages shop/use/sell: floating credits, warnings ou upgrade selon le message de controller.

Ces effets ne changent aucune regle de scoring et ne debloquent aucun contenu secret.

## Multi-Hand Levels

Chaque niveau utilise maintenant un score cumule sur 3 mains au lieu d'une seule validation decisive.

Etat de niveau:

| Champ | Valeur initiale | Role |
| --- | ---: | --- |
| `MaxHands` | 3 | Nombre maximum de mains jouables. |
| `HandsRemaining` | 3 | Mains encore disponibles. |
| `MaxDiscards` | 3 | Discards du niveau, modifiable par boss/rewards. |
| `DiscardsRemaining` | 3 | Discards encore disponibles. |
| `CurrentScore` | 0 | Score cumule du niveau. |
| `Quota` | table de quotas existante | Score cumule a atteindre. |

Validation d'une main:

1. Le scoring existant calcule le score de la main courante avec patterns, combos, boss et DomiNex.
2. `CurrentScore += HandScore`.
3. `HandsRemaining -= 1`.
4. Les dominos poses partent dans `PlayedThisLevel` et ne reviennent pas dans la pioche du niveau.
5. Si `CurrentScore >= Quota`, le niveau se termine immediatement en victoire.
6. Si `HandsRemaining <= 0` et le quota n'est pas atteint, le niveau est perdu.
7. Sinon la grille est videe, les dominos non joues de la main sortent du cycle du niveau, puis une nouvelle main est piochee depuis le sac actif.

Discards:

1. Les discards ne se reset pas entre les mains d'un meme niveau.
2. Un discard retire les dominos selectionnes de la main et les ajoute a `DiscardedThisLevel`.
3. Les dominos discardes ne reviennent pas dans le sac actif avant le prochain niveau.
4. Le jeu pioche autant de remplacements que possible depuis le sac actif; si le sac est vide, aucun crash et la main peut devenir plus petite.

Sac permanent vs sac actif:

1. Le sac permanent de run est le set double-six plus `RunState.PurchasedDominoes`.
2. Au debut de chaque niveau, `CreateRunDominoSet()` cree une nouvelle liste d'instances depuis ce sac permanent.
3. `Run.Bag` est le sac actif du niveau: piocher retire les dominos du niveau, sans reshuffle automatique des discardes ou joues.
4. Les achats de dominos en shop modifient le sac permanent et apparaissent naturellement au niveau suivant.

UI:

1. Le panel gauche affiche `CurrentScore / Quota`.
2. Il affiche `HandsRemaining / MaxHands`, `DiscardsRemaining / MaxDiscards`, `Placed / MaxPlacedDominoes` et le nombre de dominos restants dans le sac actif.
3. Pendant l'animation, le score affiche le score de main puis le total projete du niveau.

Jackpot/Boss/DomiNex:

1. Les gains Jackpot de main sont appliques a chaque validation.
2. Les bonus Jackpot de depassement 50% et 100% utilisent le score cumule apres la main et sont attribues une seule fois par niveau via `AwardedQuotaOver50JackpotGain` et `AwardedQuotaOver100JackpotGain`.
3. Les boss restent actifs sur tout le niveau. `LockHandDominoes` verrouille seulement la main de depart pour cette premiere version.
4. Les DomiNex de scoring s'appliquent a chaque main validee; les post-scoring comme `Gros Michel` et `Space Dominex` peuvent trigger apres chaque main.

## Domino Modifiers

Un `DominoInstance` peut avoir un `ModifierId`. `null` ou vide signifie domino normal. Un domino modifie garde ses valeurs normales, par exemple `2|5`, mais applique un effet centralise par `DominoModifierEffectResolver`.

Modificateurs disponibles, sans autres modificateurs pour l'instant:

| Id | Nom | Effet |
| --- | --- | --- |
| `blue` | Blue Domino | Quand score: +15 Tile. |
| `red` | Red Domino | Quand score: +3 Mult. |
| `gold` | Gold Domino | Si joue pendant un niveau gagne: +2 credits en fin de niveau. |
| `glass` | Glass Domino | Quand score: x2 score de main. Apres scoring: 1 chance sur 4 de casser. |
| `lucky` | Lucky Domino | Rolls independants: 1/30 pour +1 Spin +100 Tile +10 Mult, 1/10 pour +30 Mult, 1/3 pour +5 Mult. |
| `jackpot` | Jackpot Domino | Quand score: +10 Jackpot Meter via `JackpotMeterService.AddJackpotMeter`. |
| `light` | Light Domino | Ne compte pas dans la limite de pose, mais donne -2 Mult quand score. |

Ordre scoring actuel:

1. Detection Value Pattern.
2. Detection Design Pattern.
3. Detection Combo.
4. Valeur de base des dominos.
5. Effets de modificateurs de dominos.
6. Effets boss.
7. DomiNex additifs puis multiplicatifs.
8. Score final.
9. Effets post-scoring: Jackpot modifier, Lucky spin, Glass break, Gold fin de niveau gagne.

Interactions multi-mains:

1. Blue, Red, Glass, Lucky, Jackpot et Light s'appliquent sur chaque main ou le domino est joue et score.
2. Les Gold Dominoes sont comptes dans `PlayedThisLevel`; s'ils ont ete joues en main 1 et que le niveau est gagne en main 3, ils donnent quand meme +2 credits chacun.
3. Glass utilise `ChanceUtils.RollChance(1, 4)`. Si le domino casse, son `InstanceId` est ajoute a `RunState.DestroyedDominoInstanceIds` et il est filtre du sac permanent au prochain niveau.
4. Lucky utilise `ChanceUtils.RollChance` pour ses trois rolls independants, donc les bonus peuvent se cumuler.
5. Light occupe toujours physiquement des cases, respecte les connexions, compte pour les patterns et le scoring, mais `GameFlowController.CountPlacedAgainstLimit` l'ignore pour la limite de pose.

Visuel:

1. Les dominos modifies ont une teinte et un badge court dans `DominoView` et `BagPanelView`.
2. Les steps de scoring `DominoModifier` animent leurs floating texts apres la valeur de base du domino.
3. Glass affiche un multiplicateur `x2`; Jackpot affiche `+10 Jackpot`; Lucky gros hit utilise le texte `LUCKY HIT!`.

## Bosses Demo

Les boss apparaissent tous les 5 niveaux. Le boss d'etage est choisi a l'avance pour etre visible dans `FloorProgressView`, en evitant de reprendre le meme boss deux fois de suite quand possible.

| Boss | Rule Type | Effet |
| --- | --- | --- |
| Le Croupier Manchot | BannedValue | Une valeur entre 0 et 6 est bannie. Les dominos contenant cette valeur ne peuvent pas etre poses. |
| La Table Rouge | ModifyDiscards | Le niveau commence avec 1 discard en moins. |
| La Veuve des Doubles | DisablePatterns | Les patterns `double_tile` et `triple_double` ne donnent aucun bonus. |
| La Machine 777 | JackpotBoost | Chaque domino de somme 7 donne +7 Count et +1 Mult. `jackpot_7` donne x1.25 score final. |
| Le Sabot Verrouille | LockHandDominoes | 2 dominos de la main de depart sont verrouilles et ne peuvent pas etre defausses. |

## DomiNex

| Id | Name | Rarity | Current Effect |
| --- | --- | --- | --- |
| `dominex` | Dominex | Common | +4 Mult. |
| `low_dominex` | Low Dominex | Common | Si tous les dominos joues ont une somme <= 5, +8 Mult. |
| `silly_dominex` | Silly Dominex | Common | Si tous les dominos joues ont une somme <= 5, +50 Tile. |
| `high_dominex` | High Dominex | Common | Si tous les dominos joues ont une somme > 8, +6 Mult. |
| `willy_dominex` | Willy Dominex | Common | Si tous les dominos joues ont une somme > 8, +40 Tile. |
| `twin_dominex` | Twin Dominex | Common | Si au moins 2 doubles sont joues, +10 Mult. |
| `niwt_dominex` | Niwt Dominex | Common | Si au moins 2 doubles sont joues, +80 Tile. |
| `triplet_dominex` | Triplet Dominex | Common | Si au moins 3 doubles sont joues, +12 Mult. |
| `telprit_dominex` | Telprit Dominex | Common | Si au moins 3 doubles sont joues, +100 Tile. |
| `straight_dominex` | Straight Dominex | Common | Si Small Tile Straight est joue, +70 Tile. |
| `long_straight_dominex` | Long Straight Dominex | Common | Si Long Tile Straight est joue, +20 Mult. |
| `jacko_7even_dominex` | Jack'o 7even Dominex | Common | Chaque domino joue de somme 7 donne +30 Tile et +3 Mult. |
| `line_dominex` | Line Dominex | Common | Si Tile Line est joue, +60 Tile. |
| `cross_dominex` | Cross Dominex | Common | Si Cross Tile est joue, +80 Tile. Si la croix est prolongee, +10 Mult par extension. |
| `looping_dominex` | Looping Dominex | Common | Si Tile Loop est joue, +100 Tile. |
| `looper_dominex` | Looper Dominex | Common | Si Tile Loop est joue, +12 Mult. |
| `half_dominex` | Half Dominex | Common | Si 3 dominos ou moins sont places, +20 Mult. |
| `reroll_dominex` | Reroll | Common | Le premier reroll de chaque shop est gratuit. Hook futur. |
| `mystic` | Mystic | Common | Si tu valides avec 0 discard restant, +15 Mult. |
| `shallow` | Shallow | Common | +5 Mult par discard restant. |
| `credit_dominex` | Credit Dominex | Common | Tu peux acheter si l'achat te laisse a -20 credits ou plus. |
| `gros_michel` | Gros Michel | Common | +15 Mult. Apres chaque main jouee, 1 chance sur 6 de detruire ce DomiNex. |
| `even_dominex` | Even Dominex | Common | Chaque domino joue contenant uniquement des valeurs paires donne +4 Mult. |
| `odd_dominex` | Odd Dominex | Common | Chaque domino joue contenant uniquement des valeurs impaires donne +30 Tile. |
| `scholar` | Scholar | Common | Chaque domino joue contenant un 0 donne +20 Tile et +4 Mult. |
| `fibonacci` | Fibonacci | Rare | Chaque domino joue contenant 1, 3 ou 5 donne +5 Mult. |
| `empty_dominex` | Empty Dominex | Rare | Mult x nombre de slots DomiNex libres. |
| `doublish` | Doublish | Rare | Score final x1 + 0.2 par double dans ton sac. |
| `space_dominex` | Space Dominex | Rare | 1 chance sur 4 d'ameliorer le niveau du Value Pattern joue de +1. |

## Consumables / Gem Tiles

Les Gem Tiles sont des consommables separes des DomiNex. Elles sont centralisees dans `GemTileRegistry`, stockees dans `RunState.Consumables`, et utilisent `PatternLevelState` pour augmenter le niveau du pattern cible de +1.

Regles:

1. Pas de rarete.
2. Maximum 2 consommables stockes.
3. Utilisation manuelle via le bouton `Use` dans la barre haute.
4. Une Gem Tile utilisee est retiree de l'inventaire.
5. Un pattern ne peut pas depasser `GameConstants.MaxPatternLevel` (5).
6. Une Gem Tile dont le pattern cible est deja niveau max n'est plus proposee par les Booster Packs.
7. Les achats de Booster Packs respectent `Credit Dominex`: sans lui credits >= prix, avec lui credits - prix >= -20.
8. Une Gem Tile selectionnee affiche une carte de detail avec cible, niveau actuel, niveau apres utilisation, bouton `Use` et bouton `Sell $X`.
9. Si le pattern cible est niveau max, `Use` affiche `Max Level` et reste desactive.
10. Vendre une Gem Tile retire l'instance sans appliquer son effet.

Valeur de vente consommable:

`SellValueService.GetConsumableSellValue(instance) = max(1, instance.SellValue)`

Pour la version actuelle, les Gem Tiles issues des Booster Packs ont `SellValue = 1`.

| Id | Name | Target Pattern | Price | Effect |
| --- | --- | --- | ---: | --- |
| `quartz_tile` | Quartz Tile | `tile_high` | 3 | Upgrade Tile High by 1 level. |
| `sapphire_tile` | Sapphire Tile | `low_tile` | 4 | Upgrade Low Tile by 1 level. |
| `ruby_tile` | Ruby Tile | `high_tile` | 4 | Upgrade High Tile by 1 level. |
| `opal_tile` | Opal Tile | `double_tile` | 5 | Upgrade Double Tile by 1 level. |
| `amethyst_tile` | Amethyst Tile | `triple_double` | 6 | Upgrade Triple Double by 1 level. |
| `topaz_tile` | Topaz Tile | `small_tile_straight` | 5 | Upgrade Small Tile Straight by 1 level. |
| `emerald_tile` | Emerald Tile | `long_tile_straight` | 8 | Upgrade Long Tile Straight by 1 level. |
| `diamond_tile` | Diamond Tile | `jackpot_7` | 7 | Upgrade Jackpot 7 by 1 level. |
| `onyx_tile` | Onyx Tile | `tile_line` | 4 | Upgrade Tile Line by 1 level. |
| `jade_tile` | Jade Tile | `cross_tile` | 6 | Upgrade Cross Tile by 1 level. |
| `obsidian_tile` | Obsidian Tile | `tile_loop` | 8 | Upgrade Tile Loop by 1 level. |
| `garnet_tile` | Garnet Tile | `christ_cross` | 9 | Upgrade Christ Cross by 1 level. |
| `lapis_tile` | Lapis Tile | `big_loop` | 10 | Upgrade Big Loop by 1 level. |

`Garnet Tile` est secrete: elle n'apparait pas dans le shop tant que `christ_cross` n'a pas ete revele pendant la run. Apres revelation, elle suit les memes regles que les autres Gem Tiles et disparait si `Christ Cross` est au niveau max.

`Lapis Tile` est secrete: elle n'apparait pas dans le shop tant que `big_loop` n'a pas ete revele pendant la run. Apres revelation, elle suit les memes regles que les autres Gem Tiles et disparait si `Big Loop` est au niveau max.

## Visibility / Dev Mode

La visibilite des contenus secrets est centralisee dans `CollectableVisibilityService`, avec des methodes separees pour la Collection et la run.

Methodes publiques principales:

1. Collection/dev mode: `IsPatternVisible`, `IsComboVisible`, `IsConsumableVisible`.
2. Run Info: `IsPatternVisibleInRunInfo`, `IsComboVisibleInRunInfo`.
3. Run shop/packs: `IsConsumableVisibleInRun`, `CanGemTileAppearInPack`, `CanGemTileAppearInShop`.

Regles:

1. Collection avec `DevMode.Enabled == true`: patterns, combos, consommables et collectables sont visibles meme s'ils sont secrets ou non reveles.
2. Run Info ignore le dev mode: un pattern secret est visible seulement s'il est dans `RunState.RevealedSecretPatterns`.
3. Run Info ignore le dev mode: un combo utilisant un Design Pattern secret est visible seulement si ce Design Pattern est revele dans la run.
4. Run shop/packs ignorent le dev mode: une Gem Tile ciblant un pattern secret est visible/proposable seulement si ce pattern est revele dans la run.

Rappels secrets:

1. `Christ Cross` reste cache tant que `christ_cross` n'est pas revele dans la run.
2. `Big Loop` reste cache tant que `big_loop` n'est pas revele dans la run.
3. Les combos Christ Cross restent caches tant que Christ Cross n'est pas revele.
4. Les combos Big Loop restent caches tant que Big Loop n'est pas revele.
5. `Garnet Tile` reste cachee/proscrite des packs tant que Christ Cross n'est pas revele.
6. `Lapis Tile` reste cachee/proscrite des packs tant que Big Loop n'est pas revele.
7. En dev mode, tout est visible dans la Collection; Run Info et les packs/shop de la run restent bases sur les secrets reveles dans la run.

La Collection possede un onglet `Consumables` listant les Gem Tiles visibles selon la logique Collection. Run Info et Shop/Packs utilisent la logique de run, qui ignore le dev mode pour les secrets.

Preview visuelle:

1. `Christ Cross` utilise un diagramme large 6x5 pour afficher sa croix prolongee sans rognage.
2. `Big Loop` utilise un diagramme de contour 4x4 distinct de `Tile Loop`.

## Shop Notes

Le shop actuel genere 3 offres principales. Chaque slot principal est tire independamment: 90% DomiNex, 10% Domino, avec un maximum de 1 Domino direct par shop. Les DomiNex sont ponderes par rarete et excluent les DomiNex deja possedes. Les prix DomiNex sont generes a la creation de l'offre et restent stables jusqu'au reroll.

| Rarity | Prix |
| --- | --- |
| Common | 1-6, poids fort sur 4 |
| Rare | 4-8, poids fort sur 6 |
| Epic | 7-10, poids fort sur 8-9 |
| Legendary | 20 fixe |

Layout shop actuel:

1. Ligne haute: `Next Round`, `Reroll $X`, puis 3 offres principales directes (`DomiNex` ou rarement `Domino`).
2. Ligne basse: `Future Slot`, puis 2 slots de packs independants.

Le shop genere 3 offres principales directes et 2 packs independants via `ShopPackOfferGenerator`. Chaque slot pack tire son type de contenu puis sa taille. Les deux slots peuvent donc etre 2 Gemstone Packs, 2 DomiNex Packs ou un mix des deux. Les doublons exacts sont evites quand le pool le permet. Les packs sont generes une seule fois a l'ouverture du shop et restent fixes jusqu'au prochain shop.

Offre principale `Domino`:

1. Une carte Domino affiche `DOMINO`, les valeurs `[ L | R ]`, `Add this domino to your bag` et `Buy $X`.
2. Generation uniforme parmi les dominos classiques 0|0 a 6|6, normalises pour afficher toujours la plus petite valeur a gauche.
3. Les doubles sont autorises et un domino deja present dans le sac peut etre propose; l'achat ajoute une copie supplementaire.
4. Prix: base 3, +1 si double, +1 si somme 7, +1 si somme >= 10, clamp 2..6.
5. L'achat utilise `ShopPurchaseService.TrySpend`, respecte `Credit Dominex` jusqu'a -20 credits, marque l'offre achetee et ajoute une `DominoInstance` dans `RunState.PurchasedDominoes`.
6. Le domino achete est permanent pour la run et est ajoute au sac a partir du prochain niveau via `CreateRunDominoSet`.
7. Le compteur de sac affiche maintenant `RemainingCount / RunState.TotalDominoCount`, donc le total augmente apres achat.

Poids de generation des packs:

| Axe | Valeur | Poids |
| --- | --- | ---: |
| Type | Gemstone | 35 |
| Type | DomiNex | 35 |
| Type | Domino | 30 |
| Taille | Normal | 75 |
| Taille | Jumbo | 20 |
| Taille | Mega | 5 |

| Pack | Type | Prix | Offres | Choix |
| --- | --- | ---: | ---: | ---: |
| Gemstone Pack | Gem Tile | 4 | 3 | 1 |
| Jumbo Gemstone Pack | Gem Tile | 6 | 5 | 1 |
| Mega Gemstone Pack | Gem Tile | 8 | 5 | up to 2 |
| DomiNex Pack | DomiNex | 6 | 3 | 1 |
| Jumbo DomiNex Pack | DomiNex | 9 | 5 | 1 |
| Mega DomiNex Pack | DomiNex | 12 | 5 | up to 2 |
| Domino Pack | Domino | 4 | 3 | 1 |
| Jumbo Domino Pack | Domino | 6 | 5 | 1 |
| Mega Domino Pack | Domino | 8 | 5 | up to 2 |

Un Gemstone Pack ouvert propose uniquement des Gem Tiles visibles en run et dont le pattern cible n'est pas au niveau max. Les choix n'ont pas de doublon si le pool est suffisant. Les cartes non choisies disparaissent a la confirmation. Mega Gemstone Pack autorise 0, 1 ou 2 choix selon les slots disponibles; confirmer avec 0 choix ferme le pack sans ajouter de Gem Tile. Si un Mega Pack est ouvert avec un seul slot consommable libre, il autorise jusqu'a 1 choix.

Un DomiNex Pack ouvert propose des DomiNex selon les memes poids de rarete que les offres directes. Le pool exclut les DomiNex deja possedes, les DomiNex temporairement desactives, les DomiNex non fonctionnels et les DomiNex `Cursed`. Les choix n'ont pas de doublon. Si le pool contient moins de DomiNex que le nombre demande, le pack affiche seulement ceux disponibles. Si aucun DomiNex n'est disponible, le pack n'est pas achetable.

Les DomiNex obtenus via pack sont ajoutes dans les premiers slots DomiNex libres. Ils recoivent un `PurchasePrice` virtuel par rarete pour la vente: Common 4, Rare 6, Epic 9, Legendary 20. La valeur de vente reste `max(1, floor(purchasePrice * 0.5))`, donc Common pack vend 2, Rare 3, Epic 4, Legendary 10.

Un Domino Pack ouvert propose des `DominoInstance` a ajouter au sac permanent de run. Les valeurs sont tirees entre 0 et 6, normalisees `min|max`, avec doubles autorises. Chaque domino a 50% de chance d'avoir un modificateur existant. Les modificateurs sont tires via `DominoModifierRollService` avec les poids suivants:

| Modificateur | Poids |
| --- | ---: |
| Blue | 25 |
| Red | 25 |
| Gold | 20 |
| Jackpot | 12 |
| Lucky | 8 |
| Glass | 6 |
| Light | 4 |

Les Domino Packs utilisent les memes regles de choix que les autres packs: Normal 1 parmi 3, Jumbo 1 parmi 5, Mega jusqu'a 2 parmi 5. Mega peut etre confirme avec 0 choix; le pack est alors perdu. Les dominos choisis gardent leur `ModifierId`, sont ajoutes a `RunState.PurchasedDominoes`, augmentent `RunState.TotalDominoCount`, et apparaissent dans le sac actif du prochain niveau.

L'ouverture d'un Booster Pack est une fenetre modale centrale avec overlay sombre. Le shop derriere est bloque: pas de `Next Round`, pas de `Reroll`, pas d'achat DomiNex ou pack tant que le pack n'est pas confirme. Il n'y a pas de bouton Cancel apres achat.

Les achats de DomiNex sont bloques quand `ActiveDomiNexCount >= MaxDomiNexSlots`. Le shop affiche `DomiNex X/5`; un achat refuse ne retire aucun credit.

Les achats de Gemstone Packs sont bloques quand `ActiveConsumableCount >= MaxConsumableSlots`. Les achats de DomiNex Packs sont bloques quand `ActiveDomiNexCount >= MaxDomiNexSlots` et affichent `DomiNex slots full`. Les Domino Packs n'ont pas de limite de slot. Un achat refuse ne retire aucun credit. Le slot `Future Slot` est volontairement non interactif.

Reroll shop:

1. `Reroll` regenere uniquement les 3 offres principales directes, qui peuvent devenir DomiNex ou Domino selon les probabilites.
2. Les deux packs et le `Future Slot` ne changent jamais pendant un reroll, peu importe leur type ou taille.
3. Le premier reroll payant d'un shop coute 5 credits.
4. Chaque reroll payant reussi augmente `ShopState.RerollCountThisShop` de 1.
5. Cout courant: `ShopRerollCostService.BaseRerollCost + RerollCountThisShop`, soit 5, 6, 7, 8, etc.
6. Le cout reset a 5 a chaque nouveau shop car un nouveau `ShopState` demarre a `RerollCountThisShop = 0`.
7. `NextShopInfiniteFreeRerolls` affiche `Reroll FREE`, ne retire aucun credit et ne fait pas progresser le cout pendant ce shop.
8. `NextShopFreeReroll` rend le premier reroll gratuit; ce reroll gratuit ne compte pas dans la progression, donc le reroll payant suivant reste a 5.

`ShopRerollCostService` centralise le cout pour permettre des modificateurs futurs: flat, multiplicatifs, minimum, maximum ou gratuite. Les achats DomiNex directs, Gemstone Packs, DomiNex Packs et rerolls utilisent `ShopPurchaseService.TrySpend`, donc `Credit Dominex` s'applique partout avec une limite a -20 credits.

`Credit Dominex` abaisse seulement la limite d'achat a `-20` credits. Les interets de cash out valent 0 si les credits sont negatifs.

## Jackpot Meter

Le Jackpot Meter est une mecanique permanente de run stockee dans `RunState.Jackpot` et affichee dans le panel droit via `JackpotMeterView`.

State principal:

| Champ | Valeur initiale | Role |
| --- | ---: | --- |
| `Meter` | 0 | Progression actuelle. |
| `MaxMeter` | 100 | Seuil d'un ticket. |
| `SpinTickets` | 0 | Spins disponibles. |
| `JackpotLuck` | 0 | Pity sur les symboles forts. |
| `MachineHeat` | 0 | Heat visuel et garantie de paire. |
| `JackpotMeterMultiplier` | 1 | Multiplicateur temporaire de gain meter. |
| `JackpotMeterMultiplierLevelsRemaining` | 0 | Niveaux restants du multiplicateur. |

Tous les gains passent par `JackpotMeterService.AddJackpotMeter(run, amount, source)`. La methode applique le multiplicateur actif, ajoute le meter, cree autant de `SpinTickets` que possible et conserve le surplus.

Gains a la validation:

| Source | Gain |
| --- | ---: |
| Niveau classique gagne | random 8-12 |
| Boss gagne | random 25-35 |
| Domino somme 7 | +3 par domino |
| Value Pattern `jackpot_7` | +20 |
| Double joue | +2 par double |
| Value Pattern `double_tile` | +5 |
| Value Pattern `triple_double` | +15 |
| Quota depasse de 50 % | +15 |
| Quota depasse de 100 % | +20 seulement |
| Pattern secret revele | +50 une seule fois |

Le breakdown du dernier gain est stocke dans `JackpotState.LastGainBreakdown`.

Spin machine:

Flow exact du spin:

1. `JackpotMeterView` appelle `GameFlowController.BeginJackpotSpin()`.
2. Le controller verifie les tickets, consomme 1 ticket et cree un `PendingJackpotSpinResult` complet via `JackpotSpinService.Spin`, mais ne revele pas les symboles dans le message.
3. Les rouleaux sont immediatement remplaces par des symboles random visuels.
4. Les trois rouleaux tournent en meme temps avec des symboles random qui ne changent jamais le resultat logique.
5. Le rouleau 1 s'arrete sur `Symbol1`, puis le rouleau 2 sur `Symbol2`, puis le rouleau 3 sur `Symbol3`.
6. Si les deux premiers symboles sont identiques, le troisieme rouleau attend plus longtemps. Si les deux premiers sont `Seven`, le suspense est encore plus long.
7. Le texte d'information reste neutre (`Jackpot spinning...`) tant qu'une reward est pending; il ne montre ni symboles, ni tier, ni reward avant la fin.
8. Apres l'arret du troisieme rouleau, le resultat est marque comme pret puis la popup de reward apparait apres environ 1 seconde.
9. Le bouton `Collect` appelle `CollectPendingJackpotReward()` et applique la reward une seule fois.
10. Le bouton `SPIN` est desactive pendant l'animation et tant qu'une reward est pending.

Regle critique: l'animation n'a pas le droit de refaire un tirage logique ou d'afficher un resultat final different de `PendingJackpotSpinResult`. Elle n'a pas non plus le droit d'afficher le resultat final avant l'arret reel des rouleaux.

| Symbole | Poids base | Couleur UI |
| --- | ---: | --- |
| `Blank` | 25 | gris |
| `Coin` | 22 | jaune |
| `Domino` | 16 | ivoire |
| `Gem` | 14 | violet/bleu |
| `DomiNex` | 10 | cyan |
| `Skull` | 6 | violet sombre |
| `Crown` | 5 | or |
| `Seven` | 2 | rouge/or |

`JackpotSpinService.RollSymbol` applique `JackpotLuck`: par point de luck, `Seven +1`, `Crown +1`, `Blank -1` minimum 1. Un spin sans triple augmente `JackpotLuck` jusqu'a 5. Un triple ou 777 remet `JackpotLuck` a 0.

`MachineHeat` augmente apres chaque spin sans triple. Une consolation ajoute +1 Heat. Une paire ajoute aussi +1 Heat. N'importe quel triple remet `MachineHeat` a 0, y compris `Blank / Blank / Blank`, `Coin / Coin / Coin`, etc. Un 777 est un triple majeur et remet aussi `MachineHeat` a 0.

A `Heat >= 5`, le prochain spin garantit au moins une paire. La garantie est appliquee avant le calcul du tier et avant la creation du resultat final utilise par l'animation. Si le tirage brut contient deja une paire ou un triple, il est conserve. Si les trois symboles sont differents, la version simple force `symbols[2] = symbols[0]`. La garantie consommee remet `MachineHeat` a 0 apres le spin, meme si le resultat final est seulement une paire.

L'animation utilise toujours le resultat final apres garantie, jamais le tirage brut interne.

Tiers de spin:

1. `Seven / Seven / Seven`: `MajorJackpot`.
2. Trois symboles identiques: `Triple`.
3. Deux symboles identiques: `Pair`.
4. Sinon: `Consolation`.

Rewards implementes ou placeholders propres:

| Resultat | Effet |
| --- | --- |
| Consolation | +5 credits, +25 meter, free reroll prochain shop, ou +1 discard prochain niveau. |
| 2 Coins | +10 credits. |
| 2 Gems | Ouvre un Jumbo Gemstone Pack gratuit. |
| 2 Dominos | Placeholder, +10 credits compensation. |
| 2 DomiNex | Prochain shop a un DomiNex gratuit. |
| 2 Crowns | Prochain premier achat shop -50 %. |
| 2 Skulls | +20 credits, prochain quota +10 %. |
| 2 Sevens | +1 Spin Ticket et +10 credits. |
| 3 Coins | +30 credits. |
| 3 Gems | Ouvre un Mega Pack gratuit et ajoute +5 niveaux aux patterns les plus joues. |
| 3 Dominos | Placeholder, +10 credits compensation. |
| 3 DomiNex | Legendary gratuit si disponible/slot libre, sinon +20 credits. |
| 3 Crowns | +1 slot DomiNex et +1 slot consommable. |
| 3 Skulls | Placeholder cursed DomiNex, +30 credits. |
| 777 | Ouvre `MAJOR JACKPOT`, choix de 2 rewards parmi 5. |

Major Jackpot rewards:

1. `Royal Seat`: +1 slot DomiNex et +1 slot consommable.
2. `Gem Flood`: applique 3 upgrades Gem Tile visibles directement.
3. `Casino Credit`: +50 credits.
4. `Pattern Ascension`: +5 niveaux aux patterns les plus joues.
5. `Crown DomiNex`: Legendary gratuit si possible, sinon +25 credits.
6. `Boss Bribe`: prochain boss quota x0.10.
7. `Infinite Reroll`: prochain shop rerolls gratuits.
8. `Double Prize`: credits doubles jusqu'a fin d'etage.
9. `Jackpot Engine`: Jackpot Meter x3 pendant 3 niveaux.

Les gains de credits des rewards Jackpot passent par `RunEconomyService.AddCredits`, qui applique `DoubleCreditsUntilEndOfFloor` une seule fois.

## DomiNex UX / Sell / Reorder

Les DomiNex possedes dans la barre haute sont selectionnables. Le slot selectionne a un contour dore et ouvre une carte de detail proche de la barre haute.

La carte DomiNex affiche:

1. Nom.
2. Rarete.
3. Description dynamique via `DomiNexDefinition.GetCurrentEffectText(run)` quand disponible.
4. Tags.
5. Bouton `Sell $X`.

Valeur de vente DomiNex:

`SellValueService.GetDomiNexSellValue(instance) = max(1, floor(purchasePrice * 0.5))`

Si le prix d'achat est inconnu ou gratuit, un fallback par rarete est utilise pour garantir une vente minimum de 1 credit.

Le drag & drop DomiNex utilise `DomiNexInventory.SlotInstances`, une structure slot-based qui conserve les trous intermediaires. Chaque slot UI implemente `IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`, `IDropHandler`, `IPointerExitHandler` et `IPointerClickHandler`. Un seuil de 6 pixels separe le click simple du drag. Pendant le drag, un ghost visuel non-raycastable suit la souris; il ne bloque donc pas les slots cibles. Les slots vides acceptent un drop; dans ce cas le DomiNex est deplace dans ce slot precis et son ancien slot devient vide. Les slots occupes echangent leurs instances. Un drop hors zone annule le deplacement car aucun `SwapSlots` n'est appele. Le scoring et l'animation lisent ensuite les slots de gauche a droite via `DomiNexInventory.Active`, en ignorant les slots vides.

Cleanup visuel drag: `DomiNexSlotInteraction.ClearAllDomiNexDragVisualStates()` restaure les outlines temporaires de tous les slots actifs, reset les flags drag/drop, et detruit toujours le ghost. Elle est appelee dans `OnDrop`, `OnEndDrag`, `OnPointerExit` et `OnDisable`, ce qui evite les carres orange/jaunes bloques apres un drop, un swap, un drop hors zone ou un re-render immediat.

Vendre un DomiNex:

1. Retire l'instance active.
2. Ajoute les credits de vente.
3. Libere le slot.
4. Reset la selection.
5. Force la prochaine preview/scoring a ignorer ses effets.

## Reward / Cash Out

| Reward Line | Formula |
| --- | --- |
| Niveau gagne | `LevelWinCredits` |
| Discards restants | `DiscardsRemaining x CreditsPerRemainingDiscard` |
| Interets | `credits <= 0 ? 0 : min(MaxInterestCredits, Credits / InterestCreditStep)` |
