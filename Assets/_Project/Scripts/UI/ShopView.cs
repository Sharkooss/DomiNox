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
        private Button continueButton;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.06f, 0.07f, 0.1f, 0.96f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperCenter;

            title = UiFactory.CreateText(transform, "Title", "Section DomiNex", 34, TextAnchor.MiddleCenter);
            title.color = new Color(0.98f, 0.86f, 0.38f);

            offersRoot = new GameObject("Offers", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement)).transform;
            offersRoot.SetParent(transform, false);
            var offersLayout = offersRoot.GetComponent<HorizontalLayoutGroup>();
            offersLayout.spacing = 14f;
            offersLayout.childForceExpandWidth = true;
            offersLayout.childForceExpandHeight = true;
            offersRoot.GetComponent<LayoutElement>().flexibleHeight = 1f;

            continueButton = UiFactory.CreateButton(transform, "ContinueButton", "Passer au niveau suivant");
            continueButton.GetComponent<LayoutElement>().preferredHeight = 54f;
            continueButton.onClick.AddListener(controller.ContinueAfterShop);
        }

        public void Render(RunState run)
        {
            gameObject.SetActive(run.Phase == RunPhase.Shop);
            if (run.Phase != RunPhase.Shop)
            {
                return;
            }

            title.text = $"Section DomiNex - Credits: {run.Credits}";
            foreach (Transform child in offersRoot)
            {
                Destroy(child.gameObject);
            }

            if (run.CurrentShop == null || run.CurrentShop.Offers.Count == 0)
            {
                UiFactory.CreateText(offersRoot, "Empty", "Aucune offre disponible.", 16, TextAnchor.MiddleCenter);
                return;
            }

            for (var i = 0; i < run.CurrentShop.Offers.Count; i++)
            {
                CreateOffer(i, run.CurrentShop.Offers[i], run.Credits);
            }
        }

        private void CreateOffer(int index, ShopOffer offer, int credits)
        {
            var root = new GameObject($"Offer_{index}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image), typeof(Outline), typeof(LayoutElement));
            root.transform.SetParent(offersRoot, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            root.GetComponent<Outline>().effectColor = GetRarityColor(offer.DomiNex.Rarity);
            root.GetComponent<Outline>().effectDistance = new Vector2(4f, -4f);
            root.GetComponent<LayoutElement>().flexibleWidth = 1f;
            root.GetComponent<LayoutElement>().minHeight = 300f;

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.UpperLeft;

            var rarityBadge = new GameObject("RarityBadge", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            rarityBadge.transform.SetParent(root.transform, false);
            rarityBadge.GetComponent<Image>().color = GetRarityColor(offer.DomiNex.Rarity);
            rarityBadge.GetComponent<LayoutElement>().preferredHeight = 34f;

            var rarity = UiFactory.CreateText(rarityBadge.transform, "Rarity", offer.DomiNex.Rarity.ToString().ToUpperInvariant(), 18, TextAnchor.MiddleCenter);
            rarity.color = Color.black;
            rarity.rectTransform.anchorMin = Vector2.zero;
            rarity.rectTransform.anchorMax = Vector2.one;
            rarity.rectTransform.offsetMin = Vector2.zero;
            rarity.rectTransform.offsetMax = Vector2.zero;

            var header = UiFactory.CreateText(root.transform, "Header", offer.DomiNex.Name, 24, TextAnchor.MiddleCenter);
            header.color = offer.IsPurchased ? Color.gray : Color.white;
            header.horizontalOverflow = HorizontalWrapMode.Wrap;
            header.verticalOverflow = VerticalWrapMode.Overflow;

            var price = UiFactory.CreateText(root.transform, "Price", $"Prix: {offer.Price} credits", 18, TextAnchor.MiddleCenter);
            price.color = credits >= offer.Price ? new Color(0.8f, 1f, 0.72f) : new Color(1f, 0.52f, 0.52f);

            var description = UiFactory.CreateText(root.transform, "Description", offer.DomiNex.Description, 17, TextAnchor.UpperLeft);
            description.color = new Color(0.82f, 0.86f, 0.92f);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.verticalOverflow = VerticalWrapMode.Overflow;
            description.GetComponent<LayoutElement>().flexibleHeight = 1f;

            var button = UiFactory.CreateButton(root.transform, "BuyButton", offer.IsPurchased ? "Acheté" : "Acheter");
            button.GetComponent<LayoutElement>().preferredHeight = 54f;
            button.interactable = !offer.IsPurchased && credits >= offer.Price;
            var capturedIndex = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(capturedIndex));
        }

        private static Color GetRarityColor(DomiNexRarity rarity)
        {
            return rarity switch
            {
                DomiNexRarity.Common => new Color(0.78f, 0.82f, 0.88f),
                DomiNexRarity.Rare => new Color(0.25f, 0.55f, 1f),
                DomiNexRarity.Epic => new Color(0.72f, 0.35f, 1f),
                DomiNexRarity.Legendary => new Color(1f, 0.72f, 0.18f),
                DomiNexRarity.Cursed => new Color(0.9f, 0.12f, 0.16f),
                _ => Color.white
            };
        }
    }
}
