using System.Collections;
using System.Collections.Generic;
using KittyHelpYouOut;
using Object703.UI;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Object703
{
    public class Main : MonoBehaviour
    {
        [SerializeField]
        private int fixFramerate=60;
        public KittyBindDictionary<int, GameObject> testSerializeDic=new(3);

        private void Awake()
        {
            ResKit.Init();
            Application.targetFrameRate = fixFramerate;
            Application.runInBackground = true;
            SceneManager.LoadScene("Frontend", LoadSceneMode.Additive);
            
        }

        [ContextMenu("Log")]
        private void TestDrawer()
        {
            foreach (var item in testSerializeDic)
            {
                Debug.Log($"{item.Key} | {item.Value}");
            }
        }
    
    }
}
