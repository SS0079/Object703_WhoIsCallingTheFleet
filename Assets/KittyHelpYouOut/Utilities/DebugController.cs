using System;
using KittyHelpYouOut;
using UnityEngine;

namespace KittyHelpYouOut
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public enum PanelPosition
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }
    
    [ExecuteAlways]
    public class DebugController : MonoBehaviour
    {
        public bool ShowFPS;
        public PanelPosition Position; 
        public float FPSUpdateInterval=1f;
        private KittyTimer _FPSUpdateTimer=new KittyTimer(true);
        private float _FPS;
        private GUIStyle _LabelStyle;

        private void Start()
        {
            _FPSUpdateTimer.StartTimer(FPSUpdateInterval);
            _LabelStyle = new GUIStyle();
            _LabelStyle.fontSize = 30;
            _LabelStyle.alignment = TextAnchor.UpperRight;
            _LabelStyle.normal.textColor=Color.white;
            _LabelStyle.margin = new RectOffset(2, 2, 2, 2);
        }

        private void Update()
        {
            var Δt = Time.deltaTime;
            if (ShowFPS)
            {
                if (_FPSUpdateTimer.Tick(Δt))
                {
                    _FPS = 1f / Δt;
                    _FPSUpdateTimer.StartTimer(FPSUpdateInterval);
                }
            }
        }

        private void OnGUI()
        {
            var rect = new Rect();
            switch (Position)
            {
                case PanelPosition.UpperLeft:
                    rect = new Rect(0, 0, 300, 200);
                    _LabelStyle.alignment = TextAnchor.UpperLeft;
                    break;
                case PanelPosition.UpperRight:
                    rect = new Rect(Screen.width - 300, 0, 300, 200);
                    _LabelStyle.alignment = TextAnchor.UpperRight;
                    break;
                case PanelPosition.LowerLeft:
                    rect = new Rect(0, Screen.height-50, 300, 200);
                    _LabelStyle.alignment = TextAnchor.LowerLeft;
                    break;
                case PanelPosition.LowerRight:
                    rect = new Rect(Screen.width - 300, Screen.height-50, 300, 200);
                    _LabelStyle.alignment = TextAnchor.LowerRight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            GUILayout.BeginArea(rect);
            if (ShowFPS)
            {
                
                GUILayout.Label($"帧数:{_FPS:F0}",_LabelStyle);
            }
            GUILayout.EndArea();
        }
    }
#endif
}