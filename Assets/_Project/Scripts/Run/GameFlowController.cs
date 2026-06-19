using System;
using System.Collections.Generic;
using System.Linq;
using DomiNox.Bosses;
using DomiNox.Consumables;
using DomiNox.Core;
using DomiNox.Dominoes;
using DomiNox.Dominex;
using DomiNox.Grid;
using DomiNox.Jackpot;
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
        private readonly DominoPackGenerator dominoPackGenerator = new DominoPackGenerator();
        private readonly System.Random random = new System.Random();
        private ScoreCalculator scoreCalculator;
        private ScoreResult lastScoreResult;
        private JackpotSpinResult lastJackpotSpinResult;
        private JackpotSpinResult pendingJackpotSpinResult;
        private DominoInstance selectedDomino;
        private int selectedDomiNexIndex = -1;
        private int selectedConsumableIndex = -1;
        private readonly HashSet<DominoInstance> selectedForDiscard = new HashSet<DominoInstance>();

        public event Action<RunState, ScoreResult, string> StateChanged;

        public RunState Run { get; private set; }
        public DominoOrientation CurrentOrientation { get; private set; } = DominoOrientation.HorizontalRight;
        public DominoInstance SelectedDomino => selectedDomino;
        public IReadOnlyCollection<DominoInstance> SelectedForDiscard => selectedForDiscard;
        public int SelectedDomiNexIndex => selectedDomiNexIndex;
        public int SelectedConsumableIndex => selectedConsumableIndex;
        public OpenBoosterPackState OpenBoosterPack { get; private set; }
        public MajorJackpotState OpenMajorJackpot { get; private set; }
        public JackpotSpinResult LastJackpotSpinResult => lastJackpotSpinResult;
        public JackpotSpinResult PendingJackpotSpinResult => pendingJackpotSpinResult;
        public bool HasPendingJackpotReward => pendingJackpotSpinResult != null;
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
            selectedDomiNexIndex = -1;
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
                level.DiscardedThisLevel.Add(domino);
            }

            foreach (var domino in Run.Bag.Draw(discarded.Count))
            {
                level.Hand.AddDomino(domino);
            }

            level.DiscardsRemaining--;
            level.DiscardsUsed++;
            selectedForDiscard.Clear();
            selectedDomino = null;
            Notify($"{discarded.Count} domino(s) discard. {level.DiscardsRemaining} discard(s) restant(s). Bag {Run.Bag.RemainingCount()}.");
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

            if (!DominoModifierRegistry.IsLight(selectedDomino) && CountPlacedAgainstLimit(level) >= level.MaxPlacedDominoes)
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
            if (Run.Phase != RunPhase.PlayingLevel || BossIntroActive || IsScoring || selectedDomino == null || (!DominoModifierRegistry.IsLight(selectedDomino) && CountPlacedAgainstLimit(level) >= level.MaxPlacedDominoes) || IsBannedByBoss(selectedDomino))
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
            var secretUnlocked = !string.IsNullOrWhiteSpace(secretName);
            var secretMessage = string.IsNullOrWhiteSpace(secretName) ? string.Empty : $" Secret Pattern discovered: {secretName}.";
            var placedDominoes = level.Grid.GetPlacedDominoes();
            var scoreBeforeHand = level.CurrentScore;
            level.CurrentScore += lastScoreResult.FinalScore;
            level.HandsRemaining = System.Math.Max(0, level.HandsRemaining - 1);
            var jackpotGain = JackpotMeterService.ApplyScoringGains(Run, lastScoreResult, placedDominoes, secretUnlocked, random, scoreBeforeHand, level.CurrentScore);
            jackpotGain += ApplyDominoModifierPostScoringEffects(lastScoreResult, level.IsWon);
            var jackpotMessage = jackpotGain <= 0 ? string.Empty : $" Jackpot +{jackpotGain}.";
            ApplyPostScoringDomiNexEffects(lastScoreResult);
            level.IsWon = level.CurrentScore >= level.Quota;
            foreach (var placed in placedDominoes)
            {
                level.PlayedThisLevel.Add(placed.Domino);
            }

            level.Grid.Clear();
            if (level.IsWon)
            {
                AdvanceJackpotLevelEffects();
                var goldCredits = AwardGoldDominoCredits(level);
                OpenLevelReward();
                Notify($"Niveau reussi. Score {level.CurrentScore}/{level.Quota}. Cash out disponible.{(goldCredits > 0 ? $" Gold +{goldCredits} credits." : string.Empty)}{secretMessage}{jackpotMessage}");
                return;
            }

            if (level.HandsRemaining <= 0)
            {
                level.IsLost = true;
                Run.Phase = RunPhase.RunLost;
                Notify($"Score insuffisant: {level.CurrentScore}/{level.Quota}.{secretMessage}{jackpotMessage}");
                return;
            }

            PrepareNextHand(level);
            Notify($"Main score +{lastScoreResult.FinalScore}. Total {level.CurrentScore}/{level.Quota}. {level.HandsRemaining} hand(s) left.{secretMessage}{jackpotMessage}");
        }

        public void CashOutReward()
        {
            if (Run.Phase != RunPhase.LevelReward || Run.CurrentReward == null)
            {
                return;
            }

            var totalCredits = RunEconomyService.AddCredits(Run, Run.CurrentReward.TotalCredits, CreditSource.LevelReward);
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
            if (offer.Type == ShopOfferType.Domino)
            {
                BuyDominoShopOffer(offer);
                return;
            }

            var price = GetShopOfferPrice(index);
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

            if (!ShopPurchaseService.TrySpend(price, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants.");
                return;
            }

            Run.DomiNexInventory.Add(offer.DomiNex, price);
            offer.MarkPurchased();
            if (Run.NextShopHasFreeDomiNex && price == 0)
            {
                Run.NextShopHasFreeDomiNex = false;
            }

            if (Run.NextShopDiscountPercent > 0)
            {
                Run.NextShopDiscountPercent = 0;
            }

            Notify($"DomiNex achete: {offer.DomiNex.Name}.");
        }

        private void BuyDominoShopOffer(ShopOffer offer)
        {
            if (offer == null || offer.Type != ShopOfferType.Domino || offer.Domino == null)
            {
                return;
            }

            if (offer.IsPurchased)
            {
                Notify("Offre deja achetee.");
                return;
            }

            if (!ShopPurchaseService.TrySpend(offer.Price, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants.");
                return;
            }

            var instanceId = $"shop_domino_{Run.PurchasedDominoes.Count}";
            Run.PurchasedDominoes.Add(new DominoInstance(instanceId, offer.Domino));
            offer.MarkPurchased();
            Notify($"Added {offer.Domino} to bag.");
        }

        public int GetShopOfferPrice(int index)
        {
            if (Run.CurrentShop == null || index < 0 || index >= Run.CurrentShop.Offers.Count)
            {
                return 0;
            }

            var price = Run.CurrentShop.Offers[index].Price;
            if (Run.NextShopHasFreeDomiNex)
            {
                return 0;
            }

            if (Run.NextShopDiscountPercent > 0)
            {
                price = System.Math.Max(0, price * (100 - Run.NextShopDiscountPercent) / 100);
            }

            return price;
        }

        public void SelectDomiNex(int index)
        {
            selectedDomiNexIndex = Run.DomiNexInventory.GetInstanceAt(index) != null && selectedDomiNexIndex != index ? index : -1;
            selectedConsumableIndex = -1;
            Notify(selectedDomiNexIndex < 0 ? "DomiNex deselectionne." : "DomiNex selectionne.");
        }

        public void SellSelectedDomiNex()
        {
            SellDomiNex(selectedDomiNexIndex);
        }

        public void SellDomiNex(int index)
        {
            var instance = Run.DomiNexInventory.GetInstanceAt(index);
            if (instance == null)
            {
                Notify("Slot DomiNex vide.");
                return;
            }

            var sellValue = SellValueService.GetDomiNexSellValue(instance);
            var name = instance.Definition.Name;
            if (!Run.DomiNexInventory.RemoveAt(index))
            {
                Notify("Slot DomiNex vide.");
                return;
            }

            RunEconomyService.AddCredits(Run, sellValue, CreditSource.SellDomiNex);
            selectedDomiNexIndex = -1;
            Notify($"{name} vendu: +{sellValue} credits.");
        }

        public JackpotSpinResult BeginJackpotSpin()
        {
            if (Run.Jackpot.SpinTickets <= 0 || OpenBoosterPack != null || OpenMajorJackpot != null || IsScoring || pendingJackpotSpinResult != null)
            {
                Notify("No Spin Ticket available.");
                return null;
            }

            Run.Jackpot.SpinTickets--;
            pendingJackpotSpinResult = JackpotSpinService.Spin(Run.Jackpot, random);
            Notify("Jackpot spinning...");
            return pendingJackpotSpinResult;
        }

        public void RevealPendingJackpotSpinResult()
        {
            if (pendingJackpotSpinResult == null)
            {
                return;
            }

            lastJackpotSpinResult = pendingJackpotSpinResult;
            Notify($"Jackpot result ready: {pendingJackpotSpinResult.Tier}.");
        }

        public void CollectPendingJackpotReward()
        {
            if (pendingJackpotSpinResult == null)
            {
                return;
            }

            var result = pendingJackpotSpinResult;
            pendingJackpotSpinResult = null;
            if (result.Tier == JackpotRewardTier.MajorJackpot)
            {
                OpenMajorJackpot = new MajorJackpotState(JackpotRewardCatalog.PickMajorOptions(random, 5));
                Notify("MAJOR JACKPOT! Choose 2 rewards.");
                return;
            }

            var message = JackpotRewardService.ApplyReward(Run, result.Reward, random, OpenFreeBoosterPack);
            Notify($"Jackpot reward collected. {message}");
        }

        public void ToggleMajorJackpotReward(int index)
        {
            if (OpenMajorJackpot == null || index < 0 || index >= OpenMajorJackpot.Options.Count)
            {
                return;
            }

            if (!OpenMajorJackpot.SelectedIndices.Remove(index) && OpenMajorJackpot.SelectedIndices.Count < 2)
            {
                OpenMajorJackpot.SelectedIndices.Add(index);
            }

            Notify("Major Jackpot selection updated.");
        }

        public void ConfirmMajorJackpotRewards()
        {
            if (OpenMajorJackpot == null || OpenMajorJackpot.SelectedIndices.Count != 2)
            {
                return;
            }

            var messages = OpenMajorJackpot.SelectedIndices
                .OrderBy(index => index)
                .Select(index => JackpotRewardService.ApplyReward(Run, OpenMajorJackpot.Options[index], random, OpenFreeBoosterPack))
                .ToList();
            OpenMajorJackpot = null;
            Notify($"Major Jackpot resolved. {string.Join(" ", messages)}");
        }

        public void SwapDomiNexSlots(int fromIndex, int toIndex)
        {
            if (!Run.DomiNexInventory.SwapSlots(fromIndex, toIndex))
            {
                return;
            }

            if (selectedDomiNexIndex == fromIndex)
            {
                selectedDomiNexIndex = toIndex;
            }
            else if (selectedDomiNexIndex == toIndex)
            {
                selectedDomiNexIndex = fromIndex;
            }

            Notify("DomiNex reordered.");
        }

        public void BuyBoosterPackOffer(int index)
        {
            if (Run.Phase != RunPhase.Shop || Run.CurrentShop == null || index < 0 || index >= Run.CurrentShop.BoosterPackOffers.Count)
            {
                return;
            }

            if (OpenBoosterPack != null)
            {
                Notify("Resolve the open Booster Pack first.");
                return;
            }

            var offer = Run.CurrentShop.BoosterPackOffers[index];
            if (offer.IsPurchased)
            {
                Notify("Offre deja achetee.");
                return;
            }

            if (!HasFreeSlotForPack(offer.Pack))
            {
                Notify(offer.Pack.ContentType == BoosterPackContentType.DomiNex ? "DomiNex slots full." : "Consumable slots full.");
                return;
            }

            var choiceCount = GenerateOpenPackState(offer.Pack, index, out var openPack);
            if (choiceCount == 0)
            {
                Notify(offer.Pack.ContentType == BoosterPackContentType.DomiNex ? "No DomiNex available." : "No Gem Tiles available.");
                return;
            }

            if (!ShopPurchaseService.TrySpend(offer.Pack.Price, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants.");
                return;
            }

            OpenBoosterPack = openPack;
            Notify(offer.Pack.PickMin == 0 ? $"{offer.Pack.Name}: choose up to {OpenBoosterPack.ActualPickCount}." : $"{offer.Pack.Name}: choose {OpenBoosterPack.ActualPickCount}.");
        }

        public void ToggleBoosterPackChoice(int index)
        {
            if (OpenBoosterPack == null || index < 0 || index >= OpenBoosterPack.ChoiceCount)
            {
                return;
            }

            if (!OpenBoosterPack.SelectedIndices.Remove(index) && OpenBoosterPack.SelectedIndices.Count < OpenBoosterPack.MaxPickCount)
            {
                OpenBoosterPack.SelectedIndices.Add(index);
            }

            Notify("Pack selection updated.");
        }

        public void ConfirmBoosterPackChoices()
        {
            if (OpenBoosterPack == null || OpenBoosterPack.SelectedIndices.Count < OpenBoosterPack.MinPickCount || OpenBoosterPack.SelectedIndices.Count > OpenBoosterPack.MaxPickCount)
            {
                return;
            }

            foreach (var index in OpenBoosterPack.SelectedIndices.OrderBy(index => index))
            {
                if (OpenBoosterPack.Pack.ContentType == BoosterPackContentType.DomiNex)
                {
                    var dominex = OpenBoosterPack.DomiNexChoices[index];
                    Run.DomiNexInventory.Add(dominex, DomiNexShopService.GetPackVirtualPurchasePrice(dominex.Rarity));
                }
                else if (OpenBoosterPack.Pack.ContentType == BoosterPackContentType.Domino)
                {
                    var domino = OpenBoosterPack.DominoChoices[index];
                    Run.PurchasedDominoes.Add(new DominoInstance($"pack_domino_{Run.PurchasedDominoes.Count}", domino.Definition, domino.ModifierId));
                }
                else
                {
                    Run.Consumables.Add(OpenBoosterPack.Choices[index].Id, Run.MaxConsumableSlots, 1);
                }
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

        private int GenerateOpenPackState(BoosterPackDefinition pack, int offerIndex, out OpenBoosterPackState openPack)
        {
            if (pack.ContentType == BoosterPackContentType.DomiNex)
            {
                var choices = shopService.GenerateDomiNexPackChoices(Run.DomiNexInventory, Run.CurrentLevel.FloorIndex, pack.OfferedCardCount);
                var freeSlots = Run.MaxDomiNexSlots - Run.ActiveDomiNexCount;
                var actualPickCount = System.Math.Min(System.Math.Min(pack.PickMax, freeSlots), choices.Count);
                var minPickCount = System.Math.Min(pack.PickMin, actualPickCount);
                openPack = new OpenBoosterPackState(pack, choices, actualPickCount, offerIndex, minPickCount);
                return choices.Count;
            }

            if (pack.ContentType == BoosterPackContentType.Domino)
            {
                var choices = dominoPackGenerator.GenerateChoices(pack.OfferedCardCount);
                var actualPickCount = System.Math.Min(pack.PickMax, choices.Count);
                var minPickCount = System.Math.Min(pack.PickMin, actualPickCount);
                openPack = new OpenBoosterPackState(pack, choices, actualPickCount, offerIndex, minPickCount);
                return choices.Count;
            }

            var consumableChoices = GenerateBoosterPackChoices(pack);
            var consumableFreeSlots = Run.MaxConsumableSlots - Run.ActiveConsumableCount;
            var consumableActualPickCount = System.Math.Min(System.Math.Min(pack.PickMax, consumableFreeSlots), consumableChoices.Count);
            var consumableMinPickCount = System.Math.Min(pack.PickMin, consumableActualPickCount);
            openPack = new OpenBoosterPackState(pack, consumableChoices, consumableActualPickCount, offerIndex, consumableMinPickCount);
            return consumableChoices.Count;
        }

        private bool HasFreeSlotForPack(BoosterPackDefinition pack)
        {
            return pack.ContentType == BoosterPackContentType.DomiNex ? Run.HasFreeDomiNexSlot() : pack.ContentType == BoosterPackContentType.Domino || Run.HasFreeConsumableSlot();
        }

        public bool HasAvailableChoicesForPack(BoosterPackDefinition pack)
        {
            if (pack == null)
            {
                return false;
            }

            return pack.ContentType == BoosterPackContentType.DomiNex
                ? shopService.CountAvailableDomiNexPackChoices(Run.DomiNexInventory) > 0
                : pack.ContentType == BoosterPackContentType.Domino || GemTileRegistry.GetAvailableGemTiles(Run).Any();
        }

        private void OpenFreeBoosterPack(BoosterPackDefinition pack)
        {
            if (pack == null || OpenBoosterPack != null || !HasFreeSlotForPack(pack))
            {
                return;
            }

            var choiceCount = GenerateOpenPackState(pack, -1, out var openPack);
            if (choiceCount == 0)
            {
                return;
            }

            OpenBoosterPack = openPack;
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

            if (selectedConsumableIndex >= 0)
            {
                selectedDomiNexIndex = -1;
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

        public void SellSelectedConsumable()
        {
            var instance = Run.Consumables.GetInstanceAt(selectedConsumableIndex);
            if (instance == null)
            {
                Notify("Slot consommable vide.");
                return;
            }

            var definition = GemTileRegistry.GetById(instance.DefinitionId);
            var sellValue = SellValueService.GetConsumableSellValue(instance);
            Run.Consumables.RemoveAt(selectedConsumableIndex);
            RunEconomyService.AddCredits(Run, sellValue, CreditSource.SellConsumable);
            selectedConsumableIndex = -1;
            Notify($"{definition?.Name ?? "Consumable"} vendu: +{sellValue} credits.");
        }

        public void ContinueAfterShop()
        {
            if (Run.Phase != RunPhase.Shop)
            {
                return;
            }

            if (OpenBoosterPack != null)
            {
                Notify("Resolve the open Booster Pack first.");
                return;
            }

            Run.NextShopInfiniteFreeRerolls = false;
            Run.NextShopFreeReroll = false;
            OpenFloorProgress(Run.CurrentLevel.LevelIndex + 1);
            Notify($"Etage {Run.CurrentLevel.FloorIndex}: prochaine table disponible.");
        }

        public void RerollShop()
        {
            if (Run.Phase != RunPhase.Shop)
            {
                return;
            }

            if (OpenBoosterPack != null)
            {
                Notify("Resolve the open Booster Pack first.");
                return;
            }

            var rerollPrice = GetCurrentRerollCost();
            if (!ShopPurchaseService.TrySpend(rerollPrice, Run, GetMinimumAllowedCredits()))
            {
                Notify("Credits insuffisants pour reroll.");
                return;
            }

            shopService.RerollDirectDomiNexOffers(Run.CurrentShop, Run.DomiNexInventory, Run.CurrentLevel.FloorIndex);
            if (ShopRerollCostService.ShouldCountSuccessfulReroll(Run, Run.CurrentShop, rerollPrice))
            {
                Run.CurrentShop.RerollCountThisShop++;
            }

            if (Run.NextShopFreeReroll)
            {
                Run.NextShopFreeReroll = false;
            }

            Notify("Shop reroll.");
        }

        public int GetCurrentRerollCost()
        {
            return ShopRerollCostService.GetCurrentRerollCost(Run, Run.CurrentShop);
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
                Run.DoubleCreditsUntilEndOfFloor = false;
            }

            Run.CurrentLevel = new LevelState
            {
                FloorIndex = floorIndex,
                LevelIndex = levelIndex,
                Quota = GetAdjustedQuota(levelIndex, floorIndex, levelInFloor),
                Boss = levelInFloor == GameConstants.LevelsPerFloor && Run.CurrentFloorBoss != null
                    ? new BossLevelState(Run.CurrentFloorBoss)
                    : null
            };
            Run.CurrentShop = null;
            Run.CurrentReward = null;
            Run.Phase = RunPhase.FloorProgress;
            selectedDomino = null;
            selectedForDiscard.Clear();
            selectedDomiNexIndex = -1;
            selectedConsumableIndex = -1;
            OpenBoosterPack = null;
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
                Quota = GetAdjustedQuota(levelIndex, floorIndex, levelInFloor),
                Boss = bossDefinition == null ? null : new BossLevelState(bossDefinition)
            };
            Run.CurrentShop = null;
            Run.CurrentReward = null;
            Run.Phase = RunPhase.PlayingLevel;
            selectedDomino = null;
            selectedForDiscard.Clear();
            selectedDomiNexIndex = -1;
            selectedConsumableIndex = -1;
            OpenBoosterPack = null;
            CurrentOrientation = DominoOrientation.HorizontalRight;
            BossIntroActive = bossDefinition != null;

            dominexEffectEngine.ApplyLevelStart(Run.DomiNexInventory, Run.CurrentLevel, null);
            ApplyBossLevelStart(Run.CurrentLevel);
            Run.Bag.Initialize(CreateRunDominoSet());

            DrawIntoHand(Run.CurrentLevel, GameConstants.StartingHandSize);

            ApplyBossAfterDraw(Run.CurrentLevel);
        }

        private void PrepareNextHand(LevelState level)
        {
            foreach (var domino in level.Hand.Dominoes.ToList())
            {
                level.DiscardedThisLevel.Add(domino);
            }

            level.Hand.Dominoes.Clear();
            level.Grid.Clear();
            selectedDomino = null;
            selectedForDiscard.Clear();
            DrawIntoHand(level, GameConstants.StartingHandSize);
        }

        private int ApplyDominoModifierPostScoringEffects(ScoreResult score, bool levelWon)
        {
            var jackpotGain = 0;
            foreach (var effect in score.DominoModifierEffects)
            {
                if (effect.JackpotMeterGain > 0)
                {
                    jackpotGain += JackpotMeterService.AddJackpotMeter(Run, effect.JackpotMeterGain, JackpotGainSource.DominoModifier);
                }

                if (effect.SpinTicketGain > 0)
                {
                    Run.Jackpot.SpinTickets += effect.SpinTicketGain;
                }

                if (effect.GlassBroken)
                {
                    Run.DestroyedDominoInstanceIds.Add(effect.DominoInstanceId);
                }
            }

            return jackpotGain;
        }

        private int AwardGoldDominoCredits(LevelState level)
        {
            var goldCount = level.PlayedThisLevel.Count(domino => domino.ModifierId == DominoModifierRegistry.GoldId);
            if (goldCount <= 0)
            {
                return 0;
            }

            return RunEconomyService.AddCredits(Run, goldCount * 2, CreditSource.LevelReward);
        }

        private void DrawIntoHand(LevelState level, int targetCount)
        {
            var missing = System.Math.Max(0, targetCount - level.Hand.Dominoes.Count);
            foreach (var domino in Run.Bag.Draw(missing))
            {
                level.Hand.AddDomino(domino);
            }
        }

        public int GetQuotaForLevel(int levelIndex)
        {
            return LevelQuotaService.GetQuotaForGlobalLevel(levelIndex);
        }

        private List<DominoInstance> CreateRunDominoSet()
        {
            var dominoes = DominoFactory.CreateDoubleSixSet();
            for (var i = 0; i < Run.PurchasedDominoes.Count; i++)
            {
                var purchased = Run.PurchasedDominoes[i];
                if (!Run.DestroyedDominoInstanceIds.Contains(purchased.InstanceId))
                {
                    dominoes.Add(purchased);
                }
            }

            return dominoes.Where(domino => !Run.DestroyedDominoInstanceIds.Contains(domino.InstanceId)).ToList();
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
            if (Run.NextLevelExtraDiscards > 0)
            {
                level.DiscardsRemaining += Run.NextLevelExtraDiscards;
                level.MaxDiscards = level.DiscardsRemaining;
                Run.NextLevelExtraDiscards = 0;
            }

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
                level.MaxDiscards = level.DiscardsRemaining;
            }

        }

        private void AdvanceJackpotLevelEffects()
        {
            if (Run.Jackpot.JackpotMeterMultiplierLevelsRemaining <= 0)
            {
                return;
            }

            Run.Jackpot.JackpotMeterMultiplierLevelsRemaining--;
            if (Run.Jackpot.JackpotMeterMultiplierLevelsRemaining == 0)
            {
                Run.Jackpot.JackpotMeterMultiplier = 1f;
            }
        }

        private int GetAdjustedQuota(int levelIndex, int floorIndex, int levelInFloor)
        {
            var quota = GetQuotaForLevel(levelIndex);
            if (levelInFloor == GameConstants.LevelsPerFloor && Run.NextBossQuotaMultiplierOverride > 0f && Run.NextBossQuotaMultiplierOverride < 1f)
            {
                quota = System.Math.Max(1, (int)System.Math.Ceiling(quota * Run.NextBossQuotaMultiplierOverride));
                Run.NextBossQuotaMultiplierOverride = 1f;
            }
            else if (Run.NextLevelQuotaMultiplier > 1f)
            {
                quota = System.Math.Max(1, (int)System.Math.Ceiling(quota * Run.NextLevelQuotaMultiplier));
                Run.NextLevelQuotaMultiplier = 1f;
            }

            return quota;
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
                .Concat(level.PlayedThisLevel)
                .Concat(level.DiscardedThisLevel)
                .GroupBy(domino => domino.InstanceId)
                .Select(group => group.First())
                .Count(domino => domino.Definition.IsDouble);
        }

        public static int CountPlacedAgainstLimit(LevelState level)
        {
            return level?.Grid.GetPlacedDominoes().Count(placed => !DominoModifierRegistry.IsLight(placed.Domino)) ?? 0;
        }

        private void Notify(string message)
        {
            var score = Run.Phase == RunPhase.PlayingLevel ? CalculateCurrentScorePreview() : lastScoreResult;
            StateChanged?.Invoke(Run, score, message);
        }
    }
}
