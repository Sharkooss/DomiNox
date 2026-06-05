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
        private Action exited;

        public void Initialize(DomiNexDefinition domiNex, Action<DomiNexDefinition> onEntered, Action onExited)
        {
            definition = domiNex;
            entered = onEntered;
            exited = onExited;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            entered?.Invoke(definition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            exited?.Invoke();
        }
    }
}
