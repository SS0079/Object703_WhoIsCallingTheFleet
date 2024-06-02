using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KittyHelpYouOut.Utilities
{
    [AddComponentMenu("KittyHelpYouOut/CustomToggle")]
    public class CustomToggle : Toggle
    {
        public Action<bool> OnClickAction;
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnClickAction?.Invoke(isOn);
        }
    }
}