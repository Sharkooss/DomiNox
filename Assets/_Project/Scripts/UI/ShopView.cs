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
            gameObject.AddComponent<Image>().color = new Color(0.05f, 0.07f, 0.09f, 0.98f);
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 14);
            layout.spacing = 12f;

            title = UiFactory.CreateText(transform, "Title", "SHOP", 26, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.86f, 0.38f);
            title.GetComponent<LayoutElement>().preferredHeight = 36f;

            var topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            topRow.transform.SetParent(transform, false);
            topRow.GetComponent<LayoutElement>().preferredHeight = 285f;
            var topLayout = topRow.GetComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 14f;

            var actionColumn = new GameObject("Actions", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            actionColumn.transform.SetParent(topRow.transform, false);
            actionColumn.GetComponent<LayoutElement>().preferredWidth = 150f;
            var actionLayout = actionColumn.GetComponent<VerticalLayoutGroup>();
            actionLayout.spacing = 10f;
            nextRoundButton = UiFactory.CreateButton(actionColumn.transform, "NextRound", "Next Round");
            nextRoundButton.GetComponent<Image>().color = new Color(0.62f, 0.12f, 0.12f);
            nextRoundButton.GetComponent<LayoutElement>().preferredHeight = 58f;
            nextRoundButton.onClick.AddListener(controller.ContinueAfterShop);
            rerollButton = UiFactory.CreateButton(actionColumn.transform, "Reroll", "Reroll $5");
            rerollButton.GetComponent<Image>().color = new Color(0.12f, 0.44f, 0.24f);
            rerollButton.GetComponent<LayoutElement>().preferredHeight = 48f;
            rerollButton.onClick.AddListener(controller.RerollShop);

            dominexOffersRoot = new GameObject("DomiNexOffers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Image), typeof(Outline)).transform;
            dominexOffersRoot.SetParent(topRow.transform, false);
            dominexOffersRoot.GetComponent<LayoutElement>().flexibleWidth = 1f;
            dominexOffersRoot.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.14f, 0.95f);
            dominexOffersRoot.GetComponent<Outline>().effectColor = new Color(0.45f, 0.34f, 0.16f);
            var offersLayout = dominexOffersRoot.GetComponent<HorizontalLayoutGroup>();
            offersLayout.padding = new RectOffset(12, 12, 12, 12);
            offersLayout.spacing = 12f;
            offersLayout.childForceExpandWidth = true;

            bottomOffersRoot = new GameObject("BottomOffers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            bottomOffersRoot.SetParent(transform, false);
            bottomOffersRoot.GetComponent<LayoutElement>().preferredHeight = 190f;
            var bottomLayout = bottomOffersRoot.GetComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 14f;
            bottomLayout.childForceExpandWidth = true;

            detailText = UiFactory.CreateText(transform, "Detail", "Survole une offre pour voir le detail.", 13, TextAnchor.UpperLeft);
            detailText.color = new Color(0.78f, 0.84f, 0.92f);
            detailText.GetComponent<LayoutElement>().flexibleHeight = 1f;

            packOverlayRoot = new GameObject("BoosterPackOverlay", typeof(RectTransform), typeof(Image)).transform;
            packOverlayRoot.SetParent(canvas.transform, false);
            var overlayRect = (RectTransform)packOverlayRoot;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            packOverlayRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

            packPanelRoot = new GameObject("PackPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(Outline)).transform;
            packPanelRoot.SetParent(packOverlayRoot, false);
            var panelRect = (RectTransform)packPanelRoot;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(920f, 500f);
            packPanelRoot.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.99f);
            packPanelRoot.GetComponent<Outline>().effectColor = new Color(0.95f, 0.72f, 0.26f);
            packPanelRoot.GetComponent<Outline>().effectDistance = new Vector2(5f, -5f);
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
            if (run.Phase != RunPhase.Shop)
            {
                return;
            }

            title.text = $"SHOP   Credits: {run.Credits}   DomiNex {run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}   Consumables {run.ActiveConsumableCount}/{run.MaxConsumableSlots}";
            var packOpen = controller.OpenBoosterPack != null;
            nextRoundButton.interactable = !packOpen;
            var rerollPrice = controller.GetCurrentRerollCost();
            rerollButton.GetComponentInChildren<Text>().text = rerollPrice == 0 ? "Reroll FREE" : $"Reroll ${rerollPrice}";
            rerollButton.interactable = !packOpen && run.Credits - rerollPrice >= controller.GetMinimumAllowedCredits();
            Clear(dominexOffersRoot);
            Clear(bottomOffersRoot);
            RenderBoosterPackOverlay(run);

            if (run.CurrentShop != null)
            {
                for (var i = 0; i < run.CurrentShop.Offers.Count; i++)
                {
                    CreateMainShopOffer(i, run.CurrentShop.Offers[i], run);
                }
            }

            CreateFutureSlot();
            if (run.CurrentShop != null)
            {
                for (var i = 0; i < run.CurrentShop.BoosterPackOffers.Count; i++)
                {
                    CreateBoosterPackOffer(i, run.CurrentShop.BoosterPackOffers[i], run);
                }
            }
        }

        private void CreateMainShopOffer(int index, ShopOffer offer, RunState run)
        {
            if (offer.Type == ShopOfferType.Domino)
            {
                CreateDominoOffer(index, offer, run);
                return;
            }

            CreateDomiNexOffer(index, offer, run);
        }

        private void CreateDomiNexOffer(int index, ShopOffer offer, RunState run)
        {
            var root = CreateCard(dominexOffersRoot, $"DomiNex_{index}", DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity));
            root.AddComponent<DomiNexHoverTarget>().Initialize(offer.DomiNex, definition => detailText.text = $"{definition.Name}\n{definition.GetCurrentEffectText(currentRun)}\n{definition.Description}", () => { });
            AddText(root.transform, offer.DomiNex.Rarity.ToString().ToUpperInvariant(), 12, DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity));
            AddText(root.transform, offer.DomiNex.Name, 17, Color.white);
            AddText(root.transform, Short(offer.DomiNex.Description, 86), 12, new Color(0.82f, 0.86f, 0.92f), true);
            var slotsFull = !run.HasFreeDomiNexSlot();
            var price = controller.GetShopOfferPrice(index);
            var buttonLabel = offer.IsPurchased ? "Bought" : slotsFull ? "Slots full" : price == 0 ? "FREE" : $"Buy ${price}";
            var button = UiFactory.CreateButton(root.transform, "Buy", buttonLabel);
            button.GetComponent<LayoutElement>().preferredHeight = 38f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && !slotsFull && run.Credits - price >= controller.GetMinimumAllowedCredits();
            var captured = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(captured));
        }

        private void CreateDominoOffer(int index, ShopOffer offer, RunState run)
        {
            var domino = offer.Domino;
            var root = CreateCard(dominexOffersRoot, $"Domino_{index}", new Color(0.9f, 0.82f, 0.62f));
            AddText(root.transform, "DOMINO", 13, new Color(0.9f, 0.82f, 0.62f));
            AddText(root.transform, $"[ {domino.Left} | {domino.Right} ]", 24, Color.white);
            AddText(root.transform, "Add this domino to your bag.", 12, new Color(0.82f, 0.86f, 0.92f), true);
            var buttonLabel = offer.IsPurchased ? "Bought" : $"Buy ${offer.Price}";
            var button = UiFactory.CreateButton(root.transform, "Buy", buttonLabel);
            button.GetComponent<LayoutElement>().preferredHeight = 38f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && run.Credits - offer.Price >= controller.GetMinimumAllowedCredits();
            var captured = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(captured));
            root.AddComponent<Button>().onClick.AddListener(() => detailText.text = $"Domino {domino}\nAdd a permanent copy to your bag.\nAvailable from the next level.\nPrice: ${offer.Price}");
        }

        private void CreateBoosterPackOffer(int index, BoosterPackShopOffer offer, RunState run)
        {
            var pack = offer.Pack;
            var color = pack.Type == BoosterPackType.Normal ? new Color(0.28f, 0.75f, 0.68f)
                : pack.Type == BoosterPackType.Jumbo ? new Color(0.65f, 0.38f, 0.82f)
                : new Color(0.92f, 0.62f, 0.18f);
            var root = CreateCard(bottomOffersRoot, $"Pack_{index}", color);
            AddText(root.transform, pack.Type.ToString().ToUpperInvariant(), 16, color);
            AddText(root.transform, pack.Name, 15, Color.white);
            var contentName = GetPackContentName(pack.ContentType);
            AddText(root.transform, pack.Type == BoosterPackType.Mega ? $"${pack.Price}  |  Pick up to {pack.PickMax}/{pack.OfferedCardCount}" : $"${pack.Price}  |  Pick {pack.PickMax}/{pack.OfferedCardCount}", 12, new Color(0.82f, 0.86f, 0.92f));
            AddText(root.transform, pack.Description, 11, new Color(0.68f, 0.72f, 0.78f), true);
            var slotsFull = pack.ContentType == BoosterPackContentType.DomiNex ? !run.HasFreeDomiNexSlot() : pack.ContentType != BoosterPackContentType.Domino && !run.HasFreeConsumableSlot();
            var hasChoices = controller.HasAvailableChoicesForPack(pack);
            var slotsFullText = pack.ContentType == BoosterPackContentType.DomiNex ? "DomiNex slots full" : "Slots full";
            var noChoicesText = pack.ContentType == BoosterPackContentType.DomiNex ? "No DomiNex" : pack.ContentType == BoosterPackContentType.Domino ? "No Dominoes" : "No Gem Tiles";
            var label = offer.IsPurchased ? "Bought" : slotsFull ? slotsFullText : !hasChoices ? noChoicesText : $"Buy ${pack.Price}";
            var button = UiFactory.CreateButton(root.transform, "BuyPack", label);
            button.GetComponent<LayoutElement>().preferredHeight = 34f;
            button.interactable = controller.OpenBoosterPack == null && !offer.IsPurchased && !slotsFull && hasChoices && run.Credits - pack.Price >= controller.GetMinimumAllowedCredits();
            var captured = index;
            button.onClick.AddListener(() => controller.BuyBoosterPackOffer(captured));
            root.AddComponent<Button>().onClick.AddListener(() => detailText.text = pack.Type == BoosterPackType.Mega ? $"{pack.Name}\n{pack.Description}\nChoose up to {pack.PickMax} of {pack.OfferedCardCount} {contentName}" : $"{pack.Name}\n{pack.Description}\nPick {pack.PickMax} of {pack.OfferedCardCount} {contentName}");
        }

        private void RenderBoosterPackOverlay(RunState run)
        {
            Clear(packPanelRoot);
            var openPack = controller.OpenBoosterPack;
            packOverlayRoot.gameObject.SetActive(openPack != null);
            if (openPack == null)
            {
                return;
            }

            var titleText = AddText(packPanelRoot, openPack.Pack.Name, 30, new Color(0.98f, 0.86f, 0.38f));
            titleText.GetComponent<LayoutElement>().preferredHeight = 42f;
            var contentName = GetPackContentName(openPack.Pack.ContentType);
            var chooseText = openPack.IsOptionalPick ? $"Choose up to {openPack.MaxPickCount} of {openPack.ChoiceCount} {contentName}" : $"Choose {openPack.ActualPickCount} of {openPack.ChoiceCount} {contentName}";
            AddText(packPanelRoot, $"{chooseText}   Selected {openPack.SelectedIndices.Count}/{openPack.MaxPickCount}", 18, new Color(0.82f, 0.86f, 0.92f));

            var row = new GameObject("Choices", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            row.SetParent(packPanelRoot, false);
            row.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            for (var i = 0; i < openPack.ChoiceCount; i++)
            {
                if (openPack.Pack.ContentType == BoosterPackContentType.DomiNex)
                {
                    CreateDomiNexPackChoice(row, i, openPack.DomiNexChoices[i], openPack.SelectedIndices.Contains(i), run);
                }
                else if (openPack.Pack.ContentType == BoosterPackContentType.Domino)
                {
                    CreateDominoPackChoice(row, i, openPack.DominoChoices[i], openPack.SelectedIndices.Contains(i));
                }
                else
                {
                    CreateBoosterPackChoice(row, i, openPack.Choices[i], openPack.SelectedIndices.Contains(i), run);
                }
            }

            var confirm = UiFactory.CreateButton(packPanelRoot, "ConfirmPack", "Confirm");
            confirm.GetComponent<LayoutElement>().preferredWidth = 220f;
            confirm.GetComponent<LayoutElement>().preferredHeight = 48f;
            confirm.interactable = openPack.SelectedIndices.Count >= openPack.MinPickCount && openPack.SelectedIndices.Count <= openPack.MaxPickCount;
            confirm.onClick.AddListener(controller.ConfirmBoosterPackChoices);
        }

        private void CreateBoosterPackChoice(Transform parent, int index, ConsumableDefinition consumable, bool selected, RunState run)
        {
            var root = CreateCard(parent, $"Choice_{index}", selected ? new Color(0.98f, 0.86f, 0.38f) : new Color(0.28f, 0.75f, 0.68f));
            root.GetComponent<Image>().color = selected ? new Color(0.18f, 0.16f, 0.08f, 0.98f) : new Color(0.1f, 0.14f, 0.17f, 0.98f);
            root.GetComponent<LayoutElement>().preferredWidth = 160f;
            AddText(root.transform, selected ? "SELECTED" : consumable.IconId, 16, selected ? new Color(0.98f, 0.86f, 0.38f) : new Color(0.28f, 0.75f, 0.68f));
            AddText(root.transform, consumable.Name, 18, Color.white);
            var pattern = PatternCatalog.GetById(consumable.TargetPatternId);
            var currentLevel = run.PatternLevels.GetLevel(consumable.TargetPatternId);
            AddText(root.transform, $"{pattern?.Name ?? consumable.TargetPatternId}\nLv.{currentLevel} -> Lv.{System.Math.Min(currentLevel + 1, DomiNox.Core.GameConstants.MaxPatternLevel)}\n{consumable.Description}", 13, new Color(0.82f, 0.86f, 0.92f), true);
            var button = root.AddComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => controller.ToggleBoosterPackChoice(captured));
        }

        private void CreateDomiNexPackChoice(Transform parent, int index, DomiNexDefinition dominex, bool selected, RunState run)
        {
            var rarityColor = DomiNexUiStyles.GetRarityColor(dominex.Rarity);
            var root = CreateCard(parent, $"Choice_{index}", selected ? new Color(0.98f, 0.86f, 0.38f) : rarityColor);
            root.GetComponent<Image>().color = selected ? new Color(0.18f, 0.16f, 0.08f, 0.98f) : new Color(0.1f, 0.12f, 0.18f, 0.98f);
            root.GetComponent<LayoutElement>().preferredWidth = 170f;
            AddText(root.transform, selected ? "SELECTED" : dominex.Rarity.ToString().ToUpperInvariant(), 15, selected ? new Color(0.98f, 0.86f, 0.38f) : rarityColor);
            AddText(root.transform, dominex.Name, 17, Color.white);
            var currentValue = dominex.GetCurrentEffectText(run);
            var tags = dominex.Tags == null || dominex.Tags.Count == 0 ? string.Empty : $"\nTags: {string.Join(", ", dominex.Tags.Take(3))}";
            var effectText = string.IsNullOrWhiteSpace(currentValue) ? dominex.Description : $"{currentValue}\n{dominex.Description}";
            AddText(root.transform, $"{effectText}{tags}\nPack reward", 12, new Color(0.82f, 0.86f, 0.92f), true);
            var button = root.AddComponent<Button>();
            var captured = index;
            button.onClick.AddListener(() => controller.ToggleBoosterPackChoice(captured));
        }

        private void CreateDominoPackChoice(Transform parent, int index, DominoInstance domino, bool selected)
        {
            var modifier = DominoModifierRegistry.GetById(domino.ModifierId);
            var color = modifier?.ColorTheme ?? new Color(0.9f, 0.82f, 0.62f);
            var root = CreateCard(parent, $"Choice_{index}", selected ? new Color(0.98f, 0.86f, 0.38f) : color);
            root.GetComponent<Image>().color = selected ? new Color(0.18f, 0.16f, 0.08f, 0.98f) : Color.Lerp(new Color(0.12f, 0.14f, 0.18f, 0.98f), color, modifier == null ? 0.08f : 0.28f);
            root.GetComponent<LayoutElement>().preferredWidth = 170f;
            AddText(root.transform, selected ? "SELECTED" : modifier?.Name ?? "Domino", 15, selected ? new Color(0.98f, 0.86f, 0.38f) : color);
            AddText(root.transform, $"[ {domino.Definition.Left} | {domino.Definition.Right} ]", 22, Color.white);
            AddText(root.transform, modifier?.Description ?? "No modifier\nAdd this domino to your bag.", 12, new Color(0.82f, 0.86f, 0.92f), true);
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
            var root = CreateCard(bottomOffersRoot, "FutureSlot", new Color(0.18f, 0.24f, 0.26f));
            AddText(root.transform, "Future Slot", 16, new Color(0.34f, 0.4f, 0.44f));
            AddText(root.transform, "Coming Soon", 12, new Color(0.26f, 0.32f, 0.36f), true);
        }

        private static GameObject CreateCard(Transform parent, string name, Color outlineColor)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(Outline), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            root.GetComponent<Outline>().effectColor = outlineColor;
            root.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
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
            if (flexible)
            {
                text.GetComponent<LayoutElement>().flexibleHeight = 1f;
            }

            return text;
        }

        private static void Clear(Transform root)
        {
            foreach (Transform child in root)
            {
                Destroy(child.gameObject);
            }
        }

        private static string Short(string text, int max)
        {
            return text.Length <= max ? text : text.Substring(0, max - 3) + "...";
        }
    }
}
