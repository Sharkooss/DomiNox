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
        private Transform infoOverlay;
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
            var bg = gameObject.AddComponent<Image>();
            bg.color = DomiNoxTheme.JackpotBg;
            DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.JackpotAmber, 2.5f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 10);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            headerRow.transform.SetParent(transform, false);
            headerRow.GetComponent<LayoutElement>().preferredHeight = 26f;
            var headerLayout = headerRow.GetComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            headerLayout.spacing = 4f;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var title = UiFactory.CreateText(headerRow.transform, "Title", "JACKPOT", DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.JackpotAmber;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(1f, 0.5f, 0f, 0.5f), new Vector2(1f, -1f));
            title.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var infoButton = UiFactory.CreateButton(headerRow.transform, "Info", "i");
            infoButton.GetComponent<LayoutElement>().preferredWidth = 26f;
            infoButton.GetComponent<LayoutElement>().preferredHeight = 24f;
            UiFactory.StyleButton(infoButton, DomiNoxTheme.BgCard, DomiNoxTheme.JackpotAmber, 24f);
            var infoLabel = infoButton.GetComponentInChildren<Text>();
            if (infoLabel != null) { infoLabel.fontStyle = FontStyle.Bold; infoLabel.fontSize = DomiNoxTheme.FontSM; }
            infoButton.onClick.AddListener(ToggleInfoOverlay);

            var reelRow = new GameObject("Reels", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            reelRow.transform.SetParent(transform, false);
            reelRow.GetComponent<LayoutElement>().preferredHeight = 46f;
            var reelLayout = reelRow.GetComponent<HorizontalLayoutGroup>();
            reelLayout.spacing = 5f;
            reelLayout.childAlignment = TextAnchor.MiddleCenter;
            reelLayout.childForceExpandWidth = false;
            reels = new Text[3];
            for (var i = 0; i < reels.Length; i++)
                reels[i] = CreateReel(reelRow.transform);

            var meterBg = new GameObject("Meter", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            meterBg.transform.SetParent(transform, false);
            meterBg.GetComponent<LayoutElement>().preferredHeight = 12f;
            meterBg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f);
            meterFill = new GameObject("Fill", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            meterFill.transform.SetParent(meterBg.transform, false);
            meterFill.color = DomiNoxTheme.JackpotCyan;
            var fillRect = meterFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            meterText = UiFactory.CreateText(transform, "MeterText", "Meter 0%", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            meterText.color = DomiNoxTheme.JackpotCyan;
            meterText.GetComponent<LayoutElement>().preferredHeight = 16f;

            ticketsText = UiFactory.CreateText(transform, "Tickets", "Tickets: 0", DomiNoxTheme.FontSM, TextAnchor.MiddleCenter);
            ticketsText.color = DomiNoxTheme.JackpotAmber;
            ticketsText.fontStyle = FontStyle.Bold;
            ticketsText.GetComponent<LayoutElement>().preferredHeight = 18f;

            heatText = UiFactory.CreateText(transform, "Heat", "Heat: 0", DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            heatText.color = DomiNoxTheme.Danger;
            heatText.GetComponent<LayoutElement>().preferredHeight = 16f;

            resultText = UiFactory.CreateText(transform, "Result", string.Empty, DomiNoxTheme.FontXS, TextAnchor.MiddleCenter);
            resultText.color = DomiNoxTheme.TextSecondary;
            resultText.GetComponent<LayoutElement>().preferredHeight = 32f;

            spinButton = UiFactory.CreateButton(transform, "Spin", "SPIN");
            spinButton.GetComponent<LayoutElement>().preferredHeight = 40f;
            UiFactory.StyleButton(spinButton, new Color(0.55f, 0.06f, 0.12f), DomiNoxTheme.JackpotAmber, 40f);
            var spinLabel = spinButton.GetComponentInChildren<Text>();
            if (spinLabel != null) { spinLabel.fontSize = DomiNoxTheme.FontMD; spinLabel.fontStyle = FontStyle.Bold; }
            spinButton.onClick.AddListener(() => StartCoroutine(SpinRoutine()));

            BuildMajorOverlay();
            BuildRewardPopup();
            BuildInfoOverlay();
        }

        private void ToggleInfoOverlay()
        {
            infoOverlay.gameObject.SetActive(!infoOverlay.gameObject.activeSelf);
            if (infoOverlay.gameObject.activeSelf)
            {
                infoOverlay.SetAsLastSibling();
            }
        }

        private void BuildInfoOverlay()
        {
            var canvas = GetComponentInParent<Canvas>();
            infoOverlay = new GameObject("JackpotInfoOverlay", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup)).transform;
            infoOverlay.SetParent(canvas.transform, false);
            var rect = (RectTransform)infoOverlay;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 660f);
            infoOverlay.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.08f, 0.99f);
            DomiNoxTheme.AddOutline(infoOverlay.gameObject, DomiNoxTheme.JackpotAmber, 3f);
            DomiNoxTheme.AddShadow(infoOverlay.gameObject, new Color(1f, 0.5f, 0f, 0.3f), new Vector2(4f, -4f));
            var layout = infoOverlay.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 16, 18);
            layout.spacing = 7f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var header = new GameObject("InfoHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            header.transform.SetParent(infoOverlay, false);
            header.GetComponent<LayoutElement>().preferredHeight = 38f;
            var hl = header.GetComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = true;

            var titleText = UiFactory.CreateText(header.transform, "Title", "MACHINE A SOUS", DomiNoxTheme.FontXL, TextAnchor.MiddleLeft);
            titleText.color = DomiNoxTheme.JackpotAmber;
            titleText.fontStyle = FontStyle.Bold;
            titleText.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var close = UiFactory.CreateButton(header.transform, "Close", "Fermer");
            close.GetComponent<LayoutElement>().preferredWidth = 110f;
            UiFactory.StyleButton(close, DomiNoxTheme.BgCard, DomiNoxTheme.JackpotAmber);
            close.onClick.AddListener(() => infoOverlay.gameObject.SetActive(false));

            var intro = UiFactory.CreateText(infoOverlay, "Intro", "Depense 1 Spin Ticket pour tourner. Aligne 2 ou 3 symboles identiques pour gagner. Trois 7 = MAJOR JACKPOT (choisis 2 recompenses majeures).", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            intro.color = DomiNoxTheme.TextSecondary;
            intro.GetComponent<LayoutElement>().preferredHeight = 32f;
            UiFactory.CreateSeparator(infoOverlay, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.JackpotAmber, 0.4f));

            var weights = JackpotSpinService.GetWeights(null);
            var total = 0;
            foreach (var weight in weights.Values) total += weight;

            var order = new[]
            {
                JackpotSymbol.Seven, JackpotSymbol.Crown, JackpotSymbol.DomiNex, JackpotSymbol.Gem,
                JackpotSymbol.Domino, JackpotSymbol.Coin, JackpotSymbol.Skull, JackpotSymbol.Blank
            };
            foreach (var symbol in order)
            {
                if (!weights.TryGetValue(symbol, out var weight)) continue;
                CreateInfoRow(symbol, weight, total);
            }

            UiFactory.CreateSeparator(infoOverlay, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.JackpotAmber, 0.4f));
            var mechanics = UiFactory.CreateText(infoOverlay, "Mechanics", "Heat : a 5, la prochaine paire est garantie (se vide a chaque triple/paire). Jackpot Luck : augmente les chances de Crown et Seven et reduit les Blank. Remplis le Meter pour gagner des Spin Tickets.", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            mechanics.color = DomiNoxTheme.TextSecondary;
            mechanics.GetComponent<LayoutElement>().flexibleHeight = 1f;

            infoOverlay.gameObject.SetActive(false);
        }

        private void CreateInfoRow(JackpotSymbol symbol, int weight, int total)
        {
            var data = JackpotSymbolViewDataCatalog.Get(symbol);
            var row = new GameObject($"Info_{symbol}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(infoOverlay, false);
            row.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.BgCard, 0.55f);
            DomiNoxTheme.AddOutline(row, DomiNoxTheme.WithAlpha(data.Color, 0.5f), 1f);
            row.GetComponent<LayoutElement>().preferredHeight = 48f;
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 4, 4);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            var glyph = UiFactory.CreateText(row.transform, "Glyph", data.Label, DomiNoxTheme.FontLG, TextAnchor.MiddleCenter);
            glyph.color = data.Color;
            glyph.fontStyle = FontStyle.Bold;
            glyph.GetComponent<LayoutElement>().preferredWidth = 58f;

            var percent = total <= 0 ? 0 : Mathf.RoundToInt(weight / (float)total * 100f);
            var pair = symbol == JackpotSymbol.Blank ? "-" : JackpotRewardCatalog.GetPairReward(symbol).Name;
            var triple = symbol == JackpotSymbol.Blank ? "-" : JackpotRewardCatalog.GetTripleReward(symbol).Name;
            var info = UiFactory.CreateText(row.transform, "Info", $"{symbol}  -  {percent}% par rouleau\nPaire : {pair}    Triple : {triple}", DomiNoxTheme.FontXS, TextAnchor.MiddleLeft);
            info.color = DomiNoxTheme.TextSecondary;
            info.GetComponent<LayoutElement>().flexibleWidth = 1f;
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
                    reels[i].color = result == null ? DomiNoxTheme.TextSecondary : JackpotSymbolViewDataCatalog.Get(i == 0 ? result.Symbol1 : i == 1 ? result.Symbol2 : result.Symbol3).Color;
            }

            var ratio = jackpot.MaxMeter <= 0 ? 0f : Mathf.Clamp01(jackpot.Meter / (float)jackpot.MaxMeter);
            meterFill.rectTransform.anchorMax = new Vector2(ratio, 1f);
            meterFill.color = Color.Lerp(DomiNoxTheme.JackpotCyan, DomiNoxTheme.JackpotAmber, ratio);
            meterText.text = $"Meter {Mathf.RoundToInt(ratio * 100f)}%";
            ticketsText.text = jackpot.SpinTickets > 0 ? $"Tickets: {jackpot.SpinTickets}" : "No Tickets";
            ticketsText.color = jackpot.SpinTickets > 0 ? DomiNoxTheme.JackpotAmber : DomiNoxTheme.TextMuted;
            heatText.text = jackpot.MachineHeat >= 5
                ? "PAIR GUARANTEED"
                : jackpot.MachineHeat >= 3 ? $"Heat: {jackpot.MachineHeat} HOT" : $"Heat: {jackpot.MachineHeat}";
            heatText.color = jackpot.MachineHeat >= 5 ? DomiNoxTheme.Warning : jackpot.MachineHeat >= 3 ? DomiNoxTheme.Danger : DomiNoxTheme.TextMuted;
            spinButton.interactable = !spinning && !rewardPopupVisible && !controller.HasPendingJackpotReward && jackpot.SpinTickets > 0 && controller.OpenBoosterPack == null && controller.OpenMajorJackpot == null;
            var last = controller.LastJackpotSpinResult;
            resultText.text = spinning || controller.HasPendingJackpotReward
                ? "Spinning..."
                : last == null ? "Fill meter to earn tickets." : $"{last.Tier}: {last.Reward.Name}";
            RenderMajor(run);
        }

        private IEnumerator SpinRoutine()
        {
            if (spinning) yield break;
            spinning = true;
            rewardPopupVisible = false;
            SetAllReelsRandom();
            var result = controller.BeginJackpotSpin();
            if (result == null) { spinning = false; yield break; }

            var targets = new[] { result.Symbol1, result.Symbol2, result.Symbol3 };
            var suspense = result.Symbol1 == result.Symbol2;
            var sevenSuspense = suspense && result.Symbol1 == JackpotSymbol.Seven;
            var reelStopped = new[] { false, false, false };
            StartCoroutine(SpinReelVisuals(0, reelStopped));
            StartCoroutine(SpinReelVisuals(1, reelStopped));
            StartCoroutine(SpinReelVisuals(2, reelStopped));

            yield return new WaitForSeconds(0.7f);
            StopReel(0, targets[0], reelStopped, suspense ? 1.22f : 1.14f);
            yield return new WaitForSeconds(0.38f);
            StopReel(1, targets[1], reelStopped, suspense ? 1.22f : 1.14f);

            if (suspense)
            {
                StartCoroutine(PulseReel(0, sevenSuspense ? 1.32f : 1.20f));
                StartCoroutine(PulseReel(1, sevenSuspense ? 1.32f : 1.20f));
            }

            yield return new WaitForSeconds(suspense ? sevenSuspense ? 1.1f : 0.72f : 0.38f);
            StopReel(2, targets[2], reelStopped, sevenSuspense ? 1.38f : suspense ? 1.26f : 1.14f);
            spinning = false;
            controller.RevealPendingJackpotSpinResult();
            yield return new WaitForSeconds(0.9f);
            ShowRewardPopup(result);
        }

        private IEnumerator SpinReelVisuals(int reel, bool[] reelStopped)
        {
            while (!reelStopped[reel])
            {
                SetReel(reel, RandomSymbol());
                yield return new WaitForSeconds(0.038f + (reel * 0.005f));
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
            for (var i = 0; i < reels.Length; i++) SetReel(i, RandomSymbol());
        }

        private JackpotSymbol RandomSymbol() => spinningSymbols[Random.Range(0, spinningSymbols.Length)];

        private void SetReel(int index, JackpotSymbol symbol)
        {
            var data = JackpotSymbolViewDataCatalog.Get(symbol);
            reels[index].text = data.Label;
            reels[index].color = data.Color;
        }

        private Text CreateReel(Transform parent)
        {
            var root = new GameObject("Reel", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.02f, 0.02f, 0.04f);
            DomiNoxTheme.AddOutline(root, DomiNoxTheme.JackpotAmber, 2f);
            DomiNoxTheme.AddShadow(root, new Color(1f, 0.5f, 0f, 0.25f), new Vector2(1f, -1f));
            root.GetComponent<LayoutElement>().preferredWidth = 52f;
            root.GetComponent<LayoutElement>().preferredHeight = 42f;
            var text = UiFactory.CreateText(root.transform, "Text", "?", 26, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.color = DomiNoxTheme.TextPrimary;
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
            rect.sizeDelta = new Vector2(740f, 520f);
            majorOverlay.GetComponent<Image>().color = new Color(0.06f, 0.02f, 0.08f, 0.99f);
            DomiNoxTheme.AddOutline(majorOverlay.gameObject, DomiNoxTheme.JackpotAmber, 4f);
            var layout = majorOverlay.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 18, 22);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            var titleText = UiFactory.CreateText(majorOverlay, "Title", "MAJOR JACKPOT!", 34, TextAnchor.MiddleCenter);
            titleText.color = DomiNoxTheme.JackpotAmber;
            titleText.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(titleText.gameObject, new Color(1f, 0.4f, 0f, 0.5f), new Vector2(2f, -2f));
            titleText.GetComponent<LayoutElement>().preferredHeight = 52f;
            UiFactory.CreateSeparator(majorOverlay, 2f, DomiNoxTheme.JackpotAmber);
            majorOptionsRoot = new GameObject("Options", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement)).transform;
            majorOptionsRoot.SetParent(majorOverlay, false);
            majorOptionsRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            majorOptionsRoot.GetComponent<VerticalLayoutGroup>().spacing = 8f;
            majorConfirm = UiFactory.CreateButton(majorOverlay, "Confirm", "Confirm 2 Rewards");
            majorConfirm.GetComponent<LayoutElement>().preferredHeight = 48f;
            UiFactory.StyleButton(majorConfirm, new Color(0.10f, 0.28f, 0.10f), DomiNoxTheme.Success, 48f);
            majorConfirm.onClick.AddListener(controller.ConfirmMajorJackpotRewards);
            majorOverlay.gameObject.SetActive(false);
        }

        private void BuildRewardPopup()
        {
            var canvas = GetComponentInParent<Canvas>();
            rewardPopup = new GameObject("JackpotRewardPopup", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup)).transform;
            rewardPopup.SetParent(canvas.transform, false);
            var rect = (RectTransform)rewardPopup;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(540f, 340f);
            rewardPopup.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.09f, 0.99f);
            DomiNoxTheme.AddOutline(rewardPopup.gameObject, DomiNoxTheme.JackpotAmber, 4f);
            DomiNoxTheme.AddShadow(rewardPopup.gameObject, new Color(1f, 0.5f, 0f, 0.35f), new Vector2(4f, -4f));
            var layout = rewardPopup.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 18, 20);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            rewardTitle = UiFactory.CreateText(rewardPopup, "Title", "Reward", 28, TextAnchor.MiddleCenter);
            rewardTitle.color = DomiNoxTheme.Gold;
            rewardTitle.fontStyle = FontStyle.Bold;
            rewardTitle.GetComponent<LayoutElement>().preferredHeight = 44f;
            rewardSymbols = UiFactory.CreateText(rewardPopup, "Symbols", string.Empty, 32, TextAnchor.MiddleCenter);
            rewardSymbols.fontStyle = FontStyle.Bold;
            rewardSymbols.GetComponent<LayoutElement>().preferredHeight = 48f;
            rewardDescription = UiFactory.CreateText(rewardPopup, "Description", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            rewardDescription.color = DomiNoxTheme.TextSecondary;
            rewardDescription.GetComponent<LayoutElement>().flexibleHeight = 1f;
            rewardCollect = UiFactory.CreateButton(rewardPopup, "Collect", "Collect");
            rewardCollect.GetComponent<LayoutElement>().preferredHeight = 48f;
            UiFactory.StyleButton(rewardCollect, new Color(0.08f, 0.24f, 0.10f), DomiNoxTheme.Success, 48f);
            rewardCollect.onClick.AddListener(CollectRewardPopup);
            rewardPopup.gameObject.SetActive(false);
        }

        private void ShowRewardPopup(JackpotSpinResult result)
        {
            rewardPopupVisible = true;
            rewardTitle.text = GetTierTitle(result.Tier);
            rewardTitle.color = result.Tier == JackpotRewardTier.MajorJackpot ? DomiNoxTheme.Danger
                              : result.Tier == JackpotRewardTier.Triple        ? DomiNoxTheme.JackpotAmber
                              : result.Tier == JackpotRewardTier.Pair          ? DomiNoxTheme.JackpotCyan
                              : DomiNoxTheme.TextSecondary;
            rewardSymbols.text = $"[{JackpotSymbolViewDataCatalog.Get(result.Symbol1).Label}] [{JackpotSymbolViewDataCatalog.Get(result.Symbol2).Label}] [{JackpotSymbolViewDataCatalog.Get(result.Symbol3).Label}]";
            rewardSymbols.color = result.Tier == JackpotRewardTier.MajorJackpot ? DomiNoxTheme.Danger : DomiNoxTheme.TextPrimary;
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
            if (state == null) return;
            Clear(majorOptionsRoot);
            for (var i = 0; i < state.Options.Count; i++)
            {
                var option = state.Options[i];
                var selected = state.SelectedIndices.Contains(i);
                var button = UiFactory.CreateButton(majorOptionsRoot, option.Id, option.Name);
                button.GetComponent<LayoutElement>().preferredHeight = 52f;
                UiFactory.StyleButton(button, selected ? new Color(0.55f, 0.38f, 0.06f) : DomiNoxTheme.BgCard, selected ? DomiNoxTheme.Gold : DomiNoxTheme.TextSecondary, 52f);
                button.GetComponentInChildren<Text>().text = $"{(selected ? "✓ " : "")}{option.Name}\n{option.Description}";
                var captured = i;
                button.onClick.AddListener(() => controller.ToggleMajorJackpotReward(captured));
            }
            majorConfirm.interactable = state.SelectedIndices.Count == 2;
            majorOverlay.SetAsLastSibling();
        }

        private static string GetTierTitle(JackpotRewardTier tier) => tier switch
        {
            JackpotRewardTier.MajorJackpot => "MAJOR JACKPOT!",
            JackpotRewardTier.Triple       => "Triple — Lucky!",
            JackpotRewardTier.Pair         => "Pair Reward",
            _                              => "Consolation"
        };

        private IEnumerator PulseReel(int index, float scale)
        {
            var rect = (RectTransform)reels[index].transform.parent;
            var start = rect.localScale;
            const float duration = 0.22f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rect.localScale = start * Mathf.Lerp(1f, scale, Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI));
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
            foreach (Transform child in root) Destroy(child.gameObject);
        }
    }
}
