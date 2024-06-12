using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KittyHelpYouOut
{
    [AddComponentMenu("KittyHelpYouOut/AttachToWheelCollider")]
    public class AttachToWheelCollider : MonoBehaviour
	{
        #region Variable
        public WheelCollider Target;
        private Vector3 Pos;
        private Quaternion Rot;
        #endregion


        private void Update()
        {
            Target.GetWorldPose(out Pos, out Rot);
            this.transform.position = Pos;
        }
    }
}