using System.Collections;
using DomiNox.Jackpot;
using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class JackpotMeterView : MonoBehaviour
    {
        private GameFlowController controller;
        private Text[] reels;
        private Image meterFill;
        private Text meterText;
        private Text ticketsText;
        private Text heatText;
        private Text resultText;
        private Button spinButton;
        private Transform majorOverlay;
        private Transform majorOptionsRoot;
        private Button majorConfirm;
        private Transform rewardPopup;
        private Text rewardTitle;
        private Text rewardSymbols;
        private Text rewardDescription;
        private Button rewardCollect;
        private bool spinning;
        private bool rewardPopupVisible;
        private readonly JackpotSymbol[] spinningSymbols = (JackpotSymbol[])System.Enum.GetValues(typeof(JackpotSymbol));

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            gameObject.AddComponent<Image>().color = new Color(0.08f, 0.04f, 0.09f, 0.96f);
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.68f, 0.16f);
            outline.effectDistance = new Vector2(2f, -2f);
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 10);
            layout.spacing = 7f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var title = UiFactory.CreateText(transform, "Title", "JACKPOT", 18, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.78f, 0.18f);

            var reelRow = new GameObject("Reels", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            reelRow.transform.SetParent(transform, false);
            reelRow.GetComponent<LayoutElement>().preferredHeight = 42f;
            var reelLayout = reelRow.GetComponent<HorizontalLayoutGroup>();
            reelLayout.spacing = 6f;
            reelLayout.childAlignment = TextAnchor.MiddleCenter;
            reels = new Text[3];
            for (var i = 0; i < reels.Length; i++)
            {
                reels[i] = CreateReel(reelRow.transform);
            }

            var meter = new GameObject("Meter", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            meter.transform.SetParent(transform, false);
            meter.GetComponent<LayoutElement>().preferredHeight = 18f;
            meter.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f);
            meterFill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            meterFill.transform.SetParent(meter.transform, false);
            meterFill.color = new Color(0.05f, 0.8f, 1f);
            var fillRect = meterFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            meterText = UiFactory.CreateText(transform, "MeterText", "Meter 0%", 12, TextAnchor.MiddleCenter);
            meterText.color = new Color(0.74f, 0.92f, 1f);
            ticketsText = UiFactory.CreateText(transform, "Tickets", "Tickets: 0", 13, TextAnchor.MiddleCenter);
            ticketsText.color = new Color(1f, 0.84f, 0.25f);
            heatText = UiFactory.CreateText(transform, "Heat", "Heat: 0", 12, TextAnchor.MiddleCenter);
            heatText.color = new Color(1f, 0.36f, 0.18f);
            resultText = UiFactory.CreateText(transform, "Result", "", 11, TextAnchor.MiddleCenter);
            resultText.color = new Color(0.82f, 0.86f, 0.92f);
            resultText.GetComponent<LayoutElement>().preferredHeight = 34f;

            spinButton = UiFactory.CreateButton(transform, "Spin", "SPIN");
            spinButton.GetComponent<Image>().color = new Color(0.72f, 0.08f, 0.14f);
            spinButton.GetComponent<LayoutElement>().preferredHeight = 34f;
            spinButton.onClick.AddListener(() => StartCoroutine(SpinRoutine()));

            BuildMajorOverlay();
            BuildRewardPopup();
        }

        public void Render(RunState run)
        {
            var jackpot = run.Jackpot;
            if (!spinning)
            {
                var result = controller.LastJackpotSpinResult;
                reels[0].text = result == null ? "?" : JackpotSymbolViewDataCatalog.Get(result.Symbol1).Label;
                reels[1].text = result == null ? "?" : JackpotSymbolViewDataCatalog.Get(result.Symbol2).Label;
                reels[2].text = result == null ? "?" : JackpotSymbolViewDataCatalog.Get(result.Symbol3).Label;
                for (var i = 0; i < reels.Length; i++)
                {
                    reels[i].color = result == null ? Color.white : JackpotSymbolViewDataCatalog.Get(i == 0 ? result.Symbol1 : i == 1 ? result.Symbol2 : result.Symbol3).Color;
                }
            }

            var ratio = jackpot.MaxMeter <= 0 ? 0f : Mathf.Clamp01(jackpot.Meter / (float)jackpot.MaxMeter);
            meterFill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            meterText.text = $"Jackpot Meter {Mathf.RoundToInt(ratio * 100f)}%";
            ticketsText.text = $"Tickets: {jackpot.SpinTickets}";
            heatText.text = jackpot.MachineHeat >= 5 ? "PAIR GUARANTEED" : jackpot.MachineHeat >= 3 ? $"Heat: {jackpot.MachineHeat} HOT" : $"Heat: {jackpot.MachineHeat}";
            spinButton.interactable = !spinning && !rewardPopupVisible && !controller.HasPendingJackpotReward && jackpot.SpinTickets > 0 && controller.OpenBoosterPack == null && controller.OpenMajorJackpot == null;
            var last = controller.LastJackpotSpinResult;
            resultText.text = spinning || controller.HasPendingJackpotReward
                ? "Jackpot spinning..."
                : last == null ? "Luck builds when spins miss triples." : $"{last.Tier}: {last.Reward.Name}";
            RenderMajor(run);
        }

        private IEnumerator SpinRoutine()
        {
            if (spinning)
            {
                yield break;
            }

            spinning = true;
            rewardPopupVisible = false;
            SetAllReelsRandom();
            var result = controller.BeginJackpotSpin();
            if (result == null)
            {
                spinning = false;
                yield break;
            }

            var targets = new[] { result.Symbol1, result.Symbol2, result.Symbol3 };
            var suspense = result.Symbol1 == result.Symbol2;
            var sevenSuspense = suspense && result.Symbol1 == JackpotSymbol.Seven;
            var reelStopped = new[] { false, false, false };
            StartCoroutine(SpinReelVisuals(0, reelStopped));
            StartCoroutine(SpinReelVisuals(1, reelStopped));
            StartCoroutine(SpinReelVisuals(2, reelStopped));

            yield return new WaitForSeconds(0.8f);
            StopReel(0, targets[0], reelStopped, suspense ? 1.18f : 1.12f);

            yield return new WaitForSeconds(0.45f);
            StopReel(1, targets[1], reelStopped, suspense ? 1.18f : 1.12f);

            if (suspense)
            {
                StartCoroutine(PulseReel(0, sevenSuspense ? 1.28f : 1.18f));
                StartCoroutine(PulseReel(1, sevenSuspense ? 1.28f : 1.18f));
            }

            yield return new WaitForSeconds(suspense ? sevenSuspense ? 1.05f : 0.7f : 0.45f);
            StopReel(2, targets[2], reelStopped, sevenSuspense ? 1.3f : suspense ? 1.2f : 1.12f);

            spinning = false;
            controller.RevealPendingJackpotSpinResult();
            yield return new WaitForSeconds(1f);
            ShowRewardPopup(result);
        }

        private IEnumerator SpinReelVisuals(int reel, bool[] reelStopped)
        {
            while (!reelStopped[reel])
            {
                SetReel(reel, RandomSymbol());
                yield return new WaitForSeconds(0.045f + (reel * 0.006f));
            }
        }

        private void StopReel(int reel, JackpotSymbol symbol, bool[] reelStopped, float pulseScale)
        {
            reelStopped[reel] = true;
            SetReel(reel, symbol);
            StartCoroutine(PulseReel(reel, pulseScale));
        }

        private void SetAllReelsRandom()
        {
            for (var i = 0; i < reels.Length; i++)
            {
                SetReel(i, RandomSymbol());
            }
        }

        private JackpotSymbol RandomSymbol()
        {
            return spinningSymbols[Random.Range(0, spinningSymbols.Length)];
        }

        private void SetReel(int index, JackpotSymbol symbol)
        {
            var data = JackpotSymbolViewDataCatalog.Get(symbol);
            reels[index].text = data.Label;
            reels[index].color = data.Color;
        }

        private Text CreateReel(Transform parent)
        {
            var root = new GameObject("Reel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f);
            root.GetComponent<Outline>().effectColor = new Color(0.95f, 0.72f, 0.26f);
            root.GetComponent<LayoutElement>().preferredWidth = 48f;
            root.GetComponent<LayoutElement>().preferredHeight = 38f;
            var text = UiFactory.CreateText(root.transform, "Text", "?", 22, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            Stretch(text.rectTransform);
            return text;
        }

        private void BuildMajorOverlay()
        {
            var canvas = GetComponentInParent<Canvas>();
            majorOverlay = new GameObject("MajorJackpotOverlay", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup)).transform;
            majorOverlay.SetParent(canvas.transform, false);
            var rect = (RectTransform)majorOverlay;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 500f);
            majorOverlay.GetComponent<Image>().color = new Color(0.08f, 0.02f, 0.06f, 0.99f);
            var layout = majorOverlay.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 16, 20);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperCenter;
            var title = UiFactory.CreateText(majorOverlay, "Title", "MAJOR JACKPOT", 30, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.22f, 0.1f);
            majorOptionsRoot = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).transform;
            majorOptionsRoot.SetParent(majorOverlay, false);
            majorOptionsRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            majorOptionsRoot.GetComponent<VerticalLayoutGroup>().spacing = 7f;
            majorConfirm = UiFactory.CreateButton(majorOverlay, "Confirm", "Confirm 2 Rewards");
            majorConfirm.GetComponent<LayoutElement>().preferredHeight = 44f;
            majorConfirm.onClick.AddListener(controller.ConfirmMajorJackpotRewards);
            majorOverlay.gameObject.SetActive(false);
        }

        private void BuildRewardPopup()
        {
            var canvas = GetComponentInParent<Canvas>();
            rewardPopup = new GameObject("JackpotRewardPopup", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup)).transform;
            rewardPopup.SetParent(canvas.transform, false);
            var rect = (RectTransform)rewardPopup;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 330f);
            rewardPopup.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.99f);
            rewardPopup.GetComponent<Outline>().effectColor = new Color(1f, 0.68f, 0.12f);
            rewardPopup.GetComponent<Outline>().effectDistance = new Vector2(4f, -4f);
            var layout = rewardPopup.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 16, 18);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperCenter;
            rewardTitle = UiFactory.CreateText(rewardPopup, "Title", "Reward", 24, TextAnchor.MiddleCenter);
            rewardTitle.color = new Color(1f, 0.84f, 0.25f);
            rewardSymbols = UiFactory.CreateText(rewardPopup, "Symbols", "", 28, TextAnchor.MiddleCenter);
            rewardDescription = UiFactory.CreateText(rewardPopup, "Description", "", 15, TextAnchor.MiddleCenter);
            rewardDescription.GetComponent<LayoutElement>().flexibleHeight = 1f;
            rewardCollect = UiFactory.CreateButton(rewardPopup, "Collect", "Collect");
            rewardCollect.GetComponent<LayoutElement>().preferredHeight = 44f;
            rewardCollect.onClick.AddListener(CollectRewardPopup);
            rewardPopup.gameObject.SetActive(false);
        }

        private void ShowRewardPopup(JackpotSpinResult result)
        {
            rewardPopupVisible = true;
            rewardTitle.text = GetTierTitle(result.Tier);
            rewardTitle.color = result.Tier == JackpotRewardTier.MajorJackpot ? new Color(1f, 0.16f, 0.08f) : result.Tier == JackpotRewardTier.Triple ? new Color(1f, 0.68f, 0.12f) : result.Tier == JackpotRewardTier.Pair ? new Color(0.1f, 0.88f, 1f) : new Color(0.72f, 0.78f, 0.86f);
            rewardSymbols.text = $"[{JackpotSymbolViewDataCatalog.Get(result.Symbol1).Label}] [{JackpotSymbolViewDataCatalog.Get(result.Symbol2).Label}] [{JackpotSymbolViewDataCatalog.Get(result.Symbol3).Label}]";
            rewardSymbols.color = result.Tier == JackpotRewardTier.MajorJackpot ? new Color(1f, 0.16f, 0.08f) : Color.white;
            rewardDescription.text = result.Tier == JackpotRewardTier.MajorJackpot
                ? "Choose 2 major rewards."
                : $"{result.Reward.Name}\n{result.Reward.Description}";
            rewardCollect.GetComponentInChildren<Text>().text = result.Tier == JackpotRewardTier.MajorJackpot ? "Continue" : "Collect";
            rewardPopup.gameObject.SetActive(true);
            rewardPopup.SetAsLastSibling();
        }

        private void CollectRewardPopup()
        {
            rewardPopupVisible = false;
            rewardPopup.gameObject.SetActive(false);
            controller.CollectPendingJackpotReward();
        }

        private void RenderMajor(RunState run)
        {
            var state = controller.OpenMajorJackpot;
            majorOverlay.gameObject.SetActive(state != null);
            if (state == null)
            {
                return;
            }

            Clear(majorOptionsRoot);
            for (var i = 0; i < state.Options.Count; i++)
            {
                var option = state.Options[i];
                var selected = state.SelectedIndices.Contains(i);
                var button = UiFactory.CreateButton(majorOptionsRoot, option.Id, selected ? $"SELECTED - {option.Name}" : option.Name);
                button.GetComponent<Image>().color = selected ? new Color(0.9f, 0.65f, 0.12f) : new Color(0.18f, 0.1f, 0.22f);
                button.GetComponent<LayoutElement>().preferredHeight = 48f;
                button.GetComponentInChildren<Text>().text = $"{(selected ? "SELECTED - " : "")}{option.Name}\n{option.Description}";
                var captured = i;
                button.onClick.AddListener(() => controller.ToggleMajorJackpotReward(captured));
            }

            majorConfirm.interactable = state.SelectedIndices.Count == 2;
            majorOverlay.SetAsLastSibling();
        }

        private static string GetTierTitle(JackpotRewardTier tier)
        {
            return tier switch
            {
                JackpotRewardTier.MajorJackpot => "MAJOR JACKPOT",
                JackpotRewardTier.Triple => "Triple Reward",
                JackpotRewardTier.Pair => "Pair Reward",
                _ => "Consolation Prize"
            };
        }

        private IEnumerator PulseReel(int index, float scale)
        {
            var rect = (RectTransform)reels[index].transform.parent;
            var start = rect.localScale;
            const float duration = 0.22f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                rect.localScale = start * Mathf.Lerp(1f, scale, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            rect.localScale = start;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Clear(Transform root)
        {
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
