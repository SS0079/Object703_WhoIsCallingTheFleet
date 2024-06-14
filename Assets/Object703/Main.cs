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
        public KittyBindDictionary<string, int> testSerializeDic=new(3);
        public List<int> testIntlist;

        private void Awake()
        {
            testSerializeDic.Add("a",0);
            testSerializeDic.Add("b",1);
            testSerializeDic.Add("c",2);
            testSerializeDic.Remove("b");
            testSerializeDic.Add("d",10);
            testSerializeDic.Remove("a");
            testSerializeDic.Remove("c");
            testSerializeDic.Add("x",11);
            testSerializeDic.Add("y",12);
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
