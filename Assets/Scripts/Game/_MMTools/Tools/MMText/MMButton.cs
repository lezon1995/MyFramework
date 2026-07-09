using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    public class MMButton : Button
    {
        public Action<PointerEventData> onEnter { get; set; }
        public Action<PointerEventData> onExit { get; set; }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            onEnter?.Invoke(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onExit?.Invoke(eventData);
        }
    }
}