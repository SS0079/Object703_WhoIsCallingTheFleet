using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace KittyHelpYouOut.Utilities
{
    [ExecuteAlways]
    public class Orbiter : MonoBehaviour
    {
        private enum UpdateStage
        {
            Update,
            LateUpdate,
            FixUpdate,
            ManualUpdate
        }
        [SerializeField]
        private Transform mover;
        [SerializeField]
        private Transform target;
        [SerializeField]
        private UpdateStage stage;
        
        public UnityEvent afterUpdate;
        
        
        [Tooltip("Weather the orbiter can rotate")]
        public bool allowRotate = true;
        [Tooltip("Weather the orbiter can zoom")]
        public bool allowZoom = true;
        [Tooltip("Distance to follow target")]
        [SerializeField]
        private float distance=10;
        public float Distance
        {
            get
            {
                return distance;
            }
            set
            {
                if (allowZoom)
                {
                    distance = value;
                }
            }
        }
        [FormerlySerializedAs("cameraLatitude")]
        [Tooltip("Vertical angle from camera forward to follow target backward")]
        [Range(2,89)]
        [SerializeField]
        private float latitude=60;
        public float Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                if (allowRotate)
                {
                    latitude = Mathf.Clamp(value, 2, 89);
                }
            }
        }
        [FormerlySerializedAs("cameraLongitude")]
        [Tooltip("Horizontal angle from camera forward to follow target backward")]
        [SerializeField]
        private float longitude = 0;
        public float Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                if (allowRotate)
                {
                    longitude = value;
                }
            }
        }

        private void Update()
        {
            if (stage==UpdateStage.Update)
            {
                ManualUpdate();
            }
        }

        private void LateUpdate()
        {
            if (stage==UpdateStage.LateUpdate)
            {
                ManualUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (stage==UpdateStage.FixUpdate)
            {
                ManualUpdate();
            }
        }

        public void ManualUpdate()
        {
            var hq = Quaternion.AngleAxis(longitude, target.up);
            var vq = Quaternion.AngleAxis(latitude, target.right);
            var camDir = hq*vq*-target.forward;
            var camOffset = camDir * distance;
            mover.position = camOffset+target.position;
            mover.LookAt(target);
            afterUpdate?.Invoke();
        }
    }
}