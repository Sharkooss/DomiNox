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
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            discardButton = UiFactory.CreateButton(transform, "DiscardButton", "Defausser");
            discardButton.onClick.AddListener(controller.DiscardSelectedDominoes);
            resetButton = UiFactory.CreateButton(transform, "ResetButton", "Reset");
            resetButton.onClick.AddListener(controller.ResetPlacements);
            validateButton = UiFactory.CreateButton(transform, "ValidateButton", "Valider");
            validateButton.GetComponent<Image>().color = new Color(0.14f, 0.48f, 0.28f);
            validateButton.onClick.AddListener(controller.ValidateScore);

            rotateLabel = null;
        }

        public void Render(GameFlowController controller)
        {
            if (rotateLabel != null)
            {
                rotateLabel.text = $"A/E rotation: {controller.CurrentOrientation}";
            }

            var interactable = !controller.IsScoring;
            discardButton.interactable = interactable;
            resetButton.interactable = interactable;
            validateButton.interactable = interactable;
        }
    }
}
