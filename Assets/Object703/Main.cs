using System.Collections;
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


        private void Awake()
        {
            ResKit.Init();
            Application.targetFrameRate = fixFramerate;
            Application.runInBackground = true;
            SceneManager.LoadScene("Frontend", LoadSceneMode.Additive);
            
        }

    
    }
}
