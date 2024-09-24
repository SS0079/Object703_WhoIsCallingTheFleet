using System.Collections.Generic;
using KittyHelpYouOut;
using UnityEngine;

namespace Object703.Test
{
    public class TestDrawTrack : MonoBehaviour
    {
        [SerializeField]
        private Transform[] children;
        private List<ushort> trackKey = new();
        private void Start()
        {
            if (Camera.main != null)
            {
                var mainTransform = Camera.main.transform;
                for (int i = 0; i < children.Length; i++)
                {
                    var key = KittyDebug.Instance.AddTrackIcon(children[i],mainTransform,1f,KittyDebugIcon.Sqare | KittyDebugIcon.Diamond,Color.green);
                    trackKey.Add(key);
                    key = KittyDebug.Instance.AddTrackIcon(children[i],mainTransform,2f,KittyDebugIcon.Cross,Color.red);
                    trackKey.Add(key);
                }
            }
        }

        private void Update()
        {
            this.transform.Rotate(0,30*Time.deltaTime,0);
            if (Input.GetKeyDown(KeyCode.P))
            {
                for (int i = 0; i < trackKey.Count; i++)
                {
                    KittyDebug.Instance.RemoveTrackIcon(trackKey[i]);
                }
            }
        }
    }
}