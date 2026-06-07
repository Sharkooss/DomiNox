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

Le joueur peut stocker 2 consommables Gem Tiles. Les Gem Tiles ne prennent pas de slot DomiNex et s'utilisent manuellement depuis la barre haute.

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

Le sac de la colonne droite est un indicateur compact `Sac` + compteur. La tooltip boss est positionnee pres du label boss et clamp dans l'ecran.

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
6. Une Gem Tile dont le pattern cible est deja niveau max n'est plus proposee par le shop.
7. Les achats respectent `Credit Dominex`: sans lui credits >= prix, avec lui credits - prix >= -20.

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

La visibilite des contenus secrets est centralisee dans `CollectableVisibilityService`.

Regles:

1. `DevMode.Enabled == true`: patterns, combos, consommables et collectables sont visibles meme s'ils sont secrets ou non reveles.
2. Dev mode inactif: un pattern secret est visible seulement s'il est dans `RunState.RevealedSecretPatterns`.
3. Dev mode inactif: un combo utilisant un Design Pattern secret est visible seulement si ce Design Pattern est revele.
4. Dev mode inactif: une Gem Tile ciblant un pattern secret est visible/proposable seulement si ce pattern est revele.

La Collection possede un onglet `Consumables` listant les Gem Tiles visibles. Run Info et Shop utilisent la meme logique de visibilite.

Preview visuelle:

1. `Christ Cross` utilise un diagramme large 6x5 pour afficher sa croix prolongee sans rognage.
2. `Big Loop` utilise un diagramme de contour 4x4 distinct de `Tile Loop`.

## Shop Notes

Le shop actuel genere 3 offres DomiNex ponderees par rarete et exclut les DomiNex deja possedes. Il genere aussi 2 offres Gem Tiles sans rarete, sans doublon dans le meme shop, et exclut les Gem Tiles dont le pattern cible est niveau max.

Les achats de DomiNex sont bloques quand `ActiveDomiNexCount >= MaxDomiNexSlots`. Le shop affiche `DomiNex X/5`; un achat refuse ne retire aucun credit.

Les achats de Gem Tiles sont bloques quand `ActiveConsumableCount >= MaxConsumableSlots`. Le shop affiche les Gem Tiles avec leur pattern cible, `Lv.X -> Lv.Y`, et le prix. Le slot `Future Slot` est volontairement non interactif.

`Credit Dominex` abaisse seulement la limite d'achat a `-20` credits. Les interets de cash out valent 0 si les credits sont negatifs.

## Reward / Cash Out

| Reward Line | Formula |
| --- | --- |
| Niveau gagne | `LevelWinCredits` |
| Discards restants | `DiscardsRemaining x CreditsPerRemainingDiscard` |
| Interets | `credits <= 0 ? 0 : min(MaxInterestCredits, Credits / InterestCreditStep)` |
