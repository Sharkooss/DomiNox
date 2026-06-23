using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DomiNox.Tests.EditMode
{
    public sealed class DomiNoxEngineTests
    {
        [Test]
        public void SpaceDomiNex_HasPostScoringChancePatternLevelUpEffect()
        {
            var definition = GetDomiNexById("space_dominex");
            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var postScoring = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "PostScoring");

            Assert.IsNotNull(postScoring, "space_dominex doit avoir un effet PostScoring");
            Assert.AreEqual("ChancePatternLevelUp", GetString(GetValue(postScoring, "Type"), null));
            Assert.AreEqual(1, GetInt(postScoring, "Value"));
            Assert.AreEqual(4, GetInt(postScoring, "Threshold"));
        }

        [Test]
        public void GrosMichel_HasBothScoringAndPostScoringEffects()
        {
            var definition = GetDomiNexById("gros_michel");
            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();

            var scoring = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "Scoring");
            var postScoring = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "PostScoring");

            Assert.IsNotNull(scoring, "gros_michel doit avoir un effet Scoring (+15 Mult)");
            Assert.AreEqual("AddMult", GetString(GetValue(scoring, "Type"), null));
            Assert.AreEqual(15, GetInt(scoring, "Value"));

            Assert.IsNotNull(postScoring, "gros_michel doit avoir un effet PostScoring");
            Assert.AreEqual("ChanceDestroySelf", GetString(GetValue(postScoring, "Type"), null));
            Assert.AreEqual(1, GetInt(postScoring, "Value"));
            Assert.AreEqual(6, GetInt(postScoring, "Threshold"));
        }

        [Test]
        public void CreditDomiNex_HasPassiveSetMinimumCreditFloorEffect()
        {
            var definition = GetDomiNexById("credit_dominex");
            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var passive = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "Passive");

            Assert.IsNotNull(passive, "credit_dominex doit avoir un effet Passive");
            Assert.AreEqual("SetMinimumCreditFloor", GetString(GetValue(passive, "Type"), null));
            Assert.AreEqual(20, GetInt(passive, "Value"));
        }

        [Test]
        public void GetMinimumCreditFloor_WithoutCreditDomiNex_ReturnsZero()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var floor = (int)T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("GetMinimumCreditFloor")
                .Invoke(engine, new[] { inventory });

            Assert.AreEqual(0, floor);
        }

        [Test]
        public void GetMinimumCreditFloor_WithCreditDomiNex_ReturnsMinus20()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var credit = GetDomiNexById("credit_dominex");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { credit, (object)4 });
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var floor = (int)T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("GetMinimumCreditFloor")
                .Invoke(engine, new[] { inventory });

            Assert.AreEqual(-20, floor);
        }

        [Test]
        public void RollPostScoringEffects_WithSpaceDomiNex_ReturnsPatternLevelUpResult()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var space = GetDomiNexById("space_dominex");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { space, (object)4 });
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var results = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("RollPostScoringEffects")
                .Invoke(engine, new object[] { inventory, "tile_high", "Tile High" })).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("space_dominex", GetString(results[0], "SourceId"));
            Assert.AreEqual("PatternLevelUp", GetString(GetValue(results[0], "ActionType"), null));
            Assert.AreEqual("tile_high", GetString(results[0], "PatternId"));
        }

        [Test]
        public void RollPostScoringEffects_WithSpaceDomiNexAndNoValuePattern_ReturnsNoResult()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var space = GetDomiNexById("space_dominex");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { space, (object)4 });
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var results = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("RollPostScoringEffects")
                .Invoke(engine, new object[] { inventory, null, null })).ToList();

            Assert.AreEqual(0, results.Count, "Aucun result si pas de value pattern");
        }

        [Test]
        public void RollPostScoringEffects_WithGrosMichel_ReturnsDestroySelfResult()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var michel = GetDomiNexById("gros_michel");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { michel, (object)4 });
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var results = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("RollPostScoringEffects")
                .Invoke(engine, new object[] { inventory, null, null })).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("gros_michel", GetString(results[0], "SourceId"));
            Assert.AreEqual("DestroySelf", GetString(GetValue(results[0], "ActionType"), null));
        }

        [Test]
        public void ApplyPostScoringResults_TriggeredPatternLevelUp_IncreasesPatternLevel()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var patternLevels = Activator.CreateInstance(T("DomiNox.Run.PatternLevelState"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));
            var actionType = Enum.Parse(T("DomiNox.Scoring.PostScoringActionType"), "PatternLevelUp");
            var result = Activator.CreateInstance(T("DomiNox.Scoring.PostScoringEffectResult"),
                "space_dominex", "Space Dominex", "tile_high level up", true, "tile_high", actionType);
            var resultList = Activator.CreateInstance(typeof(List<>).MakeGenericType(T("DomiNox.Scoring.PostScoringEffectResult")));
            resultList.GetType().GetMethod("Add").Invoke(resultList, new[] { result });

            T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("ApplyPostScoringResults")
                .Invoke(engine, new[] { inventory, resultList, patternLevels });

            var level = (int)T("DomiNox.Run.PatternLevelState").GetMethod("GetLevel").Invoke(patternLevels, new object[] { "tile_high" });
            Assert.AreEqual(2, level);
        }

        [Test]
        public void ApplyPostScoringResults_TriggeredDestroySelf_RemovesDomiNexFromInventory()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var michel = GetDomiNexById("gros_michel");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { michel, (object)4 });
            var patternLevels = Activator.CreateInstance(T("DomiNox.Run.PatternLevelState"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));
            var actionType = Enum.Parse(T("DomiNox.Scoring.PostScoringActionType"), "DestroySelf");
            var result = Activator.CreateInstance(T("DomiNox.Scoring.PostScoringEffectResult"),
                "gros_michel", "Gros Michel", "Destroyed", true, null, actionType);
            var resultList = Activator.CreateInstance(typeof(List<>).MakeGenericType(T("DomiNox.Scoring.PostScoringEffectResult")));
            resultList.GetType().GetMethod("Add").Invoke(resultList, new[] { result });

            T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("ApplyPostScoringResults")
                .Invoke(engine, new[] { inventory, resultList, patternLevels });

            var count = (int)inventory.GetType().GetProperty("Count").GetValue(inventory);
            Assert.AreEqual(0, count, "gros_michel doit etre supprime de l'inventaire");
        }

        [Test]
        public void ApplyPostScoringResults_Untriggered_ChangesNothing()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var michel = GetDomiNexById("gros_michel");
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { michel, (object)4 });
            var patternLevels = Activator.CreateInstance(T("DomiNox.Run.PatternLevelState"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));
            var actionType = Enum.Parse(T("DomiNox.Scoring.PostScoringActionType"), "DestroySelf");
            var result = Activator.CreateInstance(T("DomiNox.Scoring.PostScoringEffectResult"),
                "gros_michel", "Gros Michel", "Survived", false, null, actionType);
            var resultList = Activator.CreateInstance(typeof(List<>).MakeGenericType(T("DomiNox.Scoring.PostScoringEffectResult")));
            resultList.GetType().GetMethod("Add").Invoke(resultList, new[] { result });

            T("DomiNox.Dominex.DomiNexEffectEngine")
                .GetMethod("ApplyPostScoringResults")
                .Invoke(engine, new[] { inventory, resultList, patternLevels });

            var count = (int)inventory.GetType().GetProperty("Count").GetValue(inventory);
            Assert.AreEqual(1, count, "gros_michel ne doit pas etre supprime si Triggered=false");
        }

        [Test]
        public void SpaceDomiNex_IsAvailableInPrototypeShop()
        {
            var definition = GetDomiNexById("space_dominex");

            var available = (bool)T("DomiNox.Dominex.DomiNexRegistry")
                .GetMethod("IsAvailableInPrototypeShop", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new[] { definition });

            Assert.IsTrue(available, "space_dominex doit etre disponible en shop maintenant qu'il a un effet PostScoring");
        }

        [Test]
        public void CreditDomiNex_IsAvailableInPrototypeShop()
        {
            var definition = GetDomiNexById("credit_dominex");

            var available = (bool)T("DomiNox.Dominex.DomiNexRegistry")
                .GetMethod("IsAvailableInPrototypeShop", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new[] { definition });

            Assert.IsTrue(available, "credit_dominex doit etre disponible en shop maintenant qu'il a un effet Passive");
        }

        [Test]
        public void NoDomiNexIdIsHardcodedOutsideRegistry()
        {
            // Ce test documente l'invariant. Si tu rajoutes un DomiNex ID hors DomiNexRegistry.cs,
            // ajoute plutot un effet PostScoring/Passive dans la definition et un handler dans le moteur.
            var allIds = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexRegistry")
                .GetProperty("All", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null)).Select(d => GetString(d, "Id")).ToHashSet();

            Assert.Greater(allIds.Count, 0, "Le registry doit contenir des DomiNex");
            Assert.IsTrue(allIds.Contains("space_dominex"));
            Assert.IsTrue(allIds.Contains("gros_michel"));
            Assert.IsTrue(allIds.Contains("credit_dominex"));
        }

        [Test]
        public void RerollDomiNex_HasPassiveFreeFirstShopRerollEffect()
        {
            var definition = GetDomiNexById("reroll_dominex");
            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var passive = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "Passive");

            Assert.IsNotNull(passive, "reroll_dominex doit avoir un effet Passive");
            Assert.AreEqual("FreeFirstShopReroll", GetString(GetValue(passive, "Type"), null));
        }

        [Test]
        public void RerollDomiNex_IsAvailableInPrototypeShop()
        {
            var definition = GetDomiNexById("reroll_dominex");

            var available = (bool)T("DomiNox.Dominex.DomiNexRegistry")
                .GetMethod("IsAvailableInPrototypeShop", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new[] { definition });

            Assert.IsTrue(available, "reroll_dominex doit etre disponible en shop maintenant qu'il a un effet Passive");
        }

        [Test]
        public void GrantsFreeFirstShopReroll_WithRerollDomiNex_ReturnsTrue()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            AddToInventory(inventory, GetDomiNexById("reroll_dominex"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var result = (bool)engine.GetType().GetMethod("GrantsFreeFirstShopReroll").Invoke(engine, new[] { inventory });

            Assert.IsTrue(result);
        }

        [Test]
        public void GrantsFreeFirstShopReroll_WithoutRerollDomiNex_ReturnsFalse()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            var result = (bool)engine.GetType().GetMethod("GrantsFreeFirstShopReroll").Invoke(engine, new[] { inventory });

            Assert.IsFalse(result);
        }

        [Test]
        public void Midas_HasLevelWonAddCreditsEffect()
        {
            var definition = GetDomiNexById("midas");
            Assert.AreEqual("Legendary", GetString(GetValue(definition, "Rarity"), null));

            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var levelWon = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "LevelWon");

            Assert.IsNotNull(levelWon, "midas doit avoir un effet LevelWon");
            Assert.AreEqual("AddCredits", GetString(GetValue(levelWon, "Type"), null));
            Assert.AreEqual(6, GetInt(levelWon, "Value"));
        }

        [Test]
        public void ApplyLevelWon_WithMidas_AddsCredits()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            AddToInventory(inventory, GetDomiNexById("midas"));
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var creditsBefore = (int)run.GetType().GetProperty("Credits").GetValue(run);
            var engine = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexEffectEngine"));

            engine.GetType().GetMethod("ApplyLevelWon").Invoke(engine, new object[] { inventory, run, null });

            var creditsAfter = (int)run.GetType().GetProperty("Credits").GetValue(run);
            Assert.AreEqual(creditsBefore + 6, creditsAfter);
        }

        [Test]
        public void Overlord_HasMultiplyMultByPlacedDominoesScoringEffect()
        {
            var definition = GetDomiNexById("overlord");
            Assert.AreEqual("Legendary", GetString(GetValue(definition, "Rarity"), null));

            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var scoring = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "Scoring");

            Assert.IsNotNull(scoring);
            Assert.AreEqual("MultiplyMultByPlacedDominoes", GetString(GetValue(scoring, "Type"), null));
        }

        [Test]
        public void BloodPact_IsCursedWithPlacedPenaltyAndMultBonus()
        {
            var definition = GetDomiNexById("blood_pact");
            Assert.AreEqual("Cursed", GetString(GetValue(definition, "Rarity"), null));

            var effects = ((IEnumerable<object>)GetValue(definition, "Effects")).ToList();
            var levelStart = effects.FirstOrDefault(e => GetString(GetValue(e, "Trigger"), null) == "LevelStart");

            Assert.IsNotNull(levelStart, "blood_pact doit avoir un malus LevelStart");
            Assert.AreEqual("AddMaxPlacedDominoes", GetString(GetValue(levelStart, "Type"), null));
            Assert.AreEqual(-1, GetInt(levelStart, "Value"));
        }

        [Test]
        public void Registry_ContainsEpicLegendaryAndCursedDomiNex()
        {
            var all = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexRegistry")
                .GetProperty("All", BindingFlags.Public | BindingFlags.Static).GetValue(null)).ToList();

            Assert.IsTrue(all.Any(d => GetString(GetValue(d, "Rarity"), null) == "Epic"), "Au moins un Epic");
            Assert.IsTrue(all.Any(d => GetString(GetValue(d, "Rarity"), null) == "Legendary"), "Au moins un Legendary");
            Assert.IsTrue(all.Any(d => GetString(GetValue(d, "Rarity"), null) == "Cursed"), "Au moins un Cursed");
        }

        private static void AddToInventory(object inventory, object definition)
        {
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) })
                .Invoke(inventory, new[] { definition, (object)0 });
        }

        [Test]
        public void RunSaveMapper_RoundTrip_PreservesRunState()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            SetProp(run, "Credits", 42);

            var level = Activator.CreateInstance(T("DomiNox.Run.LevelState"));
            SetProp(level, "FloorIndex", 2);
            SetProp(level, "LevelIndex", 7);
            SetProp(run, "CurrentLevel", level);

            var inventory = GetValue(run, "DomiNexInventory");
            AddToInventory(inventory, GetDomiNexById("dominex"));

            var patternLevels = GetValue(run, "PatternLevels");
            patternLevels.GetType().GetMethod("Increase").Invoke(patternLevels, new object[] { "tile_high", 2 });

            var jackpot = GetValue(run, "Jackpot");
            jackpot.GetType().GetProperty("Meter").SetValue(jackpot, 55);

            var defType = T("DomiNox.Dominoes.DominoDefinition");
            var definition = Activator.CreateInstance(defType, "shop_domino_3_4", 3, 4);
            var instanceType = T("DomiNox.Dominoes.DominoInstance");
            var instance = Activator.CreateInstance(instanceType, "jackpot_domino_0", definition, "red");
            var purchased = GetValue(run, "PurchasedDominoes");
            purchased.GetType().GetMethod("Add").Invoke(purchased, new[] { instance });

            var mapper = T("DomiNox.Persistence.RunSaveMapper");
            var data = mapper.GetMethod("ToData").Invoke(null, new[] { run });
            Assert.AreEqual(7, GetInt(data, "levelIndex"));
            Assert.AreEqual(2, GetInt(data, "floorIndex"));

            var restored = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            mapper.GetMethod("Apply").Invoke(null, new[] { data, restored });

            Assert.AreEqual(42, GetInt(restored, "Credits"));

            var restoredActive = ((IEnumerable<object>)GetValue(GetValue(restored, "DomiNexInventory"), "Active"))
                .Select(d => GetString(d, "Id")).ToList();
            Assert.Contains("dominex", restoredActive);

            var restoredLevels = GetValue(restored, "PatternLevels");
            var tileHighLevel = (int)restoredLevels.GetType().GetMethod("GetLevel").Invoke(restoredLevels, new object[] { "tile_high" });
            Assert.AreEqual(3, tileHighLevel);

            var restoredJackpot = GetValue(restored, "Jackpot");
            Assert.AreEqual(55, (int)restoredJackpot.GetType().GetProperty("Meter").GetValue(restoredJackpot));

            var restoredPurchased = ((IEnumerable<object>)GetValue(restored, "PurchasedDominoes")).ToList();
            Assert.AreEqual(1, restoredPurchased.Count);
            Assert.AreEqual("jackpot_domino_0", GetString(restoredPurchased[0], "InstanceId"));
            Assert.AreEqual("red", GetString(restoredPurchased[0], "ModifierId"));
        }

        [Test]
        public void RunSaveMapper_Apply_WithNullData_DoesNotThrow()
        {
            var restored = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            Assert.DoesNotThrow(() =>
                T("DomiNox.Persistence.RunSaveMapper").GetMethod("Apply").Invoke(null, new object[] { null, restored }));
        }

        [Test]
        public void Registry_AllDomiNexIdsAreUnique()
        {
            var ids = ((IEnumerable<object>)T("DomiNox.Dominex.DomiNexRegistry")
                .GetProperty("All", BindingFlags.Public | BindingFlags.Static).GetValue(null))
                .Select(d => GetString(d, "Id")).ToList();

            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Les IDs DomiNex doivent etre uniques");
        }

        [Test]
        public void BossRegistry_AllBossIdsAreUniqueAndIncludeNewBosses()
        {
            var bosses = (System.Array)T("DomiNox.Bosses.BossRegistry")
                .GetProperty("DemoBosses", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            var ids = bosses.Cast<object>().Select(b => GetString(b, "Id")).ToList();

            Assert.AreEqual(ids.Count, ids.Distinct().Count(), "Les IDs de boss doivent etre uniques");
            Assert.IsTrue(ids.Contains("the_tightrope"));
            Assert.IsTrue(ids.Contains("line_breaker"));
        }

        [Test]
        public void ObjectiveService_CompleteFloor3_UnlocksSchismAndEndless()
        {
            var profile = Activator.CreateInstance(T("DomiNox.Persistence.MetaProfile"));
            var context = Activator.CreateInstance(T("DomiNox.Objectives.ObjectiveProgressContext"));
            SetProp(context, "FloorReached", 3);

            var newly = ((IEnumerable<object>)T("DomiNox.Objectives.ObjectiveService")
                .GetMethod("Evaluate").Invoke(null, new[] { profile, context }))
                .Select(o => GetString(o, "Id")).ToList();

            Assert.Contains("complete_floor_3", newly);
            Assert.IsTrue((bool)GetValue(profile, "endlessUnlocked"));
            var unlocked = ((IEnumerable<object>)GetValue(profile, "unlockedDomiNexIds")).Select(x => x.ToString()).ToList();
            Assert.Contains("le_schisme", unlocked);
        }

        [Test]
        public void ObjectiveService_AlreadyCompleted_DoesNotRetrigger()
        {
            var profile = Activator.CreateInstance(T("DomiNox.Persistence.MetaProfile"));
            var completed = GetValue(profile, "completedObjectiveIds");
            completed.GetType().GetMethod("Add").Invoke(completed, new object[] { "complete_floor_3" });
            var context = Activator.CreateInstance(T("DomiNox.Objectives.ObjectiveProgressContext"));
            SetProp(context, "FloorReached", 5);

            var newly = ((IEnumerable<object>)T("DomiNox.Objectives.ObjectiveService")
                .GetMethod("Evaluate").Invoke(null, new[] { profile, context }))
                .Select(o => GetString(o, "Id")).ToList();

            Assert.IsFalse(newly.Contains("complete_floor_3"));
        }

        [Test]
        public void DomiNexUnlockService_SchismLockedUntilUnlocked()
        {
            var unlockType = T("DomiNox.Objectives.DomiNexUnlockService");
            var locked = (bool)unlockType.GetMethod("IsLockedByDefault").Invoke(null, new object[] { "le_schisme" });
            Assert.IsTrue(locked);

            var availableLocked = (bool)unlockType.GetMethod("IsAvailable").Invoke(null, new object[] { "le_schisme", null });
            Assert.IsFalse(availableLocked);

            var availableUnlocked = (bool)unlockType.GetMethod("IsAvailable").Invoke(null, new object[] { "le_schisme", new List<string> { "le_schisme" } });
            Assert.IsTrue(availableUnlocked);

            var commonAlwaysAvailable = (bool)unlockType.GetMethod("IsAvailable").Invoke(null, new object[] { "dominex", null });
            Assert.IsTrue(commonAlwaysAvailable);
        }

        [Test]
        public void CollectionService_LockedRewardDomiNex_HasLockedStatusAndHint()
        {
            var profile = Activator.CreateInstance(T("DomiNox.Persistence.MetaProfile"));
            var entries = ((IEnumerable<object>)T("DomiNox.Objectives.CollectionService")
                .GetMethod("BuildEntries").Invoke(null, new object[] { profile, null })).ToList();

            var whale = entries.First(e => GetString(e, "DomiNexId") == "the_whale");
            Assert.AreEqual("Locked", GetString(GetValue(whale, "Status"), null));
            Assert.IsFalse(string.IsNullOrEmpty(GetString(whale, "Hint")));

            var dominex = entries.First(e => GetString(e, "DomiNexId") == "dominex");
            Assert.AreNotEqual("Locked", GetString(GetValue(dominex, "Status"), null));
        }

        [Test]
        public void GridState_DisconnectedPlacement_RequiresClusterCapAboveOne()
        {
            var gridSingle = Activator.CreateInstance(T("DomiNox.Grid.GridState"));
            Assert.IsTrue(PlaceDomino(gridSingle, MakeDomino("a", 1, 1), 0, 0, "HorizontalRight"));
            Assert.IsFalse(PlaceDomino(gridSingle, MakeDomino("b", 2, 2), 4, 4, "HorizontalRight"),
                "Sans la Faille (MaxClusters=1) on ne peut pas poser une region deconnectee");

            var gridSchism = Activator.CreateInstance(T("DomiNox.Grid.GridState"));
            gridSchism.GetType().GetProperty("MaxClusters").SetValue(gridSchism, 2);
            Assert.IsTrue(PlaceDomino(gridSchism, MakeDomino("a", 1, 1), 0, 0, "HorizontalRight"));
            Assert.IsTrue(PlaceDomino(gridSchism, MakeDomino("b", 2, 2), 4, 4, "HorizontalRight"),
                "Avec la Faille (MaxClusters=2) une 2e region deconnectee est autorisee");
            Assert.AreEqual(2, (int)gridSchism.GetType().GetMethod("CountClusters").Invoke(gridSchism, null));
        }

        private static object MakeDomino(string id, int left, int right)
        {
            var definition = Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), id, left, right);
            return Activator.CreateInstance(T("DomiNox.Dominoes.DominoInstance"), id, definition);
        }

        private static bool PlaceDomino(object grid, object domino, int x, int y, string orientationName)
        {
            var position = Activator.CreateInstance(T("DomiNox.Grid.GridPosition"), x, y);
            var orientation = Enum.Parse(T("DomiNox.Grid.DominoOrientation"), orientationName);
            return (bool)grid.GetType().GetMethod("PlaceDomino").Invoke(grid, new[] { domino, position, orientation });
        }

        private static void SetProp(object instance, string property, object value)
        {
            instance.GetType().GetProperty(property).SetValue(instance, value);
        }

        private static object GetDomiNexById(string id)
        {
            return T("DomiNox.Dominex.DomiNexRegistry")
                .GetMethod("GetById", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { id });
        }

        private static Type T(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(name))
                .First(t => t != null);
        }

        private static object GetValue(object instance, string member)
        {
            if (member == null || instance == null)
            {
                return member == null ? instance : null;
            }

            var type = instance.GetType();
            var property = type.GetProperty(member);
            if (property != null)
            {
                return property.GetValue(instance);
            }

            return type.GetField(member)?.GetValue(instance);
        }

        private static string GetString(object instance, string property)
        {
            return GetValue(instance, property)?.ToString();
        }

        private static int GetInt(object instance, string property)
        {
            return (int)GetValue(instance, property);
        }
    }
}
