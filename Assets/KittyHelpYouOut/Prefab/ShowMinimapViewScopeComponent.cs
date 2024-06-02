using KittyHelpYouOut;
using UnityEngine;

namespace KittyHelpYouOut
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class ShowMinimapViewScopeComponent : MonoBehaviour
    {
         private LineRenderer _Line;
        public LineRenderer Line
        {
            get
            {
                _Line = _Line == null ? this.GetComponent<LineRenderer>() : _Line;
                return _Line;
            }
        }
        private Vector3 ViewUpRight;
        private Vector3 ViewUpLeft;
        private Vector3 ViewDownRight;
        private Vector3 ViewDownLeft;
        [SerializeField] private Camera ViewCamera;
        [SerializeField] private float AboveGroundDistance=2;
        [SerializeField]
        private float _Infinity=500;
        
        [Space]
        public bool ShowUpRight;
        public bool ShowUpLeft;
        public bool ShowDownRight;
        public bool ShowDownLeft;
        private Vector3 ViewCameraPos;
        //================================================================================
        private void Update()
        {
            if (ViewCamera == null) return;
            ViewCameraPos = ViewCamera.transform.position;
            if (ViewCamera!=null)
            {
                ViewDownLeft = ViewPointCastOnMap(ViewCameraPos,new Vector2(0, 0),AboveGroundDistance,_Infinity);
                ViewUpLeft = ViewPointCastOnMap(ViewCameraPos,new Vector2(0, ViewCamera.pixelHeight),AboveGroundDistance,_Infinity);
                ViewUpRight = ViewPointCastOnMap(ViewCameraPos,new Vector2(ViewCamera.pixelWidth, ViewCamera.pixelHeight),AboveGroundDistance,_Infinity);
                ViewDownRight = ViewPointCastOnMap(ViewCameraPos,new Vector2(ViewCamera.pixelWidth, 0),AboveGroundDistance,_Infinity);
                Line.positionCount = 4;
                Line.SetPosition(0, ViewDownLeft);
                Line.SetPosition(1, ViewUpLeft);
                Line.SetPosition(2, ViewUpRight);
                Line.SetPosition(3, ViewDownRight);
            }
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if(ShowDownLeft) Gizmos.DrawRay(ViewCameraPos,ViewDownLeft-ViewCameraPos);
            if (ShowUpLeft) Gizmos.DrawRay(ViewCameraPos, ViewUpLeft-ViewCameraPos);
            if (ShowUpRight) Gizmos.DrawRay(ViewCameraPos, ViewUpRight-ViewCameraPos);
            if (ShowDownRight) Gizmos.DrawRay(ViewCameraPos, ViewDownRight-ViewCameraPos);
        }
#endif

        
        //================================================================================
        /// <summary>
        /// Convert point on map in camera view to world point
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector3 ViewPointCastOnMap(Vector3 cameraPos,Vector2 point,float planeHeight,float infinity=500)
        {
            
            Vector3 intersection;
            
            //convert point to world point
            Vector3 worldPoint = ViewCamera.ScreenToWorldPoint(new Vector3(point.x, point.y,1));
            //calculate the vector from camera position to world point
            Vector3 pointVector = Vector3.Normalize(worldPoint - cameraPos);
            //calculate the intersection that the vector hit the map
            float magnitude = (planeHeight-Vector3.Dot(cameraPos, Vector3.up)) / Vector3.Dot(pointVector, Vector3.up);
            if (magnitude>0)
            {
                intersection = cameraPos + pointVector * magnitude;
            }
            else
            {
                intersection = Vector3.ProjectOnPlane(cameraPos + pointVector*infinity, Vector3.up);
            }
            

            return intersection;
        }
    }
}