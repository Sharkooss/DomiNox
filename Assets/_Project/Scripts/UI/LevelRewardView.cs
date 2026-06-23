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
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.09f, 0.07f, 0.98f);
            DomiNoxTheme.AddOutline(gameObject, DomiNoxTheme.Success, 4f);
            DomiNoxTheme.AddShadow(gameObject, new Color(0.1f, 0.8f, 0.3f, 0.2f), new Vector2(0f, -4f));

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(28, 28, 24, 28);
            layout.spacing = 13f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;

            title = UiFactory.CreateText(transform, "Title", string.Empty, DomiNoxTheme.FontXL, TextAnchor.MiddleCenter);
            title.color = DomiNoxTheme.Gold;
            title.fontStyle = FontStyle.Bold;
            DomiNoxTheme.AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(1f, -1f));
            title.GetComponent<LayoutElement>().preferredHeight = 44f;

            targetText = UiFactory.CreateText(transform, "Target", string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleCenter);
            targetText.color = DomiNoxTheme.TextSecondary;
            targetText.GetComponent<LayoutElement>().preferredHeight = 48f;

            UiFactory.CreateSeparator(transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.Success, 0.45f));

            baseCreditsText     = CreateReceiptLine("BaseCredits");
            discardCreditsText  = CreateReceiptLine("DiscardCredits");
            interestCreditsText = CreateReceiptLine("InterestCredits");

            UiFactory.CreateSeparator(transform, 1f, DomiNoxTheme.WithAlpha(DomiNoxTheme.Success, 0.45f));

            totalText = UiFactory.CreateText(transform, "Total", string.Empty, DomiNoxTheme.FontXL, TextAnchor.MiddleCenter);
            totalText.color = DomiNoxTheme.Gold;
            totalText.fontStyle = FontStyle.Bold;
            totalText.GetComponent<LayoutElement>().preferredHeight = 44f;

            var cashOut = UiFactory.CreateButton(transform, "CashOutButton", "Cash Out  →");
            cashOut.GetComponent<LayoutElement>().preferredWidth = 280f;
            cashOut.GetComponent<LayoutElement>().preferredHeight = 58f;
            UiFactory.StyleButton(cashOut, new Color(0.55f, 0.38f, 0.04f), DomiNoxTheme.Gold, 58f);
            var cashLabel = cashOut.GetComponentInChildren<Text>();
            if (cashLabel != null) cashLabel.fontSize = DomiNoxTheme.FontLG;
            cashOut.onClick.AddListener(controller.CashOutReward);
        }

        public void Render(RunState run)
        {
            var reward = run.CurrentReward;
            if (reward == null) return;
            title.text = $"Level Clear!  +${reward.TotalCredits}";
            targetText.text = $"Score {run.CurrentLevel.CurrentScore} / {run.CurrentLevel.Quota}";
            baseCreditsText.text     = $"Level win                          +${reward.BaseCredits}";
            discardCreditsText.text  = $"Discards left ({run.CurrentLevel.DiscardsRemaining} × $1)     +${reward.DiscardCredits}";
            interestCreditsText.text = $"Interest on credits                +${reward.InterestCredits}";
            totalText.text = $"Total  +${reward.TotalCredits}";
        }

        private Text CreateReceiptLine(string name)
        {
            var text = UiFactory.CreateText(transform, name, string.Empty, DomiNoxTheme.FontMD, TextAnchor.MiddleLeft);
            text.color = DomiNoxTheme.TextPrimary;
            text.GetComponent<LayoutElement>().preferredHeight = 28f;
            return text;
        }
    }
}
