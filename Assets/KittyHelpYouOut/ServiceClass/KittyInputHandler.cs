using System;
using System.Collections.Generic;
using UnityEngine;

namespace KittyHelpYouOut.ServiceClass
{
    public class KittyInputHandler
    {
        private KittyInputHandler(){}

        public KittyInputHandler(string keyMapPath)
        {
            Init(keyMapPath);
        }
        
        private readonly Dictionary<KeyCode, string[]> keyboardCommandDic = new();
        private readonly Dictionary<string, string[]> mouseCommandDic = new();
        
        private void Init(string path)
        {
            var ini = new INIParser();
            ini.Open(path);
            var keyboardSec = ini.ReadWholeSection("keyboard");
            foreach (var item in keyboardSec)
            {
                var split = item.Value.Split(',');
                var keyCode = Enum.Parse<KeyCode>(item.Key);
                if (!keyboardCommandDic.TryAdd(keyCode, split))
                {
                    Debug.LogWarning($"same key map being added more than once : {keyCode}");
                }
            }
            var mouseSec = ini.ReadWholeSection("mouse");
            foreach (var item in mouseSec)
            {
                var split = item.Value.Split(',');
                if (!mouseCommandDic.TryAdd(item.Key, split))
                {
                    Debug.LogWarning($"same key map being added more than once : {item.Key}");
                }
            }
        }

        public void CheckInput(Action<KittyInputEvent> callback)
        {
            //gather input and make KittyInputEvent
            //gather keyboard input
            if (Input.anyKey)
            {
                foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKey(key))
                    {
                        if (keyboardCommandDic.TryGetValue(key,out string[] cmd))
                        {
                            for (int i = 0; i < cmd.Length; i++)
                            {
                                var localEvent = new KittyInputEvent
                                {
                                    command = cmd[i],
                                    value = 1
                                };
                                callback.Invoke(localEvent);
                            }
                        }
                    }
                }
            }

            //================================================================================

            if (Input.mouseScrollDelta==new Vector2(0,1))
            {
                Input2Event("scrollForward",1);
            }else if (Input.mouseScrollDelta==new Vector2(0,-1))
            {
                Input2Event("scrollBackward",-1);
            }
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");
            if (mouseX!=0)
            {
                Input2Event("mouseX",mouseX);
            }
            if (mouseY!=0)
            {
                Input2Event("mouseY",mouseY);
            }
            if (Input.GetMouseButton(1))
            {
                Input2Event("mouse1",1);
            }
            else
            {
                Input2Event("mouse1",0);
            }
            if (Input.GetMouseButton(0))
            {
                Input2Event("mouse0",1);
            }
            else
            {
                Input2Event("mouse0",0);
            }
            
            void Input2Event(string key,float value)
            {
                if (mouseCommandDic.TryGetValue(key,out string[] cmd))
                {
                    for (int i = 0; i < cmd.Length; i++)
                    {
                        var localEvent = new KittyInputEvent
                        {
                            command = cmd[i],
                            value = value
                        };
                        callback.Invoke(localEvent);
                    }
                }
            }
        }


    }
    
    public struct KittyInputEvent
    {

        public string command;
        public float value;

        public static bool operator ==(KittyInputEvent left, KittyInputEvent right)
        {
            return left.command.Equals(right.command) && Mathf.Abs(left.value-right.value)<0.001f;
        }

        public static bool operator !=(KittyInputEvent left, KittyInputEvent right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{command}:{value}";
        }

        public override bool Equals(object obj)
        {
            return obj != null && this==(KittyInputEvent)obj;
        }

        public bool Equals(KittyInputEvent other)
        {
            return command == other.command && value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(command, value);
        }

        // public ulong[] bits;
        // public ulong bit_0_63;
        // public ulong bit_64_127;
        // public ulong bit_128_191;
        // public ulong bit_192_255;
        // public ulong bit_256_319;
        // public ulong bit_320_383;
        // public ulong bit_384_447;
        // public ulong bit_448_511;

        // public static KittyInputEvent GetDefault()
        // {
        //     return new KittyInputEvent
        //     {
        //         bits = new ulong[8]
        //     };
        // }
    }
    
    //TODO: 这还需要一个key map 编辑器， 要写成editor工具
}