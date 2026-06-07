using DomiNox.Dominex;
using DomiNox.Run;
using DomiNox.Shop;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ShopView : MonoBehaviour
    {
        private GameFlowController controller;
        private Text title;
        private Transform dominexOffersRoot;
        private Transform bottomOffersRoot;
        private Text detailText;
        private RunState currentRun;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
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
            var nextRound = UiFactory.CreateButton(actionColumn.transform, "NextRound", "Next Round");
            nextRound.GetComponent<Image>().color = new Color(0.62f, 0.12f, 0.12f);
            nextRound.GetComponent<LayoutElement>().preferredHeight = 58f;
            nextRound.onClick.AddListener(controller.ContinueAfterShop);
            var reroll = UiFactory.CreateButton(actionColumn.transform, "Reroll", "Reroll $5");
            reroll.GetComponent<Image>().color = new Color(0.12f, 0.44f, 0.24f);
            reroll.GetComponent<LayoutElement>().preferredHeight = 48f;
            reroll.onClick.AddListener(controller.RerollShop);

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
        }

        public void Render(RunState run)
        {
            gameObject.SetActive(run.Phase == RunPhase.Shop);
            currentRun = run;
            if (run.Phase != RunPhase.Shop)
            {
                return;
            }

            title.text = $"SHOP   Credits: {run.Credits}   DomiNex {run.ActiveDomiNexCount}/{run.MaxDomiNexSlots}   Consumables {run.ActiveConsumableCount}/{DomiNox.Core.GameConstants.MaxConsumableSlots}";
            Clear(dominexOffersRoot);
            Clear(bottomOffersRoot);

            if (run.CurrentShop != null)
            {
                for (var i = 0; i < run.CurrentShop.Offers.Count; i++)
                {
                    CreateDomiNexOffer(i, run.CurrentShop.Offers[i], run);
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

        private void CreateDomiNexOffer(int index, ShopOffer offer, RunState run)
        {
            var root = CreateCard(dominexOffersRoot, $"DomiNex_{index}", DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity));
            root.AddComponent<DomiNexHoverTarget>().Initialize(offer.DomiNex, definition => detailText.text = $"{definition.Name}\n{definition.GetCurrentEffectText(currentRun)}\n{definition.Description}", () => { });
            AddText(root.transform, offer.DomiNex.Rarity.ToString().ToUpperInvariant(), 12, DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity));
            AddText(root.transform, offer.DomiNex.Name, 17, Color.white);
            AddText(root.transform, Short(offer.DomiNex.Description, 86), 12, new Color(0.82f, 0.86f, 0.92f), true);
            var slotsFull = !run.HasFreeDomiNexSlot();
            var buttonLabel = offer.IsPurchased ? "Bought" : slotsFull ? "Slots full" : $"Buy ${offer.Price}";
            var button = UiFactory.CreateButton(root.transform, "Buy", buttonLabel);
            button.GetComponent<LayoutElement>().preferredHeight = 38f;
            button.interactable = !offer.IsPurchased && !slotsFull && run.Credits - offer.Price >= controller.GetMinimumAllowedCredits();
            var captured = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(captured));
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
            AddText(root.transform, $"${pack.Price}  |  Pick {pack.PickCount}/{pack.OfferedCardCount}", 12, new Color(0.82f, 0.86f, 0.92f));
            AddText(root.transform, pack.Description, 11, new Color(0.68f, 0.72f, 0.78f), true);
            var slotsFull = !run.HasFreeConsumableSlot();
            var label = offer.IsPurchased ? "Bought" : slotsFull ? "Slots full" : $"Buy ${pack.Price}";
            var button = UiFactory.CreateButton(root.transform, "BuyPack", label);
            button.GetComponent<LayoutElement>().preferredHeight = 34f;
            button.interactable = !offer.IsPurchased && !slotsFull && run.Credits - pack.Price >= controller.GetMinimumAllowedCredits();
            var captured = index;
            button.onClick.AddListener(() => controller.BuyBoosterPackOffer(captured));
            root.AddComponent<Button>().onClick.AddListener(() => detailText.text = $"{pack.Name}\n{pack.Description}\nPick {pack.PickCount} of {pack.OfferedCardCount} Gem Tiles");
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

        private static void AddText(Transform parent, string value, int size, Color color, bool flexible = false)
        {
            var text = UiFactory.CreateText(parent, "Text", value, size, TextAnchor.MiddleCenter);
            text.color = color;
            if (flexible)
            {
                text.GetComponent<LayoutElement>().flexibleHeight = 1f;
            }
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
