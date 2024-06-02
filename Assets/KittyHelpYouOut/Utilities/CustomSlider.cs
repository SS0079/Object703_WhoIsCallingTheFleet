using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace KittyHelpYouOut
{
    [AddComponentMenu("KittyHelpYouOut/CustomSlider")]
	public class CustomSlider:Slider
	{
        public Action<float> OnDragAction;
        public Action<float> OnPointerDownAction;
        public Action<float> OnPointerUpAction;
        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            OnDragAction?.Invoke(value);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            OnPointerUpAction?.Invoke(value);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnPointerDownAction?.Invoke(value);
        }
    }
}