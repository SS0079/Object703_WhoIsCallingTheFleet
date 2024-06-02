using UnityEngine;

namespace KittyHelpYouOut
{
    public static class MainCameraUtility
    {
        private static Camera _MainCamera;

        public static Camera MainCamera
        {
            get
            {
                _MainCamera ??= Camera.main;
                return _MainCamera;
            }
        }
        
        public static int MousePointRaycast(float hitDistance, RaycastHit[] result, LayerMask layerMask)
        {
            var mouseRay = MainCamera.ScreenPointToRay(Input.mousePosition);
            var size = Physics.RaycastNonAlloc(mouseRay,result, hitDistance, layerMask);
            return size;
        }

    }
}