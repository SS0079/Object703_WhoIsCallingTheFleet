using KittyHelpYouOut;
using UnityEngine;
using UnityEngine.Serialization;

namespace KittyHelpYouOut.Utilities
{
    public class FaceCamera : MonoBehaviour
    {
        private enum UpdateState
        {
            Update,
            LateUpdate,
            FixUpdate,
            ManualUpdate
        }
        [FormerlySerializedAs("vCam")]
        public Transform cam;
        [SerializeField]
        private UpdateState stage;
        private void Update()
        {
            if (stage==UpdateState.Update)
            {
                ManualUpdate();
            }
        }

        private void LateUpdate()
        {
            if (stage==UpdateState.LateUpdate)
            {
                ManualUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (stage==UpdateState.FixUpdate)
            {
                ManualUpdate();
            }
        }

        public void ManualUpdate()
        {
            this.transform.rotation=Quaternion.LookRotation(-cam.forward,Vector3.up);
        }
    }
}