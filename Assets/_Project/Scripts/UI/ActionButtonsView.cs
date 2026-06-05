using DomiNox.Run;
using DomiNox.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class ActionButtonsView : MonoBehaviour
    {
        private Text rotateLabel;

        public void Initialize(GameFlowController controller)
        {
            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            UiFactory.CreateButton(transform, "DiscardButton", "Defausser").onClick.AddListener(controller.DiscardSelectedDominoes);
            var draw = UiFactory.CreateButton(transform, "DrawButton", "Draw (-1 AP)");
            draw.interactable = false;
            UiFactory.CreateButton(transform, "ResetButton", "Reset").onClick.AddListener(controller.ResetPlacements);
            var validate = UiFactory.CreateButton(transform, "ValidateButton", "Valider");
            validate.GetComponent<Image>().color = new Color(0.14f, 0.48f, 0.28f);
            validate.onClick.AddListener(controller.ValidateScore);

            rotateLabel = null;
        }

        public void Render(GameFlowController controller)
        {
            if (rotateLabel != null)
            {
                rotateLabel.text = $"A/E rotation: {controller.CurrentOrientation}";
            }
        }
    }
}
