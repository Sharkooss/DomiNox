using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Bosses;
using DomiNox.Consumables;
using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Grid;
using DomiNox.Patterns;
using DomiNox.Scoring;
using DomiNox.Shop;
using UnityEngine;

namespace DomiNox.Run
{
    public sealed class GameFlowController : MonoBehaviour
    {
        private readonly PatternDetector patternDetector = new PatternDetector();
        private readonly DomiNexEffectEngine dominexEffectEngine = new DomiNexEffectEngine();
        private readonly DomiNexShopService shopService = new DomiNexShopService();
        private readonly System.Random random = new System.Random();
        private ScoreCalculator scoreCalculator;
        private ScoreResult lastScoreResult;
        private DominoInstance selectedDomino;
        private int selectedConsumableIndex = -1;
        private readonly HashSet<DominoInstance> selectedForDiscard = new HashSet<DominoInstance>();

        public event Action<RunState, ScoreResult, string> StateChanged;

        public RunState Run { get; private set; }
        public DominoOrientation CurrentOrientation { get; private set; } = DominoOrientation.HorizontalRight;
        public DominoInstance SelectedDomino => selectedDomino;
        public IReadOnlyCollection<DominoInstance> SelectedForDiscard => selectedForDiscard;
        public int SelectedConsumableIndex => selectedConsumableIndex;
        public OpenBoosterPackState OpenBoosterPack { get; private set; }
        public bool BossIntroActive { get; private set; }
        public bool IsScoring { get; private set; }
        public ScoreResult LastScoreResult => lastScoreResult;

        private void Awake()
        {
            scoreCalculator = new ScoreCalculator(patternDetector, dominexEffectEngine);
            InitializeRun();
        }

        public void InitializeRun()
        {
            Run = new RunState { CurrentLevel = new LevelState() };
            Run.DomiNexInventory.SetActive(Array.Empty<DomiNexDefinition>());
            selectedConsumableIndex = -1;
            dominexEffectEngine.ApplyRunStart(Run.DomiNexInventory, Run, null);

            lastScoreResult = new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string> { "Choisis la prochaine table." });
            OpenFloorProgress(1);
            Notify("Etage 1: choisis la premiere table.");
        }

        public void SelectDomino(DominoInstance domino)
        {
            if (Run.Phase != RunPhase.PlayingLevel || BossIntroActive || IsScoring)
            {
                return;
            }

            if (Run.CurrentLevel.Hand.Contains(domino))
            {
                if (IsLockedByBoss(domino))
                {
                    Notify("Domino verrouille: impossible a defausser.");
                    return;
                }

                if (!selectedForDiscard.Add(domino))
                {
                    selectedForDiscard.Remove(domino);
                }

                Notify($"Selection discard: {selectedForDiscard.Count} domino(s).");
            }
        }

        public void BeginDragDomino(DominoInstance domino)
        {
            if (Run.Phase != RunPhase.PlayingLevel || BossIntroActive)
            {
                return;
            }

            if (Run.CurrentLevel.Hand.Contains(domino))
            {
                if (IsBannedByBoss(domino))
                {
                    Notify("Valeur bannie.");
                    return;
                }

                selectedDomino = domino;
                selectedForDiscard.Remove(domino);
            }
        }

        public void DiscardSelectedDominoes()
        {
            var level = Run.CurrentLevel;
            if (Run.Phase != RunPhase.PlayingLevel || IsScoring)
            {
                Notify("Le shop est ouvert.");
                return;
            }

            if (BossIntroActive)
            {
                return;
            }

            if (level.DiscardsRemaining <= 0)
            {
                Notify("Plus aucun discard disponible.");
                return;
            }

            if (selectedForDiscard.Count == 0)
            {
                Notify("Selectionne au moins un domino a discard.");
                return;
            }

            var discarded = selectedForDiscard.Where(level.Hand.Contains).Where(domino => !IsLockedByBoss(domino)).ToList();
            if (discarded.Count == 0)
            {
                Notify("Les dominos selectionnes sont verrouilles.");
                selectedForDiscard.Clear();
                return;
            }

            foreach (var domino in discarded)
            {
                level.Hand.RemoveDomino(domino);
                Run.Bag.Discard(domino);
            }

            foreach (var domino in Run.Bag.Draw(discarded.Count))
            {
                level.Hand.AddDomino(domino);
            }

            level.DiscardsRemaining--;

            level.DiscardsUsed++;
            selectedForDiscard.Clear();
            selectedDomino = null;
            Notify($"{discarded.Count} domino(s) discard. {level.DiscardsRemaining} discard(s) restant(s).");
        }

        public void ToggleOrientation()
        {
            if (Run.Phase != RunPhase.PlayingLevel || BossIntroActive || IsScoring)
            {
                return;
            }

            RotateRight();
            Notify($"Orientation: {CurrentOrientation}");
        }

        public void RotateRightWithoutNotify()
        {
            RotateRight();
        }

        public void RotateLeftWithoutNotify()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.HorizontalRight
                ? DominoOrientation.VerticalUp
                : (DominoOrientation)((int)CurrentOrientation - 1);
        }

        private void RotateRight()
        {
            CurrentOrientation = CurrentOrientation == DominoOrientation.VerticalUp
                ? DominoOrientation.HorizontalRight
                : (DominoOrientation)((int)CurrentOrientation + 1);
        }

        public void TryPlaceSelected(int x, int y)
        {
            var level = Run.CurrentLevel;
            if (Run.Phase != RunPhase.PlayingLevel || IsScoring)
            {
                Notify("Le shop est ouvert.");
                return;
            }

            if (BossIntroActive)
            {
                return;
            }

            if (selectedDomino == null)
            {
                Notify("Aucun domino selectionne.");
                return;
            }

            if (IsBannedByBoss(selectedDomino))
            {
                Notify("Valeur bannie.");
                return;
            }

            if (level.Grid.GetPlacedDominoes().Count >= level.MaxPlacedDominoes)
            {
                Notify("Limite de placement atteinte.");
                return;
            }

            var position = new GridPosition(x, y);
            if (!level.Grid.PlaceDomino(selectedDomino, position, CurrentOrientation))
            {
                Notify("Placement impossible: connecte une valeur identique.");
                return;
            }

            level.Hand.RemoveDomino(selectedDomino);
            selectedDomino = null;
            Notify("Domino place.");
        }

        public bool CanPlaceSelected(int x, int y)
        {
            var level = Run.CurrentLevel;
            if (Run.Phase != RunPhase.PlayingLevel || BossIntroActive || IsScoring || selectedDomino == null || level.Grid.GetPlacedDominoes().Count >= level.MaxPlacedDominoes || IsBannedByBoss(selectedDomino))
            {
                return false;
            }

            return level.Grid.CanPlaceDomino(selectedDomino, new GridPosition(x, y), CurrentOrientation);
        }

        public ScoreResult CalculateCurrentScorePreview()
        {
            var level = Run.CurrentLevel;
            var dominexContext = CreateScoringContext(level);
            return scoreCalculator.Calculate(level.Grid.GetPlacedDominoes(), level.MaxPlacedDominoes, dominexContext, level.Boss);
        }

        public ScoringPreview CalculateCurrentScoringPreview()
        {
            var level = Run.CurrentLevel;
            var disabledPatternIds = level.Boss?.Definition.RuleType == BossRuleType.DisablePatterns ? level.Boss.Definition.DisabledPatternIds : null;
            var patterns = patternDetector.DetectPatternInfos(level.Grid.GetPlacedDominoes(), level.MaxPlacedDominoes, disabledPatternIds);
            var valuePattern = patterns.FirstOrDefault(item => item.Category == PatternCategory.Value) ?? PatternCatalog.GetById(PatternNames.TileHighId);
            var designPattern = patterns.FirstOrDefault(item => item.Category == PatternCategory.Design);
            if (valuePattern != null && designPattern != null && PatternComboCatalog.TryGetCombo(valuePattern.Id, designPattern.Id, out var combo))
            {
                var valueLevel = Run.PatternLevels.GetLevel(valuePattern.Id);
                var designLevel = Run.PatternLevels.GetLevel(designPattern.Id);
                var valueBonus = PatternScalingService.GetScaledPatternBonus(valuePattern, valueLevel);
                var designBonus = PatternScalingService.GetScaledPatternBonus(designPattern, designLevel);
                return new ScoringPreview(combo.Id, combo.Name, 1, valueBonus.Count + designBonus.Count + combo.CountBonus, valueBonus.Mult + designBonus.Mult + combo.MultBonus, $"{valuePattern.Name} Lv. {valueLevel} + {designPattern.Name} Lv. {designLevel}", true);
            }

            var pattern = designPattern ?? valuePattern;
            var levelValue = Run.PatternLevels.GetLevel(pattern.Id);
            var bonus = PatternScalingService.GetScaledPatternBonus(pattern, levelValue);
            return new ScoringPreview(pattern.Id, pattern.Name, levelValue, bonus.Count, bonus.Mult);
        }

        public void ResetPlacements()
        {
            var level = Run.CurrentLevel;
            if (Run.Phase != RunPhase.PlayingLevel || IsScoring)
            {
                Notify("Le shop est ouvert.");
                return;
            }

            if (BossIntroActive)
            {
                return;
            }

            var placed = level.Grid.GetPlacedDominoes();
            level.Grid.Clear();

            foreach (var item in placed)
            {
                level.Hand.AddDomino(item.Domino);
            }

            selectedDomino = null;
            selectedForDiscard.Clear();
            level.DiscardsUsed = 0;
            level.CurrentScore = 0;
            level.IsWon = false;
            level.IsLost = false;
            lastScoreResult = new ScoreResult(0, 1, new System.Collections.Generic.List<string>(), new System.Collections.Generic.List<string> { "Placements reinitialises." });
            Notify("Placements reinitialises.");
        }

        public void ValidateScore()
        {
            var level = Run.CurrentLevel;
            if (Run.Phase != RunPhase.PlayingLevel || IsScoring)
            {
                Notify("Le shop est ouvert.");
                return;
            }

            if (BossIntroActive)
            {
                return;
            }

            var dominexContext = CreateScoringContext(level);
            var score = scoreCalculator.Calculate(level.Grid.GetPlacedDominoes(), level.MaxPlacedDominoes, dominexContext, level.Boss);
            lastScoreResult = score.WithPostScoringEffects(RollPostScoringDomiNexEffects(score));
            IsScoring = true;
            selectedDomino = null;
            selectedForDiscard.Clear();
            Notify("Scoring...");
        }

        public void CompleteScoringAnimation()
        {
            if (!IsScoring)
            {
                return;
            }

            var level = Run.CurrentLevel;
            IsScoring = false;
            Run.PatternUsage.Record(lastScoreResult.DetectedPatterns);
            var secretName = RevealSecretPatterns(lastScoreResult);
            var secretMessage = string.IsNullOrWhiteSpace(secretName) ? string.Empty : $" Secret Pattern discovered: {secretName}.";
            ApplyPostScoringDomiNexEffects(lastScoreResult);
            level.CurrentScore = lastScoreResult.FinalScore;
            level.IsWon = level.CurrentScore >= level.Quota;
            level.IsLost = !level.IsWon;
            if (level.IsWon)
            {
                OpenLevelReward();
                Notify($"Niveau reussi. Cash out disponible.{secretMessage}");
                return;
            }

            Run.Phase = RunPhase.RunLost;
            Notify($"Score insuffisant.{secretMessage}");
        }

        public void CashOutReward()
        {
            if (Run.Phase != RunPhase.LevelReward || Run.CurrentReward == null)
            {
                return;
            }

            var totalCredits = Run.CurrentReward.TotalCredits;
            Run.Credits += totalCredits;
            OpenShop();
            Notify($"Cash out: +{totalCredits} credits. Shop ouvert.");
        }

        public void BuyShopOffer(int index)
        {
            if (Run.Phase != RunPhase.Shop || Run.CurrentShop == null || index < 0 || index >= Run.CurrentShop.Offers.Count)
            {
                return;
            }

            var offer = Run.CurrentShop.Offers[index];
            if (offer.IsPurchased)
            {
                Notify("Offre deja achetee.");
                return;
            }

            if (Run.DomiNexInventory.Contains(offer.DomiNex.Id))
            {
                Notify("DomiNex deja possede.");
                return;
            }

            if (!Run.HasFreeDomiNexSlot())
            {
                Notify("Slots DomiNex pleins.");
                return;
            }

            if (!ShopPurchaseService.TrySpend(offer.Price, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants.");
                return;
            }

            Run.DomiNexInventory.Add(offer.DomiNex);
            offer.MarkPurchased();
            Notify($"DomiNex achete: {offer.DomiNex.Name}.");
        }

        public void BuyBoosterPackOffer(int index)
        {
            if (Run.Phase != RunPhase.Shop || Run.CurrentShop == null || index < 0 || index >= Run.CurrentShop.BoosterPackOffers.Count)
            {
                return;
            }

            var offer = Run.CurrentShop.BoosterPackOffers[index];
            if (offer.IsPurchased)
            {
                Notify("Offre deja achetee.");
                return;
            }

            if (!Run.HasFreeConsumableSlot())
            {
                Notify("Consumable slots full.");
                return;
            }

            var choices = GenerateBoosterPackChoices(offer.Pack);
            if (choices.Count == 0)
            {
                Notify("No Gem Tiles available.");
                return;
            }

            if (!ShopPurchaseService.TrySpend(offer.Pack.Price, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants.");
                return;
            }

            var freeSlots = GameConstants.MaxConsumableSlots - Run.ActiveConsumableCount;
            OpenBoosterPack = new OpenBoosterPackState(offer.Pack, choices, System.Math.Min(offer.Pack.PickCount, freeSlots), index);
            Notify($"{offer.Pack.Name}: choose {OpenBoosterPack.ActualPickCount}.");
        }

        public void ToggleBoosterPackChoice(int index)
        {
            if (OpenBoosterPack == null || index < 0 || index >= OpenBoosterPack.Choices.Count)
            {
                return;
            }

            if (!OpenBoosterPack.SelectedIndices.Remove(index) && OpenBoosterPack.SelectedIndices.Count < OpenBoosterPack.ActualPickCount)
            {
                OpenBoosterPack.SelectedIndices.Add(index);
            }

            Notify("Pack selection updated.");
        }

        public void ConfirmBoosterPackChoices()
        {
            if (OpenBoosterPack == null || OpenBoosterPack.SelectedIndices.Count != OpenBoosterPack.ActualPickCount)
            {
                return;
            }

            foreach (var index in OpenBoosterPack.SelectedIndices.OrderBy(index => index))
            {
                Run.Consumables.Add(OpenBoosterPack.Choices[index].Id, GameConstants.MaxConsumableSlots);
            }

            if (OpenBoosterPack.OfferIndex >= 0 && OpenBoosterPack.OfferIndex < Run.CurrentShop.BoosterPackOffers.Count)
            {
                Run.CurrentShop.BoosterPackOffers[OpenBoosterPack.OfferIndex].MarkPurchased();
            }

            var packName = OpenBoosterPack.Pack.Name;
            OpenBoosterPack = null;
            Notify($"{packName} resolved.");
        }

        private List<ConsumableDefinition> GenerateBoosterPackChoices(BoosterPackDefinition pack)
        {
            var pool = GemTileRegistry.GetAvailableGemTiles(Run).ToList();
            var choices = new List<ConsumableDefinition>();
            while (choices.Count < pack.OfferedCardCount && pool.Count > 0)
            {
                var selected = pool[random.Next(pool.Count)];
                pool.Remove(selected);
                choices.Add(selected);
            }

            return choices;
        }

        public void SelectConsumable(int index)
        {
            if (index < 0 || index >= Run.Consumables.Count)
            {
                selectedConsumableIndex = -1;
            }
            else
            {
                selectedConsumableIndex = selectedConsumableIndex == index ? -1 : index;
            }

            Notify(selectedConsumableIndex < 0 ? "Consumable deselectionne." : "Consumable selectionne.");
        }

        public void UseSelectedConsumable()
        {
            if (IsScoring || selectedConsumableIndex < 0)
            {
                return;
            }

            if (GemTileRegistry.UseConsumable(selectedConsumableIndex, Run, out var message))
            {
                selectedConsumableIndex = -1;
            }

            Notify(message);
        }

        public void ContinueAfterShop()
        {
            if (Run.Phase != RunPhase.Shop)
            {
                return;
            }

            OpenFloorProgress(Run.CurrentLevel.LevelIndex + 1);
            Notify($"Etage {Run.CurrentLevel.FloorIndex}: prochaine table disponible.");
        }

        public void RerollShop()
        {
            const int rerollPrice = 5;
            if (Run.Phase != RunPhase.Shop)
            {
                return;
            }

            if (!ShopPurchaseService.TrySpend(rerollPrice, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants pour reroll.");
                return;
            }

            Run.CurrentShop = shopService.GenerateShop(Run.DomiNexInventory, Run.CurrentLevel.FloorIndex, Run);
            Notify("Shop reroll.");
        }

        public void StartCurrentLevel()
        {
            if (Run.Phase != RunPhase.FloorProgress)
            {
                return;
            }

            StartLevel(Run.CurrentLevel.LevelIndex);
            Notify($"Niveau {Run.CurrentLevel.LevelIndex}. Quota {Run.CurrentLevel.Quota}.");
        }

        public void CompleteBossIntro()
        {
            if (!BossIntroActive)
            {
                return;
            }

            BossIntroActive = false;
            var boss = Run.CurrentLevel.Boss;
            Notify(boss == null ? "Niveau lance." : boss.GetEffectSummary());
        }

        private void OpenShop()
        {
            Run.CurrentShop = shopService.GenerateShop(Run.DomiNexInventory, Run.CurrentLevel.FloorIndex, Run);
            Run.CurrentReward = null;
            Run.Phase = RunPhase.Shop;
        }

        private void OpenLevelReward()
        {
            var level = Run.CurrentLevel;
            var discardCredits = level.DiscardsRemaining * GameConstants.CreditsPerRemainingDiscard;
            var interestCredits = Run.Credits <= 0 ? 0 : System.Math.Min(GameConstants.MaxInterestCredits, Run.Credits / GameConstants.InterestCreditStep);
            Run.CurrentReward = new LevelRewardState(GameConstants.LevelWinCredits, discardCredits, interestCredits);
            Run.Phase = RunPhase.LevelReward;
        }

        private void OpenFloorProgress(int levelIndex)
        {
            var floorIndex = GetFloorIndex(levelIndex);
            var levelInFloor = GetLevelInFloor(levelIndex);
            if (Run.CurrentFloorBoss == null || Run.CurrentLevel == null || Run.CurrentLevel.FloorIndex != floorIndex)
            {
                Run.CurrentFloorBoss = BossRegistry.GetRandom(random, Run.PreviousBossId);
                Run.PreviousBossId = Run.CurrentFloorBoss?.Id;
            }

            Run.CurrentLevel = new LevelState
            {
                FloorIndex = floorIndex,
                LevelIndex = levelIndex,
                Quota = GetQuotaForLevel(levelIndex),
                Boss = levelInFloor == GameConstants.LevelsPerFloor && Run.CurrentFloorBoss != null
                    ? new BossLevelState(Run.CurrentFloorBoss)
                    : null
            };
            Run.CurrentShop = null;
            Run.CurrentReward = null;
            Run.Phase = RunPhase.FloorProgress;
            selectedDomino = null;
            selectedForDiscard.Clear();
            selectedConsumableIndex = -1;
            BossIntroActive = false;
        }

        private void StartLevel(int levelIndex)
        {
            var floorIndex = GetFloorIndex(levelIndex);
            var levelInFloor = GetLevelInFloor(levelIndex);
            if (Run.CurrentFloorBoss == null || Run.CurrentLevel == null || Run.CurrentLevel.FloorIndex != floorIndex)
            {
                Run.CurrentFloorBoss = BossRegistry.GetRandom(random, Run.PreviousBossId);
                Run.PreviousBossId = Run.CurrentFloorBoss?.Id;
            }

            var bossDefinition = levelInFloor == GameConstants.LevelsPerFloor ? Run.CurrentFloorBoss : null;

            Run.CurrentLevel = new LevelState
            {
                FloorIndex = floorIndex,
                LevelIndex = levelIndex,
                Quota = GetQuotaForLevel(levelIndex),
                Boss = bossDefinition == null ? null : new BossLevelState(bossDefinition)
            };
            Run.CurrentShop = null;
            Run.CurrentReward = null;
            Run.Phase = RunPhase.PlayingLevel;
            selectedDomino = null;
            selectedForDiscard.Clear();
            selectedConsumableIndex = -1;
            CurrentOrientation = DominoOrientation.HorizontalRight;
            BossIntroActive = bossDefinition != null;

            dominexEffectEngine.ApplyLevelStart(Run.DomiNexInventory, Run.CurrentLevel, null);
            ApplyBossLevelStart(Run.CurrentLevel);
            Run.Bag.Initialize(DominoFactory.CreateDoubleSixSet());

            foreach (var domino in Run.Bag.Draw(GameConstants.StartingHandSize))
            {
                Run.CurrentLevel.Hand.AddDomino(domino);
            }

            ApplyBossAfterDraw(Run.CurrentLevel);
        }

        public int GetQuotaForLevel(int levelIndex)
        {
            return LevelQuotaService.GetQuotaForGlobalLevel(levelIndex);
        }

        public static int GetFloorIndex(int levelIndex)
        {
            return ((levelIndex - 1) / GameConstants.LevelsPerFloor) + 1;
        }

        public static int GetLevelInFloor(int levelIndex)
        {
            return ((levelIndex - 1) % GameConstants.LevelsPerFloor) + 1;
        }

        private void ApplyBossLevelStart(LevelState level)
        {
            var boss = level.Boss;
            if (boss == null)
            {
                return;
            }

            if (boss.Definition.RuleType == BossRuleType.BannedValue && boss.Definition.BannedValueCount > 0)
            {
                boss.BannedValue = random.Next(GameConstants.DominoMinValue, GameConstants.DominoMaxValue + 1);
            }

            if (boss.Definition.RuleType == BossRuleType.ModifyDiscards)
            {
                level.DiscardsRemaining = System.Math.Max(0, level.DiscardsRemaining + boss.Definition.DiscardsDelta);
            }
        }

        private void ApplyBossAfterDraw(LevelState level)
        {
            var boss = level.Boss;
            if (boss == null || boss.Definition.RuleType != BossRuleType.LockHandDominoes)
            {
                return;
            }

            var handDominoes = level.Hand.Dominoes.OrderBy(_ => random.Next()).Take(boss.Definition.LockedDominoCount);
            foreach (var domino in handDominoes)
            {
                boss.LockedDominoIds.Add(domino.InstanceId);
            }
        }

        public bool IsBannedByBoss(DominoInstance domino)
        {
            var boss = Run.CurrentLevel.Boss;
            return boss?.Definition.RuleType == BossRuleType.BannedValue
                && boss.BannedValue.HasValue
                && (domino.Definition.Left == boss.BannedValue.Value || domino.Definition.Right == boss.BannedValue.Value);
        }

        public bool IsLockedByBoss(DominoInstance domino)
        {
            return Run.CurrentLevel.Boss?.LockedDominoIds.Contains(domino.InstanceId) == true;
        }

        private DomiNexScoringContext CreateScoringContext(LevelState level)
        {
            return new DomiNexScoringContext(Run.DomiNexInventory.Active, Run.Credits, level.DiscardsUsed, level.DiscardsRemaining, level.MaxPlacedDominoes, patternUsageCounts: Run.PatternUsage.Counts, patternLevels: Run.PatternLevels.Levels, activeDomiNexCount: Run.ActiveDomiNexCount, maxDomiNexSlots: Run.MaxDomiNexSlots, bagDoubleCount: CountDeckDoubles(level));
        }

        public int GetMinimumAllowedCredits()
        {
            return Run.DomiNexInventory.Contains("credit_dominex") ? -20 : 0;
        }

        private List<PostScoringEffectResult> RollPostScoringDomiNexEffects(ScoreResult score)
        {
            var results = new List<PostScoringEffectResult>();
            if (Run.DomiNexInventory.Contains("space_dominex") && !string.IsNullOrWhiteSpace(score.ValuePatternId))
            {
                var triggered = ChanceUtils.RollChance(1, 4);
                results.Add(new PostScoringEffectResult("space_dominex", "Space Dominex", triggered ? $"{score.ValuePatternName} level up" : "No level up", triggered, score.ValuePatternId));
            }

            if (Run.DomiNexInventory.Contains("gros_michel"))
            {
                var triggered = ChanceUtils.RollChance(1, 6);
                results.Add(new PostScoringEffectResult("gros_michel", "Gros Michel", triggered ? "Destroyed" : "Survived", triggered));
            }

            return results;
        }

        private string RevealSecretPatterns(ScoreResult score)
        {
            if (score.DesignPatternId == PatternNames.ChristCrossId && Run.RevealedSecretPatterns.Add(PatternNames.ChristCrossId))
            {
                return PatternNames.ChristCross;
            }

            if (score.DesignPatternId == PatternNames.BigLoopId && Run.RevealedSecretPatterns.Add(PatternNames.BigLoopId))
            {
                return PatternNames.BigLoop;
            }

            return null;
        }

        private void ApplyPostScoringDomiNexEffects(ScoreResult score)
        {
            foreach (var effect in score.PostScoringEffects)
            {
                if (!effect.Triggered)
                {
                    continue;
                }

                if (effect.SourceId == "space_dominex")
                {
                    Run.PatternLevels.Increase(effect.PatternId);
                }
                else if (effect.SourceId == "gros_michel")
                {
                    Run.DomiNexInventory.Remove("gros_michel");
                }
            }
        }

        private int CountDeckDoubles(LevelState level)
        {
            var placed = level.Grid.GetPlacedDominoes().Select(item => item.Domino);
            return Run.Bag.RemainingDominoes
                .Concat(Run.Bag.DiscardedDominoes)
                .Concat(level.Hand.Dominoes)
                .Concat(placed)
                .GroupBy(domino => domino.InstanceId)
                .Select(group => group.First())
                .Count(domino => domino.Definition.IsDouble);
        }

        private void Notify(string message)
        {
            var score = Run.Phase == RunPhase.PlayingLevel ? CalculateCurrentScorePreview() : lastScoreResult;
            StateChanged?.Invoke(Run, score, message);
        }
    }
}
