using System.Collections;
using Object703.UI;
using QFramework;
using UnityEngine;

namespace Object703.Utility
{
    public class FrontendUIStarter : MonoBehaviour
    {
        private void Awake()
        {
            StartCoroutine(OpenPanels());
        }

        private IEnumerator OpenPanels()
        {
            yield return UIKit.OpenPanelAsync<FrontendNetworkPanel>();
            Debug.Log($"PanelShowed");
        }
    }
}