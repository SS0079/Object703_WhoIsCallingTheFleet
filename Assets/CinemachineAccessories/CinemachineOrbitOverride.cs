using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace CinemachineAccessories
{
    [ExecuteAlways]
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineOrbitOverride : CinemachineExtension
    {
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
        [Tooltip("Vertical angle from camera forward to follow target backward")]
        [Range(2,89)]
        [SerializeField]
        private float cameraLatitude=60;
        public float CameraLatitude
        {
            get
            {
                return cameraLatitude;
            }
            set
            {
                if (allowRotate)
                {
                    cameraLatitude = Mathf.Clamp(value, 2, 89);
                }
            }
        }
        [Tooltip("Horizontal angle from camera forward to follow target backward")]
        [SerializeField]
        private float cameraLongitude = 0;
        public float CameraLongitude
        {
            get
            {
                return cameraLongitude;
            }
            set
            {
                if (allowRotate)
                {
                    cameraLongitude = value;
                }
            }
        }
        
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            //if follow is invalid,return
            if (vcam.Follow==null) return;
            //apply after body
            if (stage==CinemachineCore.Stage.Body)
            {
                var hq = Quaternion.AngleAxis(cameraLongitude, vcam.Follow.up);
                var vq = Quaternion.AngleAxis(cameraLatitude, vcam.Follow.right);
                var camDir = hq*vq*-vcam.Follow.forward;
                var camOffset = camDir * distance;
                state.RawPosition = camOffset+vcam.Follow.position;

            }
        }

    }
}