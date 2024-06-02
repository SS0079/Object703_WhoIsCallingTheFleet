using System;
using KittyHelpYouOut.ServiceClass;
using KittyHelpYouOut.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace KittyHelpYouOut
{
    [AddComponentMenu("KittyHelpYouOut/CruiseMove")]
	public class CruiseMove:MonoBehaviour
    {
        public Transform mover;
        public bool active=true;
        public float cruiseSpeed;
        public float zoomSpeed;
        public Vector2 zoomHeightRestriction=new(10,100);
        public Action onMove;

        private void Start()
        {
            KittyEvent.Register<KittyInputEvent>(HandleInput);
        }


        private void HandleInput(KittyInputEvent e)
        {
            if (mover!=null)
            {
                if (CheckCanMove())
                {
                    switch (e.command)
                    {
                        case "forward":
                            mover.position += new Vector3(0, 0, cruiseSpeed * Time.deltaTime);
                            onMove?.Invoke();
                            break;
                        case "backward":
                            mover.position += new Vector3(0, 0, -cruiseSpeed * Time.deltaTime);
                            onMove?.Invoke();
                            break;
                        case "left":
                            mover.position += new Vector3(-cruiseSpeed * Time.deltaTime, 0, 0);
                            onMove?.Invoke();
                            break;
                        case "right":
                            mover.position += new Vector3(cruiseSpeed * Time.deltaTime, 0, 0);
                            onMove?.Invoke();
                            break;
                        case "zoomIn":
                        case "zoomOut":
                            var zoom = e.value * zoomSpeed;
                            var zoomVector = zoom * mover.forward;
                            if ((mover.position + zoomVector).y>zoomHeightRestriction.y || (mover.position+zoomVector).y<zoomHeightRestriction.x)
                            {
                                zoomVector=Vector3.zero;
                            }
                            mover.position += zoomVector;
                            break;
                        default:
                            break;
                    }
                    mover.position =new Vector3(mover.position.x, Mathf.Clamp(mover.position.y, zoomHeightRestriction.x, zoomHeightRestriction.y),this.transform.position.z);
                }
            }
            else
            {
                Debug.LogWarning("Mover is not set",this.gameObject);
            }
        }

        public virtual bool CheckCanMove()
        {
            return active;
        }
    }
}