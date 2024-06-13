using System.Collections;
using KittyHelpYouOut.ServiceClass;
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
        public KittyBindDictionary<string, int> testSerializeDic;

        private void Awake()
        {
            testSerializeDic = new(3);
            // testSerializeDic.Add("a",0);
            // testSerializeDic.Add("b",1);
            // testSerializeDic.Add("c",2);
            ResKit.Init();
            Application.targetFrameRate = fixFramerate;
            Application.runInBackground = true;
            SceneManager.LoadScene("Frontend", LoadSceneMode.Additive);
            
        }

    
    }
}
