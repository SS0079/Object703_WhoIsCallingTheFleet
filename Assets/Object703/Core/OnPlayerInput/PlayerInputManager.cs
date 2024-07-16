using System;
using KittyHelpYouOut;
using Unity.Mathematics;
using UnityEngine;

namespace Object703.Core.OnPlayerInput
{
    public class PlayerInputManager : KittyMonoSingletonManual<PlayerInputManager>
    {
        public float forwardBackward;
        public float leftRight;
        public float turn;
        public bool mouseLeftButton;
        public float2 mouseDelta;
        public float mouseScroll;
        public bool skill0, skill1, skill2, skill3;
        private void Update()
        {
            //reset control every frame
            skill0 = false;
            skill1 = false;
            skill2 = false;
            skill3 = false;
            forwardBackward = 0;
            leftRight = 0;
            turn = 0;
            mouseDelta=float2.zero;
            mouseScroll = 0;

            //================================================================================

            if (Input.GetKey(KeyCode.W))
            {
                forwardBackward = 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                forwardBackward = -1;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                leftRight = -1;
            }
            if (Input.GetKey(KeyCode.E))
            {
                leftRight = 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                turn = -1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                turn = 1;
            }
            mouseLeftButton = Input.GetMouseButtonUp(0);
            mouseDelta = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            mouseScroll = Input.GetAxis("Mouse ScrollWheel");
            // if (Input.GetKeyUp(KeyCode.F))
            // {
            // }
            if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                skill0 = true;
            }
            if (Input.GetKeyUp(KeyCode.Alpha2))
            {
                skill1 = true;
            }
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                skill2 = true;
            }
            if (Input.GetKeyUp(KeyCode.Alpha4))
            {
                skill3 = true;
            }
        }
    }
    
    public enum SkillSlot
    {
        Skill0,
        Skill1,
        Skill2,
        Skill3,
    }

    // public static class ControlBitExtension
    // {
    //     public static bool Check(this ushort bit, PlayerControlBit mask)
    //     {
    //         return (bit & (ushort)mask) > 0;
    //     } 
    // }
}