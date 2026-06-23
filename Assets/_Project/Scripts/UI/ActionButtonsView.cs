using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ActionButtonsView : MonoBehaviour
    {
        private Text rotateLabel;
        private Button discardButton;
        private Button resetButton;
        private Button validateButton;

        public void Initialize(GameFlowController controller)
        {
            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            discardButton = UiFactory.CreateButton(transform, "DiscardButton", "DISCARD");
            discardButton.GetComponent<LayoutElement>().preferredWidth = 130f;
            discardButton.GetComponent<LayoutElement>().preferredHeight = 52f;
            UiFactory.StyleButton(discardButton, new Color(0.18f, 0.08f, 0.06f), DomiNoxTheme.MultRed, 52f);
            discardButton.onClick.AddListener(controller.DiscardSelectedDominoes);

            resetButton = UiFactory.CreateButton(transform, "ResetButton", "RESET");
            resetButton.GetComponent<LayoutElement>().preferredWidth = 100f;
            resetButton.GetComponent<LayoutElement>().preferredHeight = 52f;
            UiFactory.StyleButton(resetButton, DomiNoxTheme.BgCard, DomiNoxTheme.TextSecondary, 52f);
            resetButton.onClick.AddListener(controller.ResetPlacements);

            validateButton = UiFactory.CreateButton(transform, "ValidateButton", "PLAY HAND");
            validateButton.GetComponent<LayoutElement>().preferredWidth = 180f;
            validateButton.GetComponent<LayoutElement>().preferredHeight = 52f;
            UiFactory.StyleButton(validateButton, new Color(0.06f, 0.22f, 0.10f), DomiNoxTheme.Success, 52f);
            var validateLabel = validateButton.GetComponentInChildren<Text>();
            if (validateLabel != null) validateLabel.fontSize = DomiNoxTheme.FontLG;
            validateButton.onClick.AddListener(controller.ValidateScore);

            rotateLabel = null;
        }

        public void Render(GameFlowController controller)
        {
            if (rotateLabel != null)
                rotateLabel.text = $"A/E: {controller.CurrentOrientation}";

            var interactable = !controller.IsScoring;
            discardButton.interactable = interactable;
            resetButton.interactable = interactable;
            validateButton.interactable = interactable;
        }
    }
}
