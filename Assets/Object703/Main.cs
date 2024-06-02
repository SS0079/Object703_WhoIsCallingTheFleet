using Object703.UI;
using QFramework;
using UnityEngine;
using UnityEngine.Serialization;

namespace Object703
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private int fixFramerate=60;

        private void Awake()
        {
            ResKit.Init();
        }

        private void Start()
        {
            Application.targetFrameRate = fixFramerate;
            Application.runInBackground = true;
            UIKit.OpenPanel<FrontendNetworkPanel>();
        }
    }
}
