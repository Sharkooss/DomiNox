using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DomiNox.Tests.EditMode
{
    public sealed class PatternDetectorDesignTests
    {
        private const string TileLoopId = "tile_loop";
        private const string CrossTileId = "cross_tile";
        private const string ChristCrossId = "christ_cross";
        private const string BigLoopId = "big_loop";
        private const string TileLineId = "tile_line";

        [Test]
        public void ClosedLoopDetectsTileLoopNotCrossTile()
        {
            var pattern = DetectDesign(CreateLoop(1));

            Assert.AreEqual(TileLoopId, GetString(pattern, "Id"));
            Assert.AreNotEqual(CrossTileId, GetString(pattern, "Id"));
        }

        [Test]
        public void InvalidLoopDoesNotDetectTileLoop()
        {
            var placed = CreateLoop(1);
            placed[3] = Domino("bad", 0, 1, 0, 1, "VerticalDown");

            var pattern = DetectDesign(placed);

            Assert.AreNotEqual(TileLoopId, GetString(pattern, "Id"));
        }

        [Test]
        public void MinimalCrossDetectsCrossTile()
        {
            var pattern = DetectDesign(CreateCross(1));

            Assert.AreEqual(CrossTileId, GetString(pattern, "Id"));
        }

        [Test]
        public void LineDoesNotDetectCrossTile()
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("a", 1, 1, 0, 0, "HorizontalRight"));
            placed.Add(Domino("b", 1, 1, 2, 0, "HorizontalRight"));
            placed.Add(Domino("c", 1, 1, 4, 0, "HorizontalRight"));

            var pattern = DetectDesign(placed);

            Assert.AreEqual(TileLineId, GetString(pattern, "Id"));
        }

        [Test]
        public void LShapeDoesNotDetectCrossTile()
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("a", 1, 1, 0, 0, "HorizontalRight"));
            placed.Add(Domino("b", 1, 1, 2, 0, "VerticalDown"));
            placed.Add(Domino("c", 1, 1, 2, 2, "VerticalDown"));

            var pattern = DetectDesign(placed);

            Assert.AreNotEqual(CrossTileId, GetString(pattern, "Id"));
        }

        [Test]
        public void VerticalExtendedCrossDetectsChristCross()
        {
            var placed = CreateCross(1);
            placed.Add(Domino("down", 1, 1, 2, 3, "VerticalDown"));

            var pattern = DetectDesign(placed);

            Assert.AreEqual(ChristCrossId, GetString(pattern, "Id"));
        }

        [Test]
        public void VerticalLineDoesNotDetectChristCross()
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("a", 1, 1, 2, 0, "VerticalDown"));
            placed.Add(Domino("b", 1, 1, 2, 2, "VerticalDown"));
            placed.Add(Domino("c", 1, 1, 2, 4, "VerticalDown"));

            var pattern = DetectDesign(placed);

            Assert.AreNotEqual(ChristCrossId, GetString(pattern, "Id"));
        }

        [Test]
        public void ChristCrossCatalogBonusIsCorrect()
        {
            var pattern = T("DomiNox.Patterns.PatternCatalog").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { ChristCrossId });

            Assert.AreEqual(50, GetInt(pattern, "ChipsBonus"));
            Assert.AreEqual(4, GetInt(pattern, "MultBonus"));
            Assert.IsTrue(GetBool(pattern, "IsSecret"));
        }

        [TestCase("tile_high", "high_christ_cross")]
        [TestCase("low_tile", "low_christ_cross")]
        [TestCase("high_tile", "high_roll_christ_cross")]
        [TestCase("double_tile", "double_christ_cross")]
        [TestCase("triple_double", "triple_christ_cross")]
        [TestCase("small_tile_straight", "small_straight_christ_cross")]
        [TestCase("long_tile_straight", "long_straight_christ_cross")]
        [TestCase("jackpot_7", "jackpot_christ_cross")]
        public void ChristCrossCombosUseExactDesignId(string valuePatternId, string expectedComboId)
        {
            var args = new object[] { valuePatternId, ChristCrossId, null };
            var found = (bool)T("DomiNox.Patterns.PatternComboCatalog").GetMethod("TryGetCombo", BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

            Assert.IsTrue(found);
            Assert.AreEqual(expectedComboId, GetString(args[2], "Id"));
            Assert.AreEqual(ChristCrossId, GetString(args[2], "DesignPatternId"));
        }

        [Test]
        public void ChristCrossComboDoesNotFallbackToCrossTile()
        {
            var args = new object[] { "tile_high", ChristCrossId, null };
            T("DomiNox.Patterns.PatternComboCatalog").GetMethod("TryGetCombo", BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

            Assert.AreNotEqual("high_cross", GetString(args[2], "Id"));
        }

        [Test]
        public void GarnetTileIsHiddenBeforeChristCrossReveal()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var available = (IEnumerable)T("DomiNox.Consumables.GemTileRegistry").GetMethod("GetAvailableGemTiles", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { run });

            Assert.IsFalse(available.Cast<object>().Any(item => GetString(item, "Id") == "garnet_tile"));
        }

        [Test]
        public void RunInfoHidesSecretPatternsBeforeReveal()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));

            var christVisible = T("DomiNox.Run.CollectableVisibilityService").GetMethod("IsPatternVisibleInRunInfo", new[] { typeof(string), T("DomiNox.Run.RunState") }).Invoke(null, new[] { ChristCrossId, run });
            var bigLoopVisible = T("DomiNox.Run.CollectableVisibilityService").GetMethod("IsPatternVisibleInRunInfo", new[] { typeof(string), T("DomiNox.Run.RunState") }).Invoke(null, new[] { BigLoopId, run });

            Assert.IsFalse((bool)christVisible);
            Assert.IsFalse((bool)bigLoopVisible);
        }

        [Test]
        public void RunInfoShowsSecretPatternAfterReveal()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            GetValue(run, "RevealedSecretPatterns").GetType().GetMethod("Add").Invoke(GetValue(run, "RevealedSecretPatterns"), new object[] { ChristCrossId });

            var visible = T("DomiNox.Run.CollectableVisibilityService").GetMethod("IsPatternVisibleInRunInfo", new[] { typeof(string), T("DomiNox.Run.RunState") }).Invoke(null, new[] { ChristCrossId, run });

            Assert.IsTrue((bool)visible);
        }

        [Test]
        public void GarnetTileIsAvailableAfterChristCrossReveal()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var revealed = GetValue(run, "RevealedSecretPatterns");
            revealed.GetType().GetMethod("Add").Invoke(revealed, new object[] { ChristCrossId });
            var available = (IEnumerable)T("DomiNox.Consumables.GemTileRegistry").GetMethod("GetAvailableGemTiles", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { run });

            Assert.IsTrue(available.Cast<object>().Any(item => GetString(item, "Id") == "garnet_tile"));
        }

        [Test]
        public void BigLoopDetectsBigLoopNotTileLoop()
        {
            var pattern = DetectDesign(CreateBigLoop(1));

            Assert.AreEqual(BigLoopId, GetString(pattern, "Id"));
            Assert.AreNotEqual(TileLoopId, GetString(pattern, "Id"));
        }

        [TestCase("tile_high", "high_big_loop")]
        [TestCase("low_tile", "low_big_loop")]
        [TestCase("high_tile", "high_roll_big_loop")]
        [TestCase("double_tile", "double_big_loop")]
        [TestCase("triple_double", "triple_big_loop")]
        [TestCase("small_tile_straight", "small_straight_big_loop")]
        [TestCase("long_tile_straight", "long_straight_big_loop")]
        [TestCase("jackpot_7", "jackpot_big_loop")]
        public void BigLoopCombosUseExactDesignId(string valuePatternId, string expectedComboId)
        {
            var args = new object[] { valuePatternId, BigLoopId, null };
            var found = (bool)T("DomiNox.Patterns.PatternComboCatalog").GetMethod("TryGetCombo", BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

            Assert.IsTrue(found);
            Assert.AreEqual(expectedComboId, GetString(args[2], "Id"));
            Assert.AreEqual(BigLoopId, GetString(args[2], "DesignPatternId"));
        }

        [Test]
        public void LapisTileTargetsBigLoop()
        {
            var tile = T("DomiNox.Consumables.GemTileRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "lapis_tile" });

            Assert.AreEqual(BigLoopId, GetString(tile, "TargetPatternId"));
            Assert.AreEqual(10, GetInt(tile, "Price"));
        }

        [Test]
        public void DomiNexSellValueUsesPurchasePrice()
        {
            var dominex = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex" });
            var instance = Activator.CreateInstance(T("DomiNox.Dominex.ActiveDomiNexInstance"), dominex, 6);
            var value = T("DomiNox.Shop.SellValueService").GetMethod("GetDomiNexSellValue", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { instance });

            Assert.AreEqual(3, value);
        }

        [Test]
        public void DomiNexInventorySwapMovesIntoEmptySlot()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var dominex = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex" });
            var grosMichel = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "gros_michel" });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { dominex, (object)4 });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { grosMichel, (object)4 });

            inventory.GetType().GetMethod("SwapSlots").Invoke(inventory, new object[] { 0, 4 });
            var active = (IEnumerable)GetValue(inventory, "Active");

            Assert.AreEqual("gros_michel", GetString(active.Cast<object>().First(), "Id"));
            Assert.AreEqual("dominex", GetString(active.Cast<object>().Last(), "Id"));
            Assert.AreEqual("dominex", GetString(GetValue(inventory.GetType().GetMethod("GetInstanceAt").Invoke(inventory, new object[] { 4 }), "Definition"), "Id"));
        }

        [Test]
        public void MegaBoosterPackAllowsZeroSelections()
        {
            var pack = T("DomiNox.Shop.BoosterPackRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "gemstone_pack_mega" });
            var choices = Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Consumables.ConsumableDefinition")));
            var state = Activator.CreateInstance(T("DomiNox.Shop.OpenBoosterPackState"), pack, choices, 2, 0, 0);

            Assert.AreEqual(0, GetInt(state, "MinPickCount"));
            Assert.AreEqual(2, GetInt(state, "MaxPickCount"));
            Assert.IsTrue(GetBool(state, "IsOptionalPick"));
        }

        [Test]
        public void DomiNexPacksExistWithExpectedCounts()
        {
            var registry = T("DomiNox.Shop.BoosterPackRegistry");
            var normal = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex_pack_normal" });
            var jumbo = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex_pack_jumbo" });
            var mega = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex_pack_mega" });

            Assert.AreEqual("DomiNex", GetString(GetValue(normal, "ContentType"), null));
            Assert.AreEqual(3, GetInt(normal, "OfferedCardCount"));
            Assert.AreEqual(1, GetInt(normal, "PickMin"));
            Assert.AreEqual(1, GetInt(normal, "PickMax"));
            Assert.AreEqual(5, GetInt(jumbo, "OfferedCardCount"));
            Assert.AreEqual(1, GetInt(jumbo, "PickMax"));
            Assert.AreEqual(5, GetInt(mega, "OfferedCardCount"));
            Assert.AreEqual(0, GetInt(mega, "PickMin"));
            Assert.AreEqual(2, GetInt(mega, "PickMax"));
        }

        [Test]
        public void MegaDomiNexPackAllowsZeroSelections()
        {
            var pack = T("DomiNox.Shop.BoosterPackRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex_pack_mega" });
            var choices = Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Dominex.DomiNexDefinition")));
            var state = Activator.CreateInstance(T("DomiNox.Shop.OpenBoosterPackState"), pack, choices, 2, 0, 0);

            Assert.AreEqual(0, GetInt(state, "MinPickCount"));
            Assert.AreEqual(2, GetInt(state, "MaxPickCount"));
            Assert.IsTrue(GetBool(state, "IsOptionalPick"));
        }

        [Test]
        public void DomiNexPackChoicesExcludeOwnedAndUnavailableDomiNex()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var dominex = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex" });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { dominex, (object)4 });
            var service = Activator.CreateInstance(T("DomiNox.Shop.DomiNexShopService"));

            var choices = (IEnumerable)service.GetType().GetMethod("GenerateDomiNexPackChoices").Invoke(service, new object[] { inventory, 1, 20 });
            var ids = choices.Cast<object>().Select(choice => GetString(choice, "Id")).ToList();
            var rarities = choices.Cast<object>().Select(choice => GetString(GetValue(choice, "Rarity"), null)).ToList();

            Assert.IsFalse(ids.Contains("dominex"));
            Assert.IsFalse(ids.Contains("reroll_dominex"));
            Assert.IsFalse(rarities.Contains("Cursed"));
            Assert.AreEqual(ids.Count, ids.Distinct().Count());
        }

        [Test]
        public void DomiNexPackVirtualPurchasePriceSetsExpectedSellValue()
        {
            var rarity = T("DomiNox.Dominex.DomiNexRarity");
            var rarePrice = T("DomiNox.Shop.DomiNexShopService").GetMethod("GetPackVirtualPurchasePrice", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { Enum.Parse(rarity, "Rare") });
            var dominex = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "fibonacci" });
            var instance = Activator.CreateInstance(T("DomiNox.Dominex.ActiveDomiNexInstance"), dominex, rarePrice);
            var value = T("DomiNox.Shop.SellValueService").GetMethod("GetDomiNexSellValue", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { instance });

            Assert.AreEqual(6, rarePrice);
            Assert.AreEqual(3, value);
        }

        [Test]
        public void GeneratedShopHasTwoIndependentPackSlots()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var service = Activator.CreateInstance(T("DomiNox.Shop.DomiNexShopService"));
            var shop = service.GetType().GetMethod("GenerateShop", new[] { T("DomiNox.Dominex.DomiNexInventory"), typeof(int) }).Invoke(service, new object[] { inventory, 1 });
            var offers = ((IEnumerable)GetValue(shop, "BoosterPackOffers")).Cast<object>().ToList();

            Assert.AreEqual(2, offers.Count);
            Assert.IsTrue(offers.All(offer => GetValue(offer, "Pack") != null));
        }

        [Test]
        public void ShopPackGeneratorCanProduceTwoGemstoneOrTwoDomiNexPacks()
        {
            var generatorType = T("DomiNox.Shop.ShopPackOfferGenerator");
            var sawDoubleGemstone = false;
            var sawDoubleDomiNex = false;
            var sawDoubleDomino = false;
            var sawMixed = false;

            for (var seed = 0; seed < 500 && (!sawDoubleGemstone || !sawDoubleDomiNex || !sawDoubleDomino || !sawMixed); seed++)
            {
                var generator = Activator.CreateInstance(generatorType, new object[] { new Random(seed) });
                var offers = ((IEnumerable)generatorType.GetMethod("GeneratePackOffers").Invoke(generator, new object[] { 2 })).Cast<object>().ToList();
                var contents = offers.Select(offer => GetString(GetValue(GetValue(offer, "Pack"), "ContentType"), null)).ToArray();
                sawDoubleGemstone |= contents.All(content => content == "GemstoneTile");
                sawDoubleDomiNex |= contents.All(content => content == "DomiNex");
                sawDoubleDomino |= contents.All(content => content == "Domino");
                sawMixed |= contents.Distinct().Count() == 2;
            }

            Assert.IsTrue(sawDoubleGemstone);
            Assert.IsTrue(sawDoubleDomiNex);
            Assert.IsTrue(sawDoubleDomino);
            Assert.IsTrue(sawMixed);
        }

        [Test]
        public void ShopPackGeneratorCanProduceDominoPacksWithExpectedWeightBand()
        {
            var generator = Activator.CreateInstance(T("DomiNox.Shop.ShopPackOfferGenerator"), new object[] { new Random(777) });
            var counts = new System.Collections.Generic.Dictionary<string, int>();
            for (var i = 0; i < 3000; i++)
            {
                var content = generator.GetType().GetMethod("PickContentType").Invoke(generator, null).ToString();
                counts[content] = counts.TryGetValue(content, out var count) ? count + 1 : 1;
            }

            Assert.Greater(counts["GemstoneTile"], counts["Domino"]);
            Assert.Greater(counts["DomiNex"], counts["Domino"]);
            Assert.Greater(counts["Domino"], 700);
        }

        [Test]
        public void ShopPackGeneratorWeightsNormalAboveJumboAboveMega()
        {
            var generator = Activator.CreateInstance(T("DomiNox.Shop.ShopPackOfferGenerator"), new object[] { new Random(1234) });
            var counts = new System.Collections.Generic.Dictionary<string, int> { ["Normal"] = 0, ["Jumbo"] = 0, ["Mega"] = 0 };

            for (var i = 0; i < 2000; i++)
            {
                var size = generator.GetType().GetMethod("PickPackSize").Invoke(generator, null).ToString();
                counts[size]++;
            }

            Assert.Greater(counts["Normal"], counts["Jumbo"]);
            Assert.Greater(counts["Jumbo"], counts["Mega"]);
        }

        [Test]
        public void ShopPackGeneratorAvoidsExactDuplicateWhenPossible()
        {
            var generator = Activator.CreateInstance(T("DomiNox.Shop.ShopPackOfferGenerator"), new object[] { new Random(7) });
            var offers = ((IEnumerable)generator.GetType().GetMethod("GeneratePackOffers").Invoke(generator, new object[] { 2 })).Cast<object>().ToList();
            var ids = offers.Select(offer => GetString(GetValue(GetValue(offer, "Pack"), "Id"), null)).ToArray();

            Assert.AreEqual(2, ids.Distinct().Count());
        }

        [Test]
        public void RerollDirectOffersKeepsPacksFixed()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var service = Activator.CreateInstance(T("DomiNox.Shop.DomiNexShopService"));
            var shop = service.GetType().GetMethod("GenerateShop", new[] { T("DomiNox.Dominex.DomiNexInventory"), typeof(int) }).Invoke(service, new object[] { inventory, 1 });
            var beforePacks = ((IEnumerable)GetValue(shop, "BoosterPackOffers")).Cast<object>().Select(offer => GetString(GetValue(GetValue(offer, "Pack"), "Id"), null)).ToArray();

            service.GetType().GetMethod("RerollDirectDomiNexOffers").Invoke(service, new object[] { shop, inventory, 1 });
            var afterPacks = ((IEnumerable)GetValue(shop, "BoosterPackOffers")).Cast<object>().Select(offer => GetString(GetValue(GetValue(offer, "Pack"), "Id"), null)).ToArray();
            var offers = ((IEnumerable)GetValue(shop, "Offers")).Cast<object>().ToList();

            CollectionAssert.AreEqual(beforePacks, afterPacks);
            Assert.AreEqual(3, offers.Count);
        }

        [Test]
        public void DirectShopOffersCanGenerateAtMostOneDominoOffer()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var service = Activator.CreateInstance(T("DomiNox.Shop.DomiNexShopService"));
            var sawDomino = false;

            for (var i = 0; i < 200; i++)
            {
                var shop = service.GetType().GetMethod("GenerateShop", new[] { T("DomiNox.Dominex.DomiNexInventory"), typeof(int) }).Invoke(service, new object[] { inventory, 1 });
                var offers = ((IEnumerable)GetValue(shop, "Offers")).Cast<object>().ToList();
                var dominoCount = offers.Count(offer => GetString(GetValue(offer, "Type"), null) == "Domino");
                sawDomino |= dominoCount > 0;
                Assert.LessOrEqual(dominoCount, 1);
            }

            Assert.IsTrue(sawDomino);
        }

        [Test]
        public void DominoShopOfferPriceUsesFormula()
        {
            var definitionType = T("DomiNox.Dominoes.DominoDefinition");
            var oneSix = Activator.CreateInstance(definitionType, "domino_1_6", 1, 6);
            var sixSix = Activator.CreateInstance(definitionType, "domino_6_6", 6, 6);
            var zeroOne = Activator.CreateInstance(definitionType, "domino_0_1", 0, 1);
            var service = T("DomiNox.Shop.DominoShopOfferService");

            Assert.AreEqual(4, service.GetMethod("GetPrice").Invoke(null, new[] { oneSix }));
            Assert.AreEqual(5, service.GetMethod("GetPrice").Invoke(null, new[] { sixSix }));
            Assert.AreEqual(3, service.GetMethod("GetPrice").Invoke(null, new[] { zeroOne }));
        }

        [Test]
        public void PurchasedDominoIncreasesRunDominoTotal()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var purchased = GetValue(run, "PurchasedDominoes");
            var definition = Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), "domino_2_5", 2, 5);
            var domino = Activator.CreateInstance(T("DomiNox.Dominoes.DominoInstance"), "purchased", definition, "blue");

            purchased.GetType().GetMethod("Add").Invoke(purchased, new[] { domino });

            Assert.AreEqual(29, GetInt(run, "TotalDominoCount"));
        }

        [Test]
        public void DominoPacksExistWithExpectedCounts()
        {
            var registry = T("DomiNox.Shop.BoosterPackRegistry");
            var normal = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "domino_pack_normal" });
            var jumbo = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "domino_pack_jumbo" });
            var mega = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "domino_pack_mega" });

            Assert.AreEqual("Domino", GetString(GetValue(normal, "ContentType"), null));
            Assert.AreEqual(4, GetInt(normal, "Price"));
            Assert.AreEqual(3, GetInt(normal, "OfferedCardCount"));
            Assert.AreEqual(1, GetInt(normal, "PickMax"));
            Assert.AreEqual(5, GetInt(jumbo, "OfferedCardCount"));
            Assert.AreEqual(1, GetInt(jumbo, "PickMax"));
            Assert.AreEqual(8, GetInt(mega, "Price"));
            Assert.AreEqual(5, GetInt(mega, "OfferedCardCount"));
            Assert.AreEqual(0, GetInt(mega, "PickMin"));
            Assert.AreEqual(2, GetInt(mega, "PickMax"));
        }

        [Test]
        public void DominoPackGeneratorCreatesValidDominoesWithAllowedModifiers()
        {
            var generator = Activator.CreateInstance(T("DomiNox.Shop.DominoPackGenerator"), new object[] { new Random(99) });
            var choices = ((IEnumerable)generator.GetType().GetMethod("GenerateChoices").Invoke(generator, new object[] { 100 })).Cast<object>().ToList();
            var allowed = ((IEnumerable)T("DomiNox.Dominoes.DominoModifierRegistry").GetProperty("All", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Cast<object>().Select(item => GetString(item, "Id")).ToHashSet();

            Assert.IsTrue(choices.Any(choice => GetString(choice, "ModifierId") != null));
            foreach (var choice in choices)
            {
                var definition = GetValue(choice, "Definition");
                Assert.GreaterOrEqual(GetInt(definition, "Left"), 0);
                Assert.LessOrEqual(GetInt(definition, "Left"), 6);
                Assert.GreaterOrEqual(GetInt(definition, "Right"), 0);
                Assert.LessOrEqual(GetInt(definition, "Right"), 6);
                Assert.LessOrEqual(GetInt(definition, "Left"), GetInt(definition, "Right"));
                var modifierId = GetString(choice, "ModifierId");
                Assert.IsTrue(modifierId == null || allowed.Contains(modifierId));
            }
        }

        [Test]
        public void DominoModifierRollWeightsFavorCommonModifiersOverRareLight()
        {
            var service = Activator.CreateInstance(T("DomiNox.Shop.DominoModifierRollService"), new object[] { new Random(123) });
            var counts = new System.Collections.Generic.Dictionary<string, int>();
            for (var i = 0; i < 2000; i++)
            {
                var id = (string)service.GetType().GetMethod("RollModifier").Invoke(service, null);
                counts[id] = counts.TryGetValue(id, out var count) ? count + 1 : 1;
            }

            Assert.Greater(counts["blue"], counts["light"]);
            Assert.Greater(counts["red"], counts["glass"]);
            Assert.Greater(counts["gold"], counts["light"]);
        }

        [Test]
        public void LevelStartsWithThreeHandsAndThreeDiscards()
        {
            var level = Activator.CreateInstance(T("DomiNox.Run.LevelState"));

            Assert.AreEqual(3, GetInt(level, "MaxHands"));
            Assert.AreEqual(3, GetInt(level, "HandsRemaining"));
            Assert.AreEqual(3, GetInt(level, "MaxDiscards"));
            Assert.AreEqual(3, GetInt(level, "DiscardsRemaining"));
            Assert.AreEqual(0, GetInt(level, "CurrentScore"));
        }

        [Test]
        public void LevelStateTracksPlayedAndDiscardedDominoesSeparately()
        {
            var level = Activator.CreateInstance(T("DomiNox.Run.LevelState"));
            var played = GetValue(level, "PlayedThisLevel");
            var discarded = GetValue(level, "DiscardedThisLevel");
            var domino = Domino("tracked", 2, 5, 0, 0, "HorizontalRight");
            var instance = GetValue(domino, "Domino");

            played.GetType().GetMethod("Add").Invoke(played, new[] { instance });
            discarded.GetType().GetMethod("Add").Invoke(discarded, new[] { instance });

            Assert.AreEqual(1, ((IEnumerable)played).Cast<object>().Count());
            Assert.AreEqual(1, ((IEnumerable)discarded).Cast<object>().Count());
        }

        [Test]
        public void DominoBagDrawDoesNotRecycleDiscardedDominoes()
        {
            var bag = Activator.CreateInstance(T("DomiNox.Dominoes.DominoBag"));
            var list = Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Dominoes.DominoInstance")));
            var domino = Activator.CreateInstance(T("DomiNox.Dominoes.DominoInstance"), "only", Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), "domino_0_0", 0, 0));
            list.GetType().GetMethod("Add").Invoke(list, new[] { domino });
            bag.GetType().GetMethod("Initialize").Invoke(bag, new[] { list });
            var drawn = (IEnumerable)bag.GetType().GetMethod("Draw").Invoke(bag, new object[] { 1 });
            bag.GetType().GetMethod("Discard").Invoke(bag, new[] { drawn.Cast<object>().First() });

            var secondDraw = (IEnumerable)bag.GetType().GetMethod("Draw").Invoke(bag, new object[] { 1 });

            Assert.AreEqual(0, secondDraw.Cast<object>().Count());
            Assert.AreEqual(0, bag.GetType().GetMethod("RemainingCount").Invoke(bag, null));
        }

        [Test]
        public void DominoModifierRegistryContainsOnlyRequestedModifiers()
        {
            var all = ((IEnumerable)T("DomiNox.Dominoes.DominoModifierRegistry").GetProperty("All", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Cast<object>().Select(item => GetString(item, "Id")).ToArray();

            CollectionAssert.AreEquivalent(new[] { "blue", "red", "gold", "glass", "lucky", "jackpot", "light" }, all);
        }

        [Test]
        public void BlueDominoAddsFifteenTileWhenScored()
        {
            var score = CalculateSingleModifiedDominoScore("blue");

            Assert.AreEqual(22, GetInt(score, "Count"));
            Assert.AreEqual(22, GetInt(score, "FinalScore"));
        }

        [Test]
        public void RedDominoAddsThreeMultWhenScored()
        {
            var score = CalculateSingleModifiedDominoScore("red");

            Assert.AreEqual(4, GetInt(score, "Mult"));
            Assert.AreEqual(28, GetInt(score, "FinalScore"));
        }

        [Test]
        public void GlassDominoDoublesHandScoreWhenScored()
        {
            var score = CalculateSingleModifiedDominoScore("glass");

            Assert.AreEqual(14, GetInt(score, "FinalScore"));
        }

        [Test]
        public void JackpotDominoCreatesJackpotMeterEffect()
        {
            var placed = ModifiedDomino("jackpot");
            var effects = (IEnumerable)T("DomiNox.Dominoes.DominoModifierEffectResolver").GetMethod("Resolve").Invoke(null, new[] { placed });
            var effect = effects.Cast<object>().Single();

            Assert.AreEqual(10, GetInt(effect, "JackpotMeterGain"));
        }

        [Test]
        public void LightDominoDoesNotCountAgainstPlacedLimitAndAppliesPenalty()
        {
            var score = CalculateSingleModifiedDominoScore("light");
            var level = Activator.CreateInstance(T("DomiNox.Run.LevelState"));
            var grid = GetValue(level, "Grid");
            var placed = ModifiedDomino("light");
            grid.GetType().GetMethod("PlaceDomino").Invoke(grid, new[] { GetValue(placed, "Domino"), GetValue(placed, "Position"), GetValue(placed, "Orientation") });

            Assert.AreEqual(7, GetInt(score, "FinalScore"));
            Assert.AreEqual(0, T("DomiNox.Run.GameFlowController").GetMethod("CountPlacedAgainstLimit").Invoke(null, new[] { level }));
        }

        [Test]
        public void DominoShopOfferCanBeMarkedPurchased()
        {
            var domino = Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), "domino_2_5", 2, 5);
            var offer = Activator.CreateInstance(T("DomiNox.Shop.ShopOffer"), domino, 4);

            offer.GetType().GetMethod("MarkPurchased").Invoke(offer, null);

            Assert.AreEqual("Domino", GetString(GetValue(offer, "Type"), null));
            Assert.IsTrue(GetBool(offer, "IsPurchased"));
        }

        [Test]
        public void RerollCostProgressesAndResetsPerShop()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var shop = Activator.CreateInstance(T("DomiNox.Shop.ShopState"));
            var costService = T("DomiNox.Shop.ShopRerollCostService");

            Assert.AreEqual(5, costService.GetMethod("GetCurrentRerollCost").Invoke(null, new[] { run, shop }));
            shop.GetType().GetProperty("RerollCountThisShop").SetValue(shop, 1);
            Assert.AreEqual(6, costService.GetMethod("GetCurrentRerollCost").Invoke(null, new[] { run, shop }));
            shop.GetType().GetProperty("RerollCountThisShop").SetValue(shop, 2);
            Assert.AreEqual(7, costService.GetMethod("GetCurrentRerollCost").Invoke(null, new[] { run, shop }));
            var newShop = Activator.CreateInstance(T("DomiNox.Shop.ShopState"));
            Assert.AreEqual(5, costService.GetMethod("GetCurrentRerollCost").Invoke(null, new[] { run, newShop }));
        }

        [Test]
        public void InfiniteFreeRerollsCostZero()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var shop = Activator.CreateInstance(T("DomiNox.Shop.ShopState"));
            run.GetType().GetProperty("NextShopInfiniteFreeRerolls").SetValue(run, true);
            shop.GetType().GetProperty("RerollCountThisShop").SetValue(shop, 4);

            var cost = T("DomiNox.Shop.ShopRerollCostService").GetMethod("GetCurrentRerollCost").Invoke(null, new[] { run, shop });

            Assert.AreEqual(0, cost);
        }

        [Test]
        public void CreditDomiNexAllowsRerollDebtToMinusTwenty()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            run.GetType().GetProperty("Credits").SetValue(run, -15);
            var inventory = GetValue(run, "DomiNexInventory");
            var credit = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "credit_dominex" });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { credit, (object)4 });

            var spent = T("DomiNox.Shop.ShopPurchaseService").GetMethod("TrySpend").Invoke(null, new object[] { 5, run, -20 });

            Assert.IsTrue((bool)spent);
            Assert.AreEqual(-20, GetInt(run, "Credits"));
        }

        [Test]
        public void DomiNexInventorySwapWithOccupiedSlotChangesVisualOrder()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Dominex.DomiNexInventory"));
            var registry = T("DomiNox.Dominex.DomiNexRegistry");
            var a = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex" });
            var b = registry.GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "gros_michel" });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { a, (object)4 });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { b, (object)4 });

            inventory.GetType().GetMethod("SwapSlots").Invoke(inventory, new object[] { 0, 1 });
            var active = ((IEnumerable)GetValue(inventory, "Active")).Cast<object>().Select(item => GetString(item, "Id")).ToArray();

            CollectionAssert.AreEqual(new[] { "gros_michel", "dominex" }, active);
        }

        [Test]
        public void FullDomiNexSlotsPreventAddingPackReward()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            run.GetType().GetProperty("MaxDomiNexSlots").SetValue(run, 1);
            var inventory = GetValue(run, "DomiNexInventory");
            var dominex = T("DomiNox.Dominex.DomiNexRegistry").GetMethod("GetById", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { "dominex" });
            inventory.GetType().GetMethod("Add", new[] { T("DomiNox.Dominex.DomiNexDefinition"), typeof(int) }).Invoke(inventory, new[] { dominex, (object)4 });

            Assert.IsFalse((bool)run.GetType().GetMethod("HasFreeDomiNexSlot").Invoke(run, null));
        }

        [Test]
        public void ConsumableSellValueDefaultsToOne()
        {
            var inventory = Activator.CreateInstance(T("DomiNox.Consumables.ConsumableInventory"));
            inventory.GetType().GetMethod("Add", new[] { typeof(string), typeof(int) }).Invoke(inventory, new object[] { "emerald_tile", 2 });
            var instance = inventory.GetType().GetMethod("GetInstanceAt").Invoke(inventory, new object[] { 0 });
            var value = T("DomiNox.Shop.SellValueService").GetMethod("GetConsumableSellValue", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { instance });

            Assert.AreEqual(1, value);
        }

        [Test]
        public void JackpotMeterCreatesTicketsAndKeepsOverflow()
        {
            var run = Activator.CreateInstance(T("DomiNox.Run.RunState"));
            var jackpot = GetValue(run, "Jackpot");
            jackpot.GetType().GetProperty("Meter").SetValue(jackpot, 85);

            T("DomiNox.Jackpot.JackpotMeterService").GetMethod("AddJackpotMeter").Invoke(null, new[] { run, (object)25, Enum.Parse(T("DomiNox.Jackpot.JackpotGainSource"), "MeterRefund") });

            Assert.AreEqual(10, GetInt(jackpot, "Meter"));
            Assert.AreEqual(1, GetInt(jackpot, "SpinTickets"));
        }

        [Test]
        public void JackpotLuckAdjustsWeights()
        {
            var state = Activator.CreateInstance(T("DomiNox.Jackpot.JackpotState"));
            state.GetType().GetProperty("JackpotLuck").SetValue(state, 3);
            var weights = (IEnumerable)T("DomiNox.Jackpot.JackpotSpinService").GetMethod("GetWeights").Invoke(null, new[] { state });
            var pairs = weights.Cast<object>().ToDictionary(item => GetString(GetValue(item, "Key"), null), item => (int)GetValue(item, "Value"));

            Assert.AreEqual(22, pairs["Blank"]);
            Assert.AreEqual(8, pairs["Crown"]);
            Assert.AreEqual(5, pairs["Seven"]);
        }

        [Test]
        public void MachineHeatFiveGuaranteesPairOrBetter()
        {
            var state = Activator.CreateInstance(T("DomiNox.Jackpot.JackpotState"));
            state.GetType().GetProperty("MachineHeat").SetValue(state, 5);
            var random = Activator.CreateInstance(typeof(Random), 42);
            var result = T("DomiNox.Jackpot.JackpotSpinService").GetMethod("Spin").Invoke(null, new[] { state, random });

            var symbols = new[] { GetString(GetValue(result, "Symbol1"), null), GetString(GetValue(result, "Symbol2"), null), GetString(GetValue(result, "Symbol3"), null) };
            Assert.IsTrue(symbols.GroupBy(symbol => symbol).Any(group => group.Count() >= 2));
            Assert.AreEqual(0, GetInt(state, "MachineHeat"));
        }

        [Test]
        public void EnsureAtLeastPairForcesThirdSymbolToFirst()
        {
            var symbolType = T("DomiNox.Jackpot.JackpotSymbol");
            var symbols = Array.CreateInstance(symbolType, 3);
            symbols.SetValue(Enum.Parse(symbolType, "Coin"), 0);
            symbols.SetValue(Enum.Parse(symbolType, "Gem"), 1);
            symbols.SetValue(Enum.Parse(symbolType, "Crown"), 2);

            var result = (Array)T("DomiNox.Jackpot.JackpotSpinService").GetMethod("EnsureAtLeastPair").Invoke(null, new object[] { symbols });

            Assert.AreEqual("Coin", result.GetValue(0).ToString());
            Assert.AreEqual("Gem", result.GetValue(1).ToString());
            Assert.AreEqual("Coin", result.GetValue(2).ToString());
        }

        [Test]
        public void TierIsCalculatedAfterGuaranteeAsPair()
        {
            var symbolType = T("DomiNox.Jackpot.JackpotSymbol");
            var listType = typeof(System.Collections.Generic.List<>).MakeGenericType(symbolType);
            var symbols = (IList)Activator.CreateInstance(listType);
            symbols.Add(Enum.Parse(symbolType, "Coin"));
            symbols.Add(Enum.Parse(symbolType, "Gem"));
            symbols.Add(Enum.Parse(symbolType, "Coin"));

            var tier = T("DomiNox.Jackpot.JackpotSpinService").GetMethod("GetTier").Invoke(null, new object[] { symbols });

            Assert.AreEqual("Pair", tier.ToString());
        }

        [Test]
        public void TripleTierResetsMachineHeat()
        {
            var state = Activator.CreateInstance(T("DomiNox.Jackpot.JackpotState"));
            var random = new AlwaysBlankRandom();
            state.GetType().GetProperty("MachineHeat").SetValue(state, 2);

            T("DomiNox.Jackpot.JackpotSpinService").GetMethod("Spin").Invoke(null, new object[] { state, random });

            Assert.AreEqual(0, GetInt(state, "MachineHeat"));
        }

        private static object DetectDesign(IList placed)
        {
            var detector = Activator.CreateInstance(T("DomiNox.Patterns.PatternDetector"));
            var method = detector.GetType().GetMethod("DetectPatternInfos", new[] { placed.GetType(), typeof(int) })
                ?? detector.GetType().GetMethod("DetectPatternInfos", new[] { typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Grid.PlacedDomino")), typeof(int) });
            var result = (IEnumerable)method.Invoke(detector, new object[] { placed, 5 });
            return result.Cast<object>().FirstOrDefault(pattern => GetString(GetValue(pattern, "Category"), null) == "Design");
        }

        private static IList CreateLoop(int value)
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("top", value, value, 0, 0, "HorizontalRight"));
            placed.Add(Domino("right", value, value, 2, 0, "VerticalDown"));
            placed.Add(Domino("bottom", value, value, 1, 2, "HorizontalRight"));
            placed.Add(Domino("left", value, value, 0, 1, "VerticalDown"));
            return placed;
        }

        private static IList CreateCross(int value)
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("center", value, value, 2, 1, "VerticalDown"));
            placed.Add(Domino("top", value, value, 2, 0, "HorizontalRight"));
            placed.Add(Domino("left", value, value, 0, 1, "HorizontalRight"));
            placed.Add(Domino("right", value, value, 3, 1, "HorizontalRight"));
            return placed;
        }

        private static IList CreateBigLoop(int value)
        {
            var placed = CreatePlacedList();
            placed.Add(Domino("top_left", value, value, 0, 0, "HorizontalRight"));
            placed.Add(Domino("top_right", value, value, 2, 0, "HorizontalRight"));
            placed.Add(Domino("right", value, value, 3, 1, "VerticalDown"));
            placed.Add(Domino("bottom_right", value, value, 2, 3, "HorizontalRight"));
            placed.Add(Domino("bottom_left", value, value, 0, 3, "HorizontalRight"));
            placed.Add(Domino("left", value, value, 0, 1, "VerticalDown"));
            return placed;
        }

        private static IList CreatePlacedList()
        {
            return (IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Grid.PlacedDomino")));
        }

        private static object Domino(string id, int left, int right, int x, int y, string orientationName)
        {
            var definition = Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), id, left, right);
            var instance = Activator.CreateInstance(T("DomiNox.Dominoes.DominoInstance"), id, definition);
            var position = Activator.CreateInstance(T("DomiNox.Grid.GridPosition"), x, y);
            var orientation = Enum.Parse(T("DomiNox.Grid.DominoOrientation"), orientationName);
            return Activator.CreateInstance(T("DomiNox.Grid.PlacedDomino"), instance, position, orientation);
        }

        private static object ModifiedDomino(string modifierId)
        {
            var definition = Activator.CreateInstance(T("DomiNox.Dominoes.DominoDefinition"), $"domino_2_5_{modifierId}", 2, 5);
            var instance = Activator.CreateInstance(T("DomiNox.Dominoes.DominoInstance"), $"domino_{modifierId}", definition, modifierId);
            var position = Activator.CreateInstance(T("DomiNox.Grid.GridPosition"), 0, 0);
            var orientation = Enum.Parse(T("DomiNox.Grid.DominoOrientation"), "HorizontalRight");
            return Activator.CreateInstance(T("DomiNox.Grid.PlacedDomino"), instance, position, orientation);
        }

        private static object CalculateSingleModifiedDominoScore(string modifierId)
        {
            var placed = CreatePlacedList();
            placed.Add(ModifiedDomino(modifierId));
            var detector = Activator.CreateInstance(T("DomiNox.Patterns.PatternDetector"));
            var calculator = Activator.CreateInstance(T("DomiNox.Scoring.ScoreCalculator"), detector, null);
            return calculator.GetType().GetMethod("Calculate", new[] { placed.GetType(), typeof(int) })?.Invoke(calculator, new object[] { placed, 5 })
                ?? calculator.GetType().GetMethod("Calculate", new[] { typeof(System.Collections.Generic.List<>).MakeGenericType(T("DomiNox.Grid.PlacedDomino")), typeof(int) }).Invoke(calculator, new object[] { placed, 5 });
        }

        private static Type T(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(name)).First(type => type != null);
        }

        private static object GetValue(object instance, string property)
        {
            return property == null ? instance : instance?.GetType().GetProperty(property)?.GetValue(instance);
        }

        private static string GetString(object instance, string property)
        {
            return GetValue(instance, property)?.ToString();
        }

        private static int GetInt(object instance, string property)
        {
            return (int)GetValue(instance, property);
        }

        private static bool GetBool(object instance, string property)
        {
            return (bool)GetValue(instance, property);
        }

        private sealed class AlwaysBlankRandom : Random
        {
            public override int Next(int maxValue)
            {
                return 0;
            }
        }
    }
}
