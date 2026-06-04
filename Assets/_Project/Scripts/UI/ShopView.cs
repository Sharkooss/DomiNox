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
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;

            title = UiFactory.CreateText(transform, "Title", "Shop DomiNex", 22, TextAnchor.MiddleCenter);
            offersRoot = new GameObject("Offers", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
            offersRoot.SetParent(transform, false);
            offersRoot.GetComponent<VerticalLayoutGroup>().spacing = 6f;

            continueButton = UiFactory.CreateButton(transform, "ContinueButton", "Passer au niveau suivant");
            continueButton.onClick.AddListener(controller.ContinueAfterShop);
        }

        public void Render(RunState run)
        {
            gameObject.SetActive(run.Phase == RunPhase.Shop);
            if (run.Phase != RunPhase.Shop)
            {
                return;
            }

            title.text = $"Shop DomiNex - Credits: {run.Credits}";
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
            var root = new GameObject($"Offer_{index}", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            root.transform.SetParent(offersRoot, false);
            root.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 4f;

            var header = UiFactory.CreateText(root.transform, "Header", $"{offer.DomiNex.Name} [{offer.DomiNex.Rarity}] - {offer.Price} credits", 16, TextAnchor.MiddleLeft);
            header.color = offer.IsPurchased ? Color.gray : Color.white;

            var description = UiFactory.CreateText(root.transform, "Description", offer.DomiNex.Description, 13, TextAnchor.MiddleLeft);
            description.color = new Color(0.82f, 0.86f, 0.92f);

            var button = UiFactory.CreateButton(root.transform, "BuyButton", offer.IsPurchased ? "Acheté" : "Acheter");
            button.interactable = !offer.IsPurchased && credits >= offer.Price;
            var capturedIndex = index;
            button.onClick.AddListener(() => controller.BuyShopOffer(capturedIndex));
        }
    }
}
