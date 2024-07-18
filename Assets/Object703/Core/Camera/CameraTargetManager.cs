using KittyHelpYouOut;
using QFramework;
using UnityEngine;

namespace Object703.Core
{
    public class CameraTargetManager : KittyMonoSingletonManual<CameraTargetManager>
    {
        [SerializeField]
        private GameObject cameraTargetPrefab;
        private GameObject target;

        private void OnEnable()
        {
            target = Instantiate(cameraTargetPrefab);
        }

        public Transform GetTarget()
        {
            return target.transform;
        }

        private void OnDisable()
        {
            target.DestroySelf();
            target = null;
        }
    }
}