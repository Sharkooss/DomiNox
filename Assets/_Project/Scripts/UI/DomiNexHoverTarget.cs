using System;
using DomiNox.Dominex;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DomiNox.UI
{
    public sealed class DomiNexHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private DomiNexDefinition definition;
        private Action<DomiNexDefinition> entered;
        private Action<DomiNexDefinition, RectTransform> enteredWithRect;
        private Action exited;

        public void Initialize(DomiNexDefinition domiNex, Action<DomiNexDefinition> onEntered, Action onExited)
        {
            definition = domiNex;
            entered = onEntered;
            exited = onExited;
        }

        public void Initialize(DomiNexDefinition domiNex, Action<DomiNexDefinition, RectTransform> onEntered, Action onExited)
        {
            definition = domiNex;
            enteredWithRect = onEntered;
            exited = onExited;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            entered?.Invoke(definition);
            enteredWithRect?.Invoke(definition, (RectTransform)transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            exited?.Invoke();
        }
    }
}
