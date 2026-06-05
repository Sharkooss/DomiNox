using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ActionButtonsView : MonoBehaviour
    {
        private Button rotateButton;
        private Text rotateLabel;

        public void Initialize(GameFlowController controller)
        {
            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;

            rotateButton = UiFactory.CreateButton(transform, "RotateButton", "E: tourner droite");
            rotateButton.onClick.AddListener(controller.ToggleOrientation);
            rotateLabel = rotateButton.GetComponentInChildren<Text>();

            UiFactory.CreateButton(transform, "DiscardButton", "Discard selection").onClick.AddListener(controller.DiscardSelectedDominoes);
            UiFactory.CreateButton(transform, "ResetButton", "Reset placement").onClick.AddListener(controller.ResetPlacements);
            UiFactory.CreateButton(transform, "ValidateButton", "Valider").onClick.AddListener(controller.ValidateScore);
        }

        public void Render(GameFlowController controller)
        {
            rotateLabel.text = $"A/E rotation: {controller.CurrentOrientation}";
        }
    }
}
