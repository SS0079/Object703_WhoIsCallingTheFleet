using UnityEngine;

namespace Object703.Test
{
    public class TestGetChild : MonoBehaviour
    {
        [ContextMenu("Log")]
        private void Log()
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                Debug.Log(this.transform.GetChild(i).name);
            }
        }
    }
}