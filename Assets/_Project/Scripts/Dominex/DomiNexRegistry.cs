using System.Collections.Generic;
using System.Linq;

namespace DomiNox.Dominex
{
    public static class DomiNexRegistry
    {
        public static IReadOnlyList<string> PrototypeActiveIds { get; } = new[]
        {
            "main_stable",
            "double_simple",
            "petit_profit",
            "compteur_bleu",
            "petite_mise",
            "gros_jeton",
            "full_nox_rare",
            "jackpot_7_rare",
            "banque_noire",
            "relance_vip",
            "limite_souple",
            "haute_mise_epic",
            "petite_fortune",
            "domino_fantome",
            "dette_rouge"
        };

        public static IReadOnlyList<DomiNexDefinition> All { get; } = new[]
        {
            Define("main_stable", "Main Stable", DomiNexRarity.Common, "La premiere defausse de chaque niveau est gratuite.", Tags("discard", "comfort"), Future("Free first discard per level.")),
            Define("petit_profit", "Petit Profit", DomiNexRarity.Common, "Gagne +1 credit si tu termines un niveau avec au moins 2 actions restantes.", Tags("credits", "actions"), Future("LevelWon credit reward.")),
            Define("double_simple", "Double Simple", DomiNexRarity.Common, "Chaque double joue donne +1 Mult supplementaire.", Tags("double", "mult"), Scoring(DomiNexEffectType.AddMultPerDouble, 1)),
            Define("compteur_bleu", "Compteur Bleu", DomiNexRarity.Common, "Si tu poses au moins 4 dominos, +15 Count.", Tags("count", "placed"), Scoring(DomiNexEffectType.AddCountIfPlacedAtLeast, 15, 4)),
            Define("jeton_de_table", "Jeton de Table", DomiNexRarity.Common, "Gagne +1 credit apres chaque niveau reussi.", Tags("credits"), Future("LevelWon credit reward.")),
            Define("fond_de_sac", "Fond de Sac", DomiNexRarity.Common, "Le dernier domino pioche pendant un niveau donne +3 Mult s'il est joue.", Tags("draw", "mult"), Future("Needs draw origin tracking.")),
            Define("petite_mise", "Petite Mise", DomiNexRarity.Common, "Les dominos dont la somme est inferieure ou egale a 4 donnent +1 Mult.", Tags("low", "mult"), Scoring(DomiNexEffectType.AddMultPerDominoSumAtMost, 1, 4)),
            Define("gros_jeton", "Gros Jeton", DomiNexRarity.Common, "Les dominos dont la somme est superieure ou egale a 10 donnent +5 Count.", Tags("high", "count"), Scoring(DomiNexEffectType.AddCountPerDominoSumAtLeast, 5, 10)),
            Define("main_propre", "Main Propre", DomiNexRarity.Common, "Si tu ne fais aucune defausse pendant un niveau, +3 Mult.", Tags("discard", "mult"), Scoring(DomiNexEffectType.AddMultIfNoDiscard, 3)),
            Define("dernier_coup", "Dernier Coup", DomiNexRarity.Common, "Le dernier domino joue donne +10 Count.", Tags("count", "order"), Scoring(DomiNexEffectType.AddCountToLastDomino, 10)),
            Define("premiere_pose", "Premiere Pose", DomiNexRarity.Common, "Le premier domino joue donne +2 Mult.", Tags("mult", "order"), Scoring(DomiNexEffectType.AddMultToFirstDomino, 2)),
            Define("suite_facile", "Suite Facile", DomiNexRarity.Common, "Les suites courtes donnent +2 Mult supplementaire.", Tags("pattern", "mult"), Future("Needs sequence pattern detector.")),
            Define("economie_mineure", "Economie Mineure", DomiNexRarity.Common, "Les interets commencent a 8 credits au lieu de 10.", Tags("economy"), Future("Needs interest system.")),
            Define("domino_poli", "Domino Poli", DomiNexRarity.Common, "Les dominos dores donnent +1 Count supplementaire.", Tags("gold", "count"), Future("Needs special domino traits.")),
            Define("reroll_leger", "Reroll Leger", DomiNexRarity.Common, "Le premier Draw de chaque niveau coute 0 action.", Tags("draw", "actions"), Future("Needs draw action system.")),
            Define("poche_secrete", "Poche Secrete", DomiNexRarity.Common, "+1 taille de main, mais seulement au premier niveau de chaque etage.", Tags("hand"), Future("Needs floor progression.")),
            Define("chaine_courte", "Chaine Courte", DomiNexRarity.Common, "Si tu joues exactement 3 dominos, +4 Mult.", Tags("mult", "placed"), Scoring(DomiNexEffectType.AddMultIfExactPlacedCount, 4, 3)),
            Define("coup_sur", "Coup Sur", DomiNexRarity.Common, "Si ton score depasse le quota de moins de 20 %, gagne +1 credit.", Tags("credits", "precision"), Future("Needs post-score quota margin reward.")),

            Define("double_ou_rien", "Double ou Rien", DomiNexRarity.Rare, "Les doubles donnent +4 Mult au lieu de +2, mais si aucun double n'est joue, le prochain quota augmente de 5%.", Tags("double", "risk"), Scoring(DomiNexEffectType.AddMultPerDouble, 2), Future("Needs next quota modifier.")),
            Define("banque_noire", "Banque Noire", DomiNexRarity.Rare, "Gagne +1 Mult par tranche de 10 credits possedes.", Tags("credits", "mult"), Scoring(DomiNexEffectType.AddMultPerCreditStep, 1, 10)),
            Define("tapis_bleu", "Tapis Bleu", DomiNexRarity.Rare, "Le premier domino bleu joue rend 1 action.", Tags("blue", "actions"), Future("Needs colored domino traits.")),
            Define("full_nox_rare", "Full Nox", DomiNexRarity.Rare, "Si tu utilises exactement ta limite maximale de dominos, +10 Mult.", Tags("full", "mult"), Scoring(DomiNexEffectType.AddMultIfFullNox, 10)),
            Define("jackpot_7_rare", "Jackpot 7", DomiNexRarity.Rare, "Chaque domino dont la somme vaut 7 donne +2 Mult. Si tu en joues 3, gagne +4 credits.", Tags("seven", "mult", "credits"), Scoring(DomiNexEffectType.AddMultPerDominoSumExactly, 2, 7), Future("Credit reward needs LevelWon hook.")),
            Define("limite_souple", "Limite Souple", DomiNexRarity.Rare, "+1 domino jouable, mais -1 Count par domino joue.", Tags("limit", "count"), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, 1), Scoring(DomiNexEffectType.AddCountPerDominoSumAtLeast, -1, 0)),
            Define("main_serree", "Main Serree", DomiNexRarity.Rare, "-1 domino jouable, mais +6 Mult de base.", Tags("limit", "mult"), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, -1), Scoring(DomiNexEffectType.AddMult, 6)),
            Define("marchandage", "Marchandage", DomiNexRarity.Rare, "Les shops coutent 1 credit de moins, minimum 1.", Tags("shop", "economy"), Future("Needs shop.")),
            Define("casino_bleu", "Casino Bleu", DomiNexRarity.Rare, "Les dominos bleus donnent +5 Count et +1 Mult.", Tags("blue", "count", "mult"), Future("Needs colored domino traits.")),
            Define("casino_rouge", "Casino Rouge", DomiNexRarity.Rare, "Les dominos rouges donnent +12 Count, mais coutent 1 credit si le niveau est rate.", Tags("red", "risk"), Future("Needs colored domino traits and fail hook.")),
            Define("mise_verte", "Mise Verte", DomiNexRarity.Rare, "Chaque domino vert joue donne +1 credit si le niveau est gagne.", Tags("green", "credits"), Future("Needs colored domino traits.")),
            Define("relance_vip", "Relance VIP", DomiNexRarity.Rare, "Une fois par niveau, tu peux relancer toute ta main pour 0 action.", Tags("draw", "comfort"), Future("Needs reroll UI action.")),
            Define("valeur_fetiche", "Valeur Fetiche", DomiNexRarity.Rare, "Au debut de chaque niveau, une valeur entre 0 et 6 est choisie. Les dominos contenant cette valeur donnent +2 Mult.", Tags("value", "mult"), Future("Needs level random modifier.")),
            Define("combo_tardif", "Combo Tardif", DomiNexRarity.Rare, "Si tu joues tous tes dominos apres avoir utilise au moins 2 actions, +8 Mult.", Tags("actions", "mult"), Future("Needs action timing tracking.")),

            Define("architecte_du_casino", "Architecte du Casino", DomiNexRarity.Epic, "Les suites longues et les patterns d'equilibre donnent deux fois plus de Mult.", Tags("pattern", "mult"), Future("Needs advanced patterns.")),
            Define("sac_dore", "Sac Dore", DomiNexRarity.Epic, "Les dominos dores donnent +2 credits au lieu de +1.", Tags("gold", "credits"), Future("Needs special domino traits.")),
            Define("limite_brisee", "Limite Brisee", DomiNexRarity.Epic, "+1 domino jouable par niveau, mais -1 action de depart.", Tags("limit", "actions"), LevelStart(DomiNexEffectType.AddMaxPlacedDominoes, 1), Future("Needs start action modifier.")),
            Define("haute_mise_epic", "Haute Mise", DomiNexRarity.Epic, "Les dominos dont la somme est superieure ou egale a 10 donnent +5 Count et +1 Mult.", Tags("high", "count", "mult"), Scoring(DomiNexEffectType.AddCountPerDominoSumAtLeast, 5, 10), Scoring(DomiNexEffectType.AddMultPerDominoSumAtLeast, 1, 10)),
            Define("petite_fortune", "Petite Fortune", DomiNexRarity.Epic, "Les dominos dont la somme est inferieure ou egale a 4 donnent +3 Mult.", Tags("low", "mult"), Scoring(DomiNexEffectType.AddMultPerDominoSumAtMost, 3, 4)),
            Define("jackpot_instable", "Jackpot Instable", DomiNexRarity.Epic, "A chaque fois que tu declenches un Jackpot, gagne x1.5 score final, mais le prochain shop a +20% de prix.", Tags("seven", "risk"), Future("Needs final score multiplier and shop modifier.")),
            Define("oeil_du_croupier", "Oeil du Croupier", DomiNexRarity.Epic, "Tu vois les 3 prochains dominos du sac.", Tags("draw", "planning"), Future("Needs bag peek UI.")),
            Define("domino_fantome", "Domino Fantome", DomiNexRarity.Epic, "Le premier domino joue a chaque niveau est copie en version fantome, qui ne compte pas dans la limite de pose.", Tags("copy", "limit"), Future("Needs ghost placement model.")),

            Define("roi_du_jackpot", "Roi du Jackpot", DomiNexRarity.Legendary, "Tous les effets Jackpot sont doubles.", Tags("seven", "legendary"), Future("Needs jackpot effect category.")),
            Define("casino_infini", "Casino Infini", DomiNexRarity.Legendary, "Apres avoir atteint le quota, tu peux continuer a jouer tes dominos restants pour augmenter tes credits bonus.", Tags("credits", "endgame"), Future("Needs post-quota play mode.")),
            Define("banquier_royal", "Banquier Royal", DomiNexRarity.Legendary, "Les interets n'ont plus de plafond.", Tags("economy", "legendary"), Future("Needs interest system.")),

            Define("dette_rouge", "Dette Rouge", DomiNexRarity.Cursed, "Gagne immediatement 30 credits. Tous les shops coutent 20% plus cher jusqu'au prochain boss.", Tags("credits", "cursed"), RunStart(DomiNexEffectType.AddCredits, 30), Future("Needs shop and boss duration.")),
            Define("main_brulee", "Main Brulee", DomiNexRarity.Cursed, "+15 Mult de base. -2 taille de main.", Tags("mult", "cursed"), Scoring(DomiNexEffectType.AddMult, 15), Future("Needs hand size modifier."))
        };

        public static IReadOnlyList<DomiNexDefinition> GetPrototypeActiveSet()
        {
            return PrototypeActiveIds.Select(GetById).Where(definition => definition != null).ToList();
        }

        public static DomiNexDefinition GetById(string id)
        {
            return All.FirstOrDefault(definition => definition.Id == id);
        }

        private static DomiNexDefinition Define(string id, string name, DomiNexRarity rarity, string description, IReadOnlyList<string> tags, params DomiNexEffectDefinition[] effects)
        {
            return new DomiNexDefinition(id, name, rarity, description, tags, effects);
        }

        private static IReadOnlyList<string> Tags(params string[] tags) => tags;
        private static DomiNexEffectDefinition Scoring(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.Scoring, type, value, threshold);
        private static DomiNexEffectDefinition LevelStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.LevelStart, type, value, threshold);
        private static DomiNexEffectDefinition RunStart(DomiNexEffectType type, int value = 0, int threshold = 0) => new DomiNexEffectDefinition(DomiNexTrigger.RunStart, type, value, threshold);
        private static DomiNexEffectDefinition Future(string note) => new DomiNexEffectDefinition(DomiNexTrigger.FutureHook, DomiNexEffectType.FutureHook, note: note);
    }
}
