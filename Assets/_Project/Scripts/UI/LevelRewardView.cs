using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class LevelRewardView : MonoBehaviour
    {
        private GameFlowController controller;
        private Text title;
        private Text targetText;
        private Text baseCreditsText;
        private Text discardCreditsText;
        private Text interestCreditsText;
        private Text totalText;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.04f, 0.07f, 0.08f, 0.98f);
            var outline = gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.22f, 0.5f, 0.38f);
            outline.effectDistance = new Vector2(4f, -4f);

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 20, 24);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", string.Empty, 36, TextAnchor.MiddleCenter);
            title.color = new Color(1f, 0.72f, 0.15f);
            title.GetComponent<LayoutElement>().preferredHeight = 62f;
            targetText = UiFactory.CreateText(transform, "Target", string.Empty, 18, TextAnchor.MiddleCenter);
            targetText.GetComponent<LayoutElement>().preferredHeight = 48f;

            CreateDivider();
            baseCreditsText = CreateReceiptLine("BaseCredits");
            discardCreditsText = CreateReceiptLine("DiscardCredits");
            interestCreditsText = CreateReceiptLine("InterestCredits");
            CreateDivider();
            totalText = UiFactory.CreateText(transform, "Total", string.Empty, 24, TextAnchor.MiddleCenter);
            totalText.color = new Color(1f, 0.82f, 0.22f);
            totalText.GetComponent<LayoutElement>().preferredHeight = 42f;

            var cashOut = UiFactory.CreateButton(transform, "CashOutButton", "Cash Out");
            cashOut.GetComponent<Image>().color = new Color(0.95f, 0.52f, 0.05f);
            cashOut.GetComponent<LayoutElement>().preferredWidth = 260f;
            cashOut.GetComponent<LayoutElement>().preferredHeight = 58f;
            cashOut.onClick.AddListener(controller.CashOutReward);
        }

        public void Render(RunState run)
        {
            var reward = run.CurrentReward;
            if (reward == null)
            {
                return;
            }

            title.text = $"Cash Out: ${reward.TotalCredits}";
            targetText.text = $"Score at least {run.CurrentLevel.Quota}\nScore: {run.CurrentLevel.CurrentScore}";
            baseCreditsText.text = $"Niveau gagne                         ${reward.BaseCredits}";
            discardCreditsText.text = $"Discards restants ({run.CurrentLevel.DiscardsRemaining} x $1)     ${reward.DiscardCredits}";
            interestCreditsText.text = $"Interets credits                     ${reward.InterestCredits}";
            totalText.text = $"Total: ${reward.TotalCredits}";
        }

        private Text CreateReceiptLine(string name)
        {
            var text = UiFactory.CreateText(transform, name, string.Empty, 17, TextAnchor.MiddleLeft);
            text.color = new Color(0.88f, 0.94f, 0.9f);
            text.GetComponent<LayoutElement>().preferredHeight = 30f;
            return text;
        }

        private void CreateDivider()
        {
            var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            divider.transform.SetParent(transform, false);
            divider.GetComponent<Image>().color = new Color(0.55f, 0.72f, 0.64f, 0.85f);
            divider.GetComponent<LayoutElement>().preferredHeight = 2f;
        }
    }
}
