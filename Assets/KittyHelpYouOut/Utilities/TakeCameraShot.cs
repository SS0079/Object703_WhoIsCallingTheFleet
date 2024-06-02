using System;
using System.IO;
using UnityEngine;

namespace KittyHelpYouOut
{
#if UNITY_EDITOR
    public class TakeCameraShot : MonoBehaviour
    {
        [SerializeField]
        private Camera _Camera;
        [SerializeField]
        private string _FileName;
        
        [ContextMenu("TakeShot")]
        public void TakeShotAndSaveToStreamingAssets()
        {
            var target = _Camera.targetTexture;
            if (target == null)
            {
                Debug.LogWarning($"Camera need a target texture to take the shot");
                return;
            }
            _Camera.Render();
            RenderTexture.active = target;
            Texture2D shot = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            var rect = new Rect(0, 0, target.width, target.height);
            shot.ReadPixels(rect,0,0);
            shot.Apply();
            byte[] bytes = shot.EncodeToPNG();
            try
            {
                File.WriteAllBytes($"{Application.streamingAssetsPath}/{_FileName}.png",bytes);
                Debug.Log($"Camera shot save successful");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
#endif
}