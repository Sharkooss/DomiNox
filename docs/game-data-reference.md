# DomiNox Game Data Reference

Ce fichier recense les valeurs éditables actuelles du prototype. À mettre à jour à chaque changement dans `GameConstants`, `DomiNexRegistry`, `PatternCatalog` ou le scoring.

## Run / Level Values

| Valeur | Actuel | Source |
| --- | ---: | --- |
| Domino min value | 0 | `GameConstants.DominoMinValue` |
| Domino max value | 6 | `GameConstants.DominoMaxValue` |
| Grid width | 6 | `GameConstants.GridWidth` |
| Grid height | 6 | `GameConstants.GridHeight` |
| Starting hand size | 7 | `GameConstants.StartingHandSize` |
| Starting quota | 80 | `GameConstants.PhaseOneQuota` |
| Quota increase per level | 40 | `GameConstants.LevelQuotaIncrease` |
| Starting credits | 10 | `GameConstants.StartingCredits` |
| Max placed dominoes | 5 | `GameConstants.PhaseOneMaxPlacedDominoes` |
| Discards | 3 | `GameConstants.PhaseOneDiscards` |
| Win credits | 5 | `GameConstants.LevelWinCredits` |
| Credits per remaining discard | 1 | `GameConstants.CreditsPerRemainingDiscard` |
| Interest step | 5 credits | `GameConstants.InterestCreditStep` |
| Max interest credits | 5 | `GameConstants.MaxInterestCredits` |

## Core Round Rules

Chaque niveau commence avec une main de 7 dominos, une limite de 5 dominos posables et 3 discards disponibles. Il n'y a plus d'action points et il n'y a pas de bouton Draw. Le joueur peut defausser pour remplacer des dominos, poser jusqu'a la limite, puis valider une seule fois pour gagner ou perdre le niveau.

## Scoring Formula

`Final Score = Count x Mult`

Current flow:

1. Base Count = sum of played domino values.
2. Base Mult = 1.
3. Add the best value pattern bonus.
4. Add the best design pattern bonus if detected.
5. Add compatible bonus patterns such as `Nox Hand`.
6. Apply active DomiNex scoring effects.

## Value Patterns

| Pattern | Requirement | Count Bonus | Mult Bonus | Priority |
| --- | --- | ---: | ---: | ---: |
| Triple double | Jouer 3 doubles ou plus. | 80 | 4 | 90 |
| Big Straight | Jouer une suite connectée de 5 valeurs consécutives. | 80 | 5 | 80 |
| Jackpot 7 | Jouer au moins 3 dominos dont la somme vaut 7. | 70 | 4 | 70 |
| Double Pair | Jouer 2 doubles dans la même validation. | 45 | 2 | 60 |
| Same Value | Jouer au moins 3 dominos qui contiennent la même valeur. | 40 | 3 | 50 |
| Small Straight | Jouer une suite connectée de 3 valeurs consécutives. | 35 | 2 | 40 |
| Double | Jouer au moins 1 double. | 25 | 1 | 30 |
| Pair Link | Jouer au moins 2 dominos qui partagent une valeur. | 20 | 1 | 20 |
| Low Roll | Tous les dominos joués ont une somme de 5 ou moins. | 20 | 4 | 15 |
| High Tile | Pattern par défaut si aucun autre pattern de valeur n'est retenu. | 10 | 0 | 0 |

## Design / Bonus Patterns

| Pattern | Type | Requirement | Count Bonus | Mult Bonus | Priority |
| --- | --- | --- | ---: | ---: | ---: |
| Loop | Design | Les cases jouées forment une boucle fermée. | 80 | 5 | 80 |
| Snake | Design | La forme change de direction au moins 2 fois sans se couper. | 25 | 2 | 50 |
| Corner | Design | La forme fait exactement un angle à 90 degrés. | 0 | 2 | 30 |
| Line | Design | Toutes les cases jouées forment une ligne horizontale ou verticale. | 20 | 0 | 20 |
| Nox Hand | Bonus | Utiliser exactement la limite de pose du niveau. | 50 | 3 | 10 |

## Bosses Demo

Les boss apparaissent tous les 5 niveaux. Le boss actif est choisi dans `BossRegistry.DemoBosses` avec rotation sur le pool.

| Boss | Rule Type | Effet | Quota |
| --- | --- | --- | ---: |
| Le Croupier Manchot | BannedValue | Une valeur entre 0 et 6 est bannie. Les dominos contenant cette valeur ne peuvent pas être posés. | x1.25 |
| La Table Rouge | ModifyDiscards | Le niveau commence avec 1 discard en moins. | x1.20 |
| La Veuve des Doubles | DisablePatterns | Les patterns `Double`, `Double Pair` et `Triple double` ne donnent aucun bonus. | x1.15 |
| La Machine 777 | JackpotBoost | Chaque domino de somme 7 donne +7 Count et +1 Mult. `Jackpot 7` donne x1.25 score final. | x1.50 |
| Le Sabot Verrouillé | LockHandDominoes | 2 dominos de la main de départ sont verrouillés et ne peuvent pas être défaussés. | x1.20 |

## Boss Rules Notes

| Rule | Notes |
| --- | --- |
| BannedValue | La valeur bannie est tirée au début du niveau boss. Les dominos concernés sont grisés et affichent `Valeur bannie` si le joueur essaie de les jouer. |
| ModifyDiscards | Modifie `DiscardsRemaining` au début du niveau. Le cash out utilise les discards réellement restants. |
| DisablePatterns | Le scoring retire les patterns désactivés avant de choisir le meilleur pattern de valeur. |
| JackpotBoost | Appliqué pendant le scoring boss après les patterns et avant les DomiNex. |
| LockHandDominoes | Les dominos verrouillés sont choisis après la main de départ. Ils peuvent être posés mais pas défaussés. |

## DomiNex

| Id | Name | Rarity | Tags | Current Effect |
| --- | --- | --- | --- | --- |
| main_stable | Main Stable | Common | discard, comfort | Première défausse de chaque niveau gratuite. |
| petit_profit | Petit Profit | Common | credits, discard | Futur: +1 crédit si niveau fini avec au moins 1 discard restant. |
| double_simple | Double Simple | Common | double, mult | +1 Mult par double joué. |
| compteur_bleu | Compteur Bleu | Common | count, placed | Si au moins 4 dominos posés, +15 Count. |
| jeton_de_table | Jeton de Table | Common | credits | Futur: +1 crédit après chaque niveau réussi. |
| fond_de_sac | Fond de Sac | Common | draw, mult | Futur: dernier domino pioché donne +3 Mult s'il est joué. |
| petite_mise | Petite Mise | Common | low, mult | Dominos de somme <= 4 donnent +1 Mult. |
| gros_jeton | Gros Jeton | Common | high, count | Dominos de somme >= 10 donnent +5 Count. |
| main_propre | Main Propre | Common | discard, mult | Si aucune défausse pendant le niveau, +3 Mult. |
| dernier_coup | Dernier Coup | Common | count, order | Le dernier domino joué donne +10 Count. |
| premiere_pose | Premiere Pose | Common | mult, order | Le premier domino joué donne +2 Mult. |
| suite_facile | Suite Facile | Common | pattern, mult | Futur: suites courtes donnent +2 Mult supplémentaire. |
| economie_mineure | Economie Mineure | Common | economy | Futur: intérêts commencent à 8 crédits au lieu de 10. |
| domino_poli | Domino Poli | Common | gold, count | Futur: dominos dorés donnent +1 Count supplémentaire. |
| reroll_leger | Reroll Leger | Common | discard, comfort | Futur: première défausse de chaque niveau ne consomme pas de discard. |
| poche_secrete | Poche Secrete | Common | hand | Futur: +1 taille de main au premier niveau de chaque étage. |
| chaine_courte | Chaine Courte | Common | mult, placed | Si exactement 3 dominos joués, +4 Mult. |
| coup_sur | Coup Sur | Common | credits, precision | Futur: dépassement quota < 20% donne +1 crédit. |
| double_ou_rien | Double ou Rien | Rare | double, risk | +2 Mult par double. Futur: malus quota si aucun double. |
| banque_noire | Banque Noire | Rare | credits, mult | +1 Mult par tranche de 10 crédits possédés. |
| tapis_bleu | Tapis Bleu | Rare | blue, discard | Futur: premier domino bleu joué rend 1 discard, une fois par niveau. |
| full_nox_rare | Full Nox | Rare | full, mult | Si limite maximale de dominos utilisée, +10 Mult. |
| jackpot_7_rare | Jackpot 7 | Rare | seven, mult, credits | Dominos de somme 7 donnent +2 Mult. Futur: 3 joués donne +4 crédits. |
| limite_souple | Limite Souple | Rare | limit, count | +1 domino jouable, -1 Count par domino joué. |
| main_serree | Main Serree | Rare | limit, mult | -1 domino jouable, +6 Mult. |
| marchandage | Marchandage | Rare | shop, economy | Futur: shops coûtent 1 crédit de moins, minimum 1. |
| casino_bleu | Casino Bleu | Rare | blue, count, mult | Futur: dominos bleus donnent +5 Count et +1 Mult. |
| casino_rouge | Casino Rouge | Rare | red, risk | Futur: dominos rouges donnent +12 Count, coût si niveau raté. |
| mise_verte | Mise Verte | Rare | green, credits | Futur: dominos verts donnent +1 crédit si niveau gagné. |
| relance_vip | Relance VIP | Rare | draw, comfort | Futur: relance toute la main pour 0 action une fois par niveau. |
| valeur_fetiche | Valeur Fetiche | Rare | value, mult | Futur: valeur choisie au niveau donne +2 Mult. |
| combo_tardif | Combo Tardif | Rare | discard, mult | Futur: validation après au moins 1 discard donne +8 Mult. |
| architecte_du_casino | Architecte du Casino | Epic | pattern, mult | Futur: suites longues et équilibre donnent deux fois plus de Mult. |
| sac_dore | Sac Dore | Epic | gold, credits | Futur: dominos dorés donnent +2 crédits au lieu de +1. |
| limite_brisee | Limite Brisee | Epic | limit, discard | +1 domino jouable. Futur: -1 discard. |
| haute_mise_epic | Haute Mise | Epic | high, count, mult | Dominos de somme >= 10 donnent +5 Count et +1 Mult. |
| petite_fortune | Petite Fortune | Epic | low, mult | Dominos de somme <= 4 donnent +3 Mult. |
| jackpot_instable | Jackpot Instable | Epic | seven, risk | Futur: jackpot donne x1.5 score final, prochain shop +20%. |
| oeil_du_croupier | Oeil du Croupier | Epic | draw, planning | Futur: voir les 3 prochains dominos du sac. |
| domino_fantome | Domino Fantome | Epic | copy, limit | Futur: premier domino copié en fantôme hors limite. |
| roi_du_jackpot | Roi du Jackpot | Legendary | seven, legendary | Futur: tous les effets Jackpot sont doublés. |
| casino_infini | Casino Infini | Legendary | credits, endgame | Futur: continuer après quota pour crédits bonus. |
| banquier_royal | Banquier Royal | Legendary | economy, legendary | Futur: intérêts sans plafond. |
| dette_rouge | Dette Rouge | Cursed | credits, cursed | +30 crédits au run start. Futur: shops +20% jusqu'au boss. |
| main_brulee | Main Brulee | Cursed | mult, cursed | +15 Mult. Futur: -2 taille de main. |

## Shop Notes

Le shop actuel génère 3 offres DomiNex pondérées par rareté et exclut les DomiNex déjà possédés. Les DomiNex maudits ne sont pas dans le pool normal pour l'instant.

## Reward / Cash Out

| Reward Line | Formula |
| --- | --- |
| Niveau gagné | `LevelWinCredits` |
| Discards restants | `DiscardsRemaining x CreditsPerRemainingDiscard` |
| Intérêts | `min(MaxInterestCredits, Credits / InterestCreditStep)` |
