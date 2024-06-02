using System;
using UnityEngine;

namespace KittyHelpYouOut
{
    [Serializable]
    public struct WheelPair
    {
        public WheelCollider Left;
        public WheelCollider Right;
    }
    public class CarPhysicsAntiRoll : MonoBehaviour
    {
        public float Force;
        public WheelPair[] WheelPairs;
        [SerializeField]
        private Rigidbody _CarRigidbody;
        private Rigidbody CarRigidbody
        {
            get
            {
                _CarRigidbody ??= this.GetComponent<Rigidbody>();
                if(_CarRigidbody==null) Debug.LogWarning($"{this.gameObject.name} rigidbody not set in anti roll script");
                return _CarRigidbody;
            }
        }
                 
                 

        private void FixedUpdate()
        {
            ApplyAntiRollForce();
        }

        /// <summary>
        /// apply anti roll force for every wheel pairs in array
        /// </summary>
        private void ApplyAntiRollForce()
        {
            WheelHit hit;
            float travelRatioLeft=0;
            float travelRatioRight=0;
            for (int i = 0; i < WheelPairs.Length; i++)
            {
                var curLeftWheel = WheelPairs[i].Left;
                var curRightWheel = WheelPairs[i].Right;
                var groundedLeft = curLeftWheel.GetGroundHit(out hit);
                if (groundedLeft)
                    //calculate suspension travel ratio
                    travelRatioLeft = (-curLeftWheel.transform.InverseTransformPoint(hit.point).y - curLeftWheel.radius) / curLeftWheel.suspensionDistance;
                var groundedRight = curRightWheel.GetGroundHit(out hit);
                if (groundedRight)
                    //calculate suspension travel ratio
                    travelRatioRight = (-curRightWheel.transform.InverseTransformPoint(hit.point).y - curRightWheel.radius) / curRightWheel.suspensionDistance;
                var antiRollForce = (travelRatioLeft - travelRatioRight) * Force;
                //apply force to each side of wheel
                if (groundedLeft)
                    CarRigidbody.AddForceAtPosition(curLeftWheel.transform.up * -antiRollForce, curLeftWheel.transform.position);  
                if (groundedLeft)
                    CarRigidbody.AddForceAtPosition(curRightWheel.transform.up * antiRollForce, curRightWheel.transform.position);  
            }
        }
    }
}