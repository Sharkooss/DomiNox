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
        private Transform offersRoot;
        private Transform futureSlotsRoot;
        private Button continueButton;
        private DomiNexDetailCardView detailCard;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.07f, 0.09f, 0.98f);

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var leftColumn = new GameObject("ShopLeft", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            leftColumn.transform.SetParent(transform, false);
            leftColumn.GetComponent<LayoutElement>().flexibleWidth = 1f;
            var leftLayout = leftColumn.GetComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 14f;

            title = UiFactory.CreateText(leftColumn.transform, "Title", "SHOP - DOMINEX", 34, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.86f, 0.38f);

            offersRoot = new GameObject("DomiNexOffers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Image), typeof(Outline)).transform;
            offersRoot.SetParent(leftColumn.transform, false);
            offersRoot.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.14f, 0.95f);
            offersRoot.GetComponent<Outline>().effectColor = new Color(0.45f, 0.34f, 0.16f);
            offersRoot.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            offersRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;
            var offersLayout = offersRoot.GetComponent<HorizontalLayoutGroup>();
            offersLayout.padding = new RectOffset(14, 14, 14, 14);
            offersLayout.spacing = 14f;
            offersLayout.childForceExpandWidth = true;
            offersLayout.childForceExpandHeight = true;

            futureSlotsRoot = new GameObject("FutureAugments", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(Image), typeof(Outline)).transform;
            futureSlotsRoot.SetParent(leftColumn.transform, false);
            futureSlotsRoot.GetComponent<Image>().color = new Color(0.06f, 0.09f, 0.1f, 0.95f);
            futureSlotsRoot.GetComponent<Outline>().effectColor = new Color(0.22f, 0.28f, 0.3f);
            futureSlotsRoot.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            futureSlotsRoot.GetComponent<LayoutElement>().preferredHeight = 190f;
            var futureLayout = futureSlotsRoot.GetComponent<HorizontalLayoutGroup>();
            futureLayout.padding = new RectOffset(14, 14, 42, 14);
            futureLayout.spacing = 14f;
            futureLayout.childForceExpandWidth = true;
            futureLayout.childForceExpandHeight = true;
            BuildFutureSlots();

            var rightColumn = new GameObject("ShopRight", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            rightColumn.transform.SetParent(transform, false);
            rightColumn.GetComponent<LayoutElement>().preferredWidth = 310f;
            var rightLayout = rightColumn.GetComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 14f;

            detailCard = new GameObject("DomiNexDetail", typeof(RectTransform), typeof(LayoutElement)).AddComponent<DomiNexDetailCardView>();
            detailCard.transform.SetParent(rightColumn.transform, false);
            detailCard.GetComponent<LayoutElement>().flexibleHeight = 1f;
            detailCard.Initialize("Survole un DomiNex");

            continueButton = UiFactory.CreateButton(rightColumn.transform, "ContinueButton", "Niveau suivant >>");
            continueButton.GetComponent<LayoutElement>().preferredHeight = 64f;
            continueButton.onClick.AddListener(controller.ContinueAfterShop);
        }

        public void Render(RunState run)
        {
            gameObject.SetActive(run.Phase == RunPhase.Shop);
            if (run.Phase != RunPhase.Shop)
            {
                return;
            }

            title.text = $"SHOP - DOMINEX   Credits: {run.Credits}";
            foreach (Transform child in offersRoot)
            {
                Destroy(child.gameObject);
            }

            if (run.CurrentShop == null || run.CurrentShop.Offers.Count == 0)
            {
                UiFactory.CreateText(offersRoot, "Empty", "Aucune offre disponible.", 18, TextAnchor.MiddleCenter);
                detailCard.Render(null, "Aucune offre");
                return;
            }

            for (var i = 0; i < run.CurrentShop.Offers.Count; i++)
            {
                CreateOffer(i, run.CurrentShop.Offers[i], run.Credits);
            }

            detailCard.Render(run.CurrentShop.Offers[0].DomiNex);
        }

        private void CreateOffer(int index, ShopOffer offer, int credits)
        {
            var root = new GameObject($"Offer_{index}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(Outline), typeof(LayoutElement), typeof(DomiNexHoverTarget));
            root.transform.SetParent(offersRoot, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            root.GetComponent<Outline>().effectColor = DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity);
            root.GetComponent<Outline>().effectDistance = new Vector2(4f, -4f);
            root.GetComponent<LayoutElement>().flexibleWidth = 1f;
            root.GetComponent<DomiNexHoverTarget>().Initialize(offer.DomiNex, definition => detailCard.Render(definition), () => { });

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var rarityBadge = new GameObject("RarityBadge", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rarityBadge.transform.SetParent(root.transform, false);
            rarityBadge.GetComponent<Image>().color = DomiNexUiStyles.GetRarityColor(offer.DomiNex.Rarity);
            rarityBadge.GetComponent<LayoutElement>().preferredHeight = 32f;

            var rarity = UiFactory.CreateText(rarityBadge.transform, "Rarity", offer.DomiNex.Rarity.ToString().ToUpperInvariant(), 16, TextAnchor.MiddleCenter);
            rarity.color = Color.black;
            Stretch(rarity.rectTransform);

            var header = UiFactory.CreateText(root.transform, "Header", offer.DomiNex.Name, 22, TextAnchor.MiddleCenter);
            header.color = offer.IsPurchased ? Color.gray : Color.white;

            var shortDescription = offer.DomiNex.Description.Length > 72 ? offer.DomiNex.Description.Substring(0, 69) + "..." : offer.DomiNex.Description;
            var description = UiFactory.CreateText(root.transform, "ShortDescription", shortDescription, 15, TextAnchor.UpperCenter);
            description.color = new Color(0.82f, 0.86f, 0.92f);
            description.GetComponent<LayoutElement>().flexibleHeight = 1f;

            var button = UiFactory.CreateButton(root.transform, "BuyButton", offer.IsPurchased ? "Acheté" : $"Acheter ${offer.Price}");
            button.GetComponent<LayoutElement>().preferredHeight = 54f;
            button.interactable = !offer.IsPurchased && credits >= offer.Price;
            var capturedIndex = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(capturedIndex));
        }

        private void BuildFutureSlots()
        {
            for (var i = 0; i < 4; i++)
            {
                var slot = new GameObject($"FutureSlot_{i}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
                slot.transform.SetParent(futureSlotsRoot, false);
                slot.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.12f, 0.86f);
                slot.GetComponent<Outline>().effectColor = new Color(0.18f, 0.24f, 0.26f);
                slot.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
                slot.GetComponent<LayoutElement>().flexibleWidth = 1f;

                var label = UiFactory.CreateText(slot.transform, "Label", "FUTURE\nAUGMENT", 18, TextAnchor.MiddleCenter);
                label.color = new Color(0.28f, 0.34f, 0.36f);
                Stretch(label.rectTransform);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
