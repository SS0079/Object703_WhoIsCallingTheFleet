using KittyHelpYouOut;
using UnityEngine;

namespace Object703.Test
{
    public class TestDrawTrack : MonoBehaviour
    {
        [SerializeField]
        private Transform[] children;
        
        private void Update()
        {
            this.transform.Rotate(0,30*Time.deltaTime,0);
            if (Camera.main != null)
            {
                var mainTransform = Camera.main.transform;
                for (int i = 0; i < children.Length; i++)
                {
                    KittyDebug.DrawTrack(children[i].position,mainTransform,1f,KittyDebug.TrackShape.Cross,Color.green);
                    KittyDebug.DrawTrack(children[i].position,mainTransform,1f,KittyDebug.TrackShape.Box,Color.green);
                }
            }
        }
    }
}