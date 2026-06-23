using DomiNox.Consumables;
using DomiNox.Dominex;
using DomiNox.Dominoes;
using DomiNox.Patterns;
using DomiNox.Run;
using DomiNox.Shop;
using DomiNox.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ShopView : MonoBehaviour
    {
        private GameFlowController controller;
        private Text title;
        private Button nextRoundButton;
        private Button rerollButton;
        private Transform dominexOffersRoot;
        private Transform bottomOffersRoot;
        private Transform packOverlayRoot;
        private Transform packPanelRoot;
        private Text detailText;
        private RunState currentRun;
        private Canvas canvas;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            canvas = GetComponentInParent<Canvas>();
            gameObject.AddComponent<Image>().color = DomiNoxTheme.BgDeep;
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 14);
            layout.spacing = 12f;

            title = UiFactory.CreateText(transform, "Title", "SHOP", DomiNoxTheme.FontXL, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.5f), new Vector2(1f, -1f));
            title.GetComponent<LayoutElement>().preferredHeight = 40f;

            UiFactory.CreateSeparator(transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.Gold, 0.35f));

            var topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            topRow.transform.SetParent(transform, false);
            topRow.GetComponent<LayoutElement>().preferredHeight = 290f;
            var topLayout = topRow.GetComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 14f;

            var actionColumn = new GameObject("Actions", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            actionColumn.transform.SetParent(topRow.transform, false);
            actionColumn.GetComponent<LayoutElement>().preferredWidth = 154f;
            var actionLayout = actionColumn.GetComponent<VerticalLayoutGroup>();
            actionLayout.spacing = 10f;

            nextRoundButton = UiFactory.CreateButton(actionColumn.transform, "NextRound", "Next Round  →");
            nextRoundButton.GetComponent<LayoutElement>().preferredHeight = 62f;
            UiFactory.StyleButton(nextRoundButton, new Color(0.42f, 0.08f, 0.08f), DomiNoxTheme.Danger, 62f);
            var nextLabel = nextRoundButton.GetComponentInChildren<Text>();
            if (nextLabel != null) nextLabel.fontSize = DomiNoxTheme.FontMD;
            nextRoundButton.onClick.AddListener(controller.ContinueAfterShop);

            rerollButton = UiFactory.CreateButton(actionColumn.transform, "Reroll", "Reroll $5");
            rerollButton.GetComponent<LayoutElement>().preferredHeight = 48f;
            UiFactory.StyleButton(rerollButton, new Color(0.08f, 0.22f, 0.12f), DomiNoxTheme.Success, 48f);
            rerollButton.onClick.AddListener(controller.RerollShop);

            dominexOffersRoot = new GameObject("DomiNexOffers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Image)).transform;
            dominexOffersRoot.SetParent(topRow.transform, false);
            dominexOffersRoot.GetComponent<LayoutElement>().flexibleWidth = 1f;
            dominexOffersRoot.GetComponent<Image>().color = DomiNoxTheme.BgPanel;
            DomiNoxTheme.AddOutline(dominexOffersRoot.gameObject, DomiNoxTheme.WithAlpha(DomiNoxTheme.GoldDim, 0.55f));
            var offersLayout = dominexOffersRoot.GetComponent<HorizontalLayoutGroup>();
            offersLayout.padding = new RectOffset(12, 12, 12, 12);
            offersLayout.spacing = 12f;
            offersLayout.childForceExpandWidth = true;

            bottomOffersRoot = new GameObject("BottomOffers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            bottomOffersRoot.SetParent(transform, false);
            bottomOffersRoot.GetComponent<LayoutElement>().preferredHeight = 196f;
            var bottomLayout = bottomOffersRoot.GetComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 14f;
            bottomLayout.childForceExpandWidth = true;

            detailText = UiFactory.CreateText(transform, "Detail", "Survole une offre pour voir le detail.", DomiNoxTheme.FontXS, TextAnchor.UpperLeft);
            detailText.color = DomiNoxTheme.TextSecondary;
            detailText.GetComponent<LayoutElement>().flexibleHeight = 1f;

            packOverlayRoot = new GameObject("BoosterPackOverlay", typeof(RectTransform), typeof(Image)).transform;
            packOverlayRoot.SetParent(canvas.transform, false);
            var overlayRect = (RectTransform)packOverlayRoot;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            packOverlayRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

            packPanelRoot = new GameObject("PackPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image)).transform;
            packPanelRoot.SetParent(packOverlayRoot, false);
            var panelRect = (RectTransform)packPanelRoot;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot     = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(920f, 500f);
            packPanelRoot.GetComponent<Image>().color = DomiNoxTheme.BgOverlay;
            DomiNoxTheme.AddOutline(packPanelRoot.gameObject, DomiNoxTheme.JackpotAmber, 3f);
            DomiNoxTheme.AddShadow(packPanelRoot.gameObject, new Color(0f, 0f, 0f, 0.6f), new Vector2(6f, -6f));
            var overlayLayout = packPanelRoot.GetComponent<VerticalLayoutGroup>();
            overlayLayout.padding = new RectOffset(26, 26, 22, 22);
            overlayLayout.spacing = 14f;
            overlayLayout.childAlignment = TextAnchor.UpperCenter;
            packOverlayRoot.gameObject.SetActive(false);
        }

        public void Render(RunState run)
        {
            gameObject.SetActive(run.Phase == RunPhase.Shop);
            currentRun = run;
            if (run.Phase != RunPhase.Shop) return;

            title.text = $"SHOP   ${run.Credits}   DomiNex {run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}   Gems {run.ActiveConsumableCount}/{run.MaxConsumableSlots}";
            var packOpen = controller.OpenBoosterPack != null;
            nextRoundButton.interactable = !packOpen;

            var rerollPrice = controller.GetCurrentRerollCost();
            var rerollLabel = rerollPrice == 0 ? "Reroll  FREE" : $"Reroll  ${rerollPrice}";
            rerollButton.GetComponentInChildren<Text>().text = rerollLabel;
            rerollButton.interactable = !packOpen && run.Credits - rerollPrice >= controller.GetMinimumAllowedCredits();
            UiFactory.StyleButton(rerollButton, rerollPrice == 0 ? new Color(0.30f, 0.20f, 0.04f) : new Color(0.08f, 0.22f, 0.12f),
                rerollPrice == 0 ? DomiNoxTheme.Gold : DomiNoxTheme.Success, 48f);

            Clear(dominexOffersRoot);
            Clear(bottomOffersRoot);
            RenderBoosterPackOverlay(run);

            if (run.CurrentShop != null)
            {
                for (var i = 0; i < run.CurrentShop.Offers.Count; i++)
                    CreateMainShopOffer(i, run.CurrentShop.Offers[i], run);
            }

            CreateFutureSlot();

            if (run.CurrentShop != null)
            {
                for (var i = 0; i < run.CurrentShop.BoosterPackOffers.Count; i++)
                    CreateBoosterPackOffer(i, run.CurrentShop.BoosterPackOffers[i], run);
            }
        }

        private void CreateMainShopOffer(int index, ShopOffer offer, RunState run)
        {
            if (offer.Type == ShopOfferType.Domino) { CreateDominoOffer(index, offer, run); return; }
            CreateDomiNexOffer(index, offer, run);
        }

        private void CreateDomiNexOffer(int index, ShopOffer offer, RunState run)
        {
            var rarityColor = DomiNoxTheme.GetRarityColor(offer.DomiNex.Rarity);
            var root = CreateCard(dominexOffersRoot, $"DomiNex_{index}", rarityColor);
            root.AddComponent<DomiNexHoverTarget>().Initialize(offer.DomiNex, definition => detailText.text = $"{definition.Name}\n{definition.GetCurrentEffectText(currentRun)}\n{definition.Description}", () => { });

            AddText(root.transform, offer.DomiNex.Rarity.ToString().ToUpperInvariant(), DomiNoxTheme.FontXS, rarityColor);
            var nameText = AddText(root.transform, offer.DomiNex.Name, DomiNoxTheme.FontMD, DomiNoxTheme.TextPrimary);
            nameText.fontStyle = FontStyle.Bold;
            AddText(root.transform, Short(offer.DomiNex.Description, 86), DomiNoxTheme.FontXS, DomiNoxTheme.TextSecondary, true);

            var slotsFull = !run.HasFreeDomiNexSlot();
            var price = controller.GetShopOfferPrice(index);
            var canAfford = run.Credits - price >= controller.GetMinimumAllowedCredits();
            var buttonLabel = offer.IsPurchased ? "Bought" : slotsFull ? "Slots full" : price == 0 ? "FREE" : $"Buy  ${price}";
            var button = UiFactory.CreateButton(root.transform, "Buy", buttonLabel);
            button.GetComponent<LayoutElement>().preferredHeight = 40f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && !slotsFull && canAfford;
            var btnBg  = offer.IsPurchased ? DomiNoxTheme.BgCard : !canAfford ? new Color(0.30f, 0.08f, 0.06f) : new Color(0.08f, 0.22f, 0.12f);
            var btnCol = offer.IsPurchased ? DomiNoxTheme.TextMuted : !canAfford ? DomiNoxTheme.Danger : DomiNoxTheme.Success;
            UiFactory.StyleButton(button, btnBg, btnCol, 40f);
            var captured = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(captured));
        }

        private void CreateDominoOffer(int index, ShopOffer offer, RunState run)
        {
            var domino = offer.Domino;
            // Render as a real ivory domino tile so the dark pips stay readable.
            var root = CreateCard(dominexOffersRoot, $"Domino_{index}", DomiNoxTheme.GoldDim);
            root.GetComponent<Image>().color = DomiNoxTheme.IvoryWhite;
            var head = AddText(root.transform, "DOMINO", DomiNoxTheme.FontXS, DomiNoxTheme.IvoryDark);
            head.fontStyle = FontStyle.Bold;
            var valText = AddText(root.transform, $"[ {domino.Left} | {domino.Right} ]", DomiNoxTheme.FontXL, DomiNoxTheme.IvoryDark, true);
            valText.fontStyle = FontStyle.Bold;
            var canAfford = run.Credits - offer.Price >= controller.GetMinimumAllowedCredits();
            var buttonLabel = offer.IsPurchased ? "Bought" : $"Buy  ${offer.Price}";
            var button = UiFactory.CreateButton(root.transform, "Buy", buttonLabel);
            button.GetComponent<LayoutElement>().preferredHeight = 40f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && canAfford;
            UiFactory.StyleButton(button, new Color(0.08f, 0.22f, 0.12f), DomiNoxTheme.Success, 40f);
            var captured = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(captured));
            root.AddComponent<Button>().onClick.AddListener(() => detailText.text = $"Domino {domino}\nAdd a permanent copy to your bag.\nPrice: ${offer.Price}");
        }

        private void CreateBoosterPackOffer(int index, BoosterPackShopOffer offer, RunState run)
        {
            var pack = offer.Pack;
            var color = pack.Type == BoosterPackType.Normal ? new Color(0.22f, 0.72f, 0.65f)
                : pack.Type == BoosterPackType.Jumbo        ? DomiNoxTheme.RarityEpic
                : DomiNoxTheme.JackpotAmber;
            var root = CreateCard(bottomOffersRoot, $"Pack_{index}", color);
            AddText(root.transform, pack.Type.ToString().ToUpperInvariant(), DomiNoxTheme.FontMD, color);
            var nameText = AddText(root.transform, pack.Name, DomiNoxTheme.FontSM, DomiNoxTheme.TextPrimary);
            nameText.fontStyle = FontStyle.Bold;
            var pickLabel = pack.Type == BoosterPackType.Mega ? $"${pack.Price}  ·  Pick up to {pack.PickMax}/{pack.OfferedCardCount}" : $"${pack.Price}  ·  Pick {pack.PickMax}/{pack.OfferedCardCount}";
            AddText(root.transform, pickLabel, DomiNoxTheme.FontXS, DomiNoxTheme.TextSecondary, true);

            var slotsFull  = pack.ContentType == BoosterPackContentType.DomiNex ? !run.HasFreeDomiNexSlot() : pack.ContentType != BoosterPackContentType.Domino && !run.HasFreeConsumableSlot();
            var hasChoices = controller.HasAvailableChoicesForPack(pack);
            var label = offer.IsPurchased ? "Bought" : slotsFull ? "Slots full" : !hasChoices ? "None available" : $"Buy  ${pack.Price}";
            var button = UiFactory.CreateButton(root.transform, "BuyPack", label);
            button.GetComponent<LayoutElement>().preferredHeight = 36f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && !slotsFull && hasChoices && run.Credits - pack.Price >= controller.GetMinimumAllowedCredits();
            UiFactory.StyleButton(button, new Color(0.08f, 0.22f, 0.12f), DomiNoxTheme.Success, 36f);
            var captured = index;
            button.onClick.AddListener(() => controller.BuyBoosterPackOffer(captured));
            root.AddComponent<Button>().onClick.AddListener(() => detailText.text = $"{pack.Name}\n{pack.Description}\nPick {pack.PickMax} of {pack.OfferedCardCount}");
        }

        private void RenderBoosterPackOverlay(RunState run)
        {
            Clear(packPanelRoot);
            var openPack = controller.OpenBoosterPack;
            packOverlayRoot.gameObject.SetActive(openPack != null);
            if (openPack == null) return;

            var packTitle = AddText(packPanelRoot, openPack.Pack.Name, DomiNoxTheme.FontXL, DomiNoxTheme.Gold);
            packTitle.fontStyle = FontStyle.Bold;
            packTitle.GetComponent<LayoutElement>().preferredHeight = 46f;

            var contentName = GetPackContentName(openPack.Pack.ContentType);
            var chooseText  = openPack.IsOptionalPick ? $"Choose up to {openPack.MaxPickCount} of {openPack.ChoiceCount} {contentName}" : $"Choose {openPack.ActualPickCount} of {openPack.ChoiceCount} {contentName}";
            AddText(packPanelRoot, $"{chooseText}   Selected {openPack.SelectedIndices.Count}/{openPack.MaxPickCount}", DomiNoxTheme.FontMD, DomiNoxTheme.TextSecondary);

            var row = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            row.SetParent(packPanelRoot, false);
            row.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childForceExpandWidth  = true;
            rowLayout.childForceExpandHeight = true;

            for (var i = 0; i < openPack.ChoiceCount; i++)
            {
                if (openPack.Pack.ContentType == BoosterPackContentType.DomiNex)
                    CreateDomiNexPackChoice(row, i, openPack.DomiNexChoices[i], openPack.SelectedIndices.Contains(i), run);
                else if (openPack.Pack.ContentType == BoosterPackContentType.Domino)
                    CreateDominoPackChoice(row, i, openPack.DominoChoices[i], openPack.SelectedIndices.Contains(i));
                else
                    CreateBoosterPackChoice(row, i, openPack.Choices[i], openPack.SelectedIndices.Contains(i), run);
            }

            var confirm = UiFactory.CreateButton(packPanelRoot, "ConfirmPack", "Confirm  →");
            confirm.GetComponent<LayoutElement>().preferredWidth  = 240f;
            confirm.GetComponent<LayoutElement>().preferredHeight = 50f;
            confirm.interactable = openPack.SelectedIndices.Count >= openPack.MinPickCount && openPack.SelectedIndices.Count <= openPack.MaxPickCount;
            UiFactory.StyleButton(confirm, new Color(0.08f, 0.22f, 0.12f), DomiNoxTheme.Success, 50f);
            confirm.onClick.AddListener(controller.ConfirmBoosterPackChoices);
        }

        private void CreateBoosterPackChoice(Transform parent, int index, ConsumableDefinition consumable, bool selected, RunState run)
        {
            var outlineColor = selected ? DomiNoxTheme.Gold : new Color(0.22f, 0.72f, 0.65f);
            var root = CreateCard(parent, $"Choice_{index}", outlineColor);
            root.GetComponent<Image>().color = selected ? new Color(0.14f, 0.12f, 0.06f) : DomiNoxTheme.BgCard;
            root.GetComponent<LayoutElement>().preferredWidth = 160f;
            var labelText = AddText(root.transform, selected ? "SELECTED" : consumable.IconId, DomiNoxTheme.FontMD, selected ? DomiNoxTheme.Gold : outlineColor);
            labelText.fontStyle = FontStyle.Bold;
            var nameText = AddText(root.transform, consumable.Name, DomiNoxTheme.FontMD, DomiNoxTheme.TextPrimary);
            nameText.fontStyle = FontStyle.Bold;
            var pattern  = PatternCatalog.GetById(consumable.TargetPatternId);
            var currentLevel = run.PatternLevels.GetLevel(consumable.TargetPatternId);
            AddText(root.transform, $"{pattern?.Name ?? consumable.TargetPatternId}\nLv.{currentLevel} → Lv.{System.Math.Min(currentLevel + 1, DomiNox.Core.GameConstants.MaxPatternLevel)}\n{consumable.Description}", DomiNoxTheme.FontXS, DomiNoxTheme.TextSecondary, true);
            var button = root.AddComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => controller.ToggleBoosterPackChoice(captured));
        }

        private void CreateDomiNexPackChoice(Transform parent, int index, DomiNexDefinition dominex, bool selected, RunState run)
        {
            var rarityColor  = DomiNoxTheme.GetRarityColor(dominex.Rarity);
            var outlineColor = selected ? DomiNoxTheme.Gold : rarityColor;
            var root = CreateCard(parent, $"Choice_{index}", outlineColor);
            root.GetComponent<Image>().color = selected ? new Color(0.14f, 0.12f, 0.06f) : Color.Lerp(DomiNoxTheme.BgCard, rarityColor, 0.10f);
            root.GetComponent<LayoutElement>().preferredWidth = 170f;
            var labelText = AddText(root.transform, selected ? "SELECTED" : dominex.Rarity.ToString().ToUpperInvariant(), DomiNoxTheme.FontSM, selected ? DomiNoxTheme.Gold : rarityColor);
            labelText.fontStyle = FontStyle.Bold;
            var nameText = AddText(root.transform, dominex.Name, DomiNoxTheme.FontMD, DomiNoxTheme.TextPrimary);
            nameText.fontStyle = FontStyle.Bold;
            var currentValue = dominex.GetCurrentEffectText(run);
            var effectText   = string.IsNullOrWhiteSpace(currentValue) ? dominex.Description : $"{currentValue}\n{dominex.Description}";
            AddText(root.transform, effectText, DomiNoxTheme.FontXS, DomiNoxTheme.TextSecondary, true);
            var button = root.AddComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => controller.ToggleBoosterPackChoice(captured));
        }

        private void CreateDominoPackChoice(Transform parent, int index, DominoInstance domino, bool selected)
        {
            var modifier     = DominoModifierRegistry.GetById(domino.ModifierId);
            var color        = modifier?.ColorTheme ?? DomiNoxTheme.IvoryWhite;
            var outlineColor = selected ? DomiNoxTheme.Gold : color;
            var root = CreateCard(parent, $"Choice_{index}", outlineColor);
            root.GetComponent<Image>().color = selected ? new Color(0.14f, 0.12f, 0.06f) : Color.Lerp(DomiNoxTheme.BgCard, color, 0.20f);
            root.GetComponent<LayoutElement>().preferredWidth = 170f;
            var labelText = AddText(root.transform, selected ? "SELECTED" : (modifier?.Name ?? "Domino"), DomiNoxTheme.FontSM, selected ? DomiNoxTheme.Gold : color);
            labelText.fontStyle = FontStyle.Bold;
            var valText = AddText(root.transform, $"[ {domino.Definition.Left} | {domino.Definition.Right} ]", DomiNoxTheme.FontLG, DomiNoxTheme.TextPrimary);
            valText.fontStyle = FontStyle.Bold;
            AddText(root.transform, modifier?.Description ?? "Add this domino to your bag.", DomiNoxTheme.FontXS, DomiNoxTheme.TextSecondary, true);
            var button = root.AddComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => controller.ToggleBoosterPackChoice(captured));
        }

        private static string GetPackContentName(BoosterPackContentType contentType)
        {
            return contentType == BoosterPackContentType.DomiNex ? "DomiNex" : contentType == BoosterPackContentType.Domino ? "Dominoes" : "Gem Tiles";
        }

        private void CreateFutureSlot()
        {
            var root = CreateCard(bottomOffersRoot, "FutureSlot", DomiNoxTheme.BorderNormal);
            root.GetComponent<Image>().color = DomiNoxTheme.WithAlpha(DomiNoxTheme.BgPanel, 0.5f);
            AddText(root.transform, "Future Slot", DomiNoxTheme.FontMD, DomiNoxTheme.TextMuted);
            AddText(root.transform, "Coming Soon", DomiNoxTheme.FontXS, DomiNoxTheme.WithAlpha(DomiNoxTheme.TextMuted, 0.6f), true);
        }

        private static GameObject CreateCard(Transform parent, string name, Color outlineColor)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = DomiNoxTheme.BgCard;
            DomiNoxTheme.AddOutline(root, outlineColor, 2.5f);
            root.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            return root;
        }

        private static Text AddText(Transform parent, string value, int size, Color color, bool flexible = false)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, TextAnchor.MiddleCenter);
            text.color = color;
            if (flexible) text.GetComponent<LayoutElement>().flexibleHeight = 1f;
            return text;
        }

        private static void Clear(Transform root)
        {
            foreach (Transform child in root) Destroy(child.gameObject);
        }

        private static string Short(string text, int max)
        {
            return text.Length <= max ? text : text.Substring(0, max - 3) + "...";
        }
    }
}
