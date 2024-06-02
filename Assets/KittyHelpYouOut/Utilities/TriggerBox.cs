using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace KittyHelpYouOut
{
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerBox : MonoBehaviour
    {
        public String[] TargetTag=new []{"Player"};
        public UnityEvent<GameObject> OnEnter;
        public UnityEvent<GameObject> OnExit;
        public AfterStayEvent[] AfterStay;
        private BoxCollider _BoxCollider;
        private BoxCollider BoxCollider
        {
            get
            {
                _BoxCollider ??= this.GetComponent<BoxCollider>();
                return _BoxCollider;
            }
        }
        private bool IsTrigger => BoxCollider.isTrigger;
                 
        
        private void Start()
        {
            for (int i = 0; i < AfterStay.Length; i++)
            {
                AfterStay[i].Timer = new KittyTimer(true);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool hit = TargetTag.Any(collision.collider.CompareTag);
            if (!hit) return;
            OnEnter.Invoke(collision.gameObject);
            foreach (var stay in AfterStay)
            {
                stay.Timer.StartTimer(stay.CountDown);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            bool hit = TargetTag.Any(other.CompareTag);
            if (!hit) return;
            OnEnter.Invoke(other.gameObject);
            foreach (var stay in AfterStay)
            {
                stay.Timer.StartTimer(stay.CountDown);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            bool hit = TargetTag.Any(other.CompareTag);
            if(!hit) return;
            OnExit.Invoke(other.gameObject);
        }

        private void OnTriggerStay(Collider other)
        {
            foreach (var stay in AfterStay)
            {
                if (stay.Timer.Tick(Time.deltaTime))
                {
                    stay.Event?.Invoke();
                    stay.Timer.Reset();
                }
            }
        }

    }
    [Serializable]
    public struct AfterStayEvent
    {
        [Tooltip("in second")]
        public float CountDown;
        public KittyTimer Timer;
        public UnityEvent Event;
    }
}