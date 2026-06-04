using System.Collections.Generic;
using DomiNox.Dominoes;
using DomiNox.Run;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class HandView : MonoBehaviour
    {
        private readonly List<DominoView> views = new List<DominoView>();
        private GameFlowController controller;

        public void Initialize(GameFlowController flowController)
        {
            controller = flowController;
            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
        }

        public void Render(HandState hand, DominoInstance selected)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            views.Clear();

            foreach (var domino in hand.Dominoes)
            {
                var go = new GameObject($"Domino_{domino.InstanceId}", typeof(RectTransform), typeof(LayoutElement));
                go.transform.SetParent(transform, false);
                var layout = go.GetComponent<LayoutElement>();
                layout.preferredWidth = 76f;
                layout.preferredHeight = 48f;
                var view = go.AddComponent<DominoView>();
                view.Initialize(domino, controller.SelectDomino);
                view.SetSelected(domino == selected);
                views.Add(view);
            }
        }
    }
}
