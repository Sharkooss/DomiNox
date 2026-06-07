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
    }
}
