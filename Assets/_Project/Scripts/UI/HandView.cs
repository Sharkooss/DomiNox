using System.Collections.Generic;
using System;
using System.Linq;
using DomiNox.Dominoes;
using DomiNox.Grid;
using DomiNox.Run;
using UnityEngine;
using UnityEngine.UI;

namespace DomiNox.UI
{
    public sealed class HandView : MonoBehaviour
    {
        private readonly List<DominoView> views = new List<DominoView>();
        private GameFlowController controller;
        private Action<Vector2> dragged;
        private Action<Vector2> dragEnded;
        private Action<DominoView> dragViewStarted;

        public void Initialize(GameFlowController flowController, Action<DominoView> onDragViewStarted, Action<Vector2> onDragged, Action<Vector2> onDragEnded)
        {
            controller = flowController;
            dragViewStarted = onDragViewStarted;
            dragged = onDragged;
            dragEnded = onDragEnded;
            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
        }

        public void Render(HandState hand, DominoInstance selected, DominoOrientation selectedOrientation, IReadOnlyCollection<DominoInstance> selectedForDiscard)
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
                layout.preferredWidth = domino == selected && !GridState.IsHorizontal(selectedOrientation) ? 48f : 76f;
                layout.preferredHeight = domino == selected && !GridState.IsHorizontal(selectedOrientation) ? 76f : 48f;
                var view = go.AddComponent<DominoView>();
                view.Initialize(domino, controller.SelectDomino, BeginDragDomino, dragged, dragEnded);
                view.SetOrientation(domino == selected ? selectedOrientation : DominoOrientation.HorizontalRight);
                view.SetSelected(domino == selected || selectedForDiscard.Contains(domino));
                views.Add(view);
            }
        }

        private void BeginDragDomino(DominoInstance domino, DominoView view)
        {
            controller.BeginDragDomino(domino);
            dragViewStarted?.Invoke(view);
        }
    }
}
