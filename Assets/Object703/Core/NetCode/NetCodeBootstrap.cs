using System;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace Object703.Core.NetCode
{
    [Preserve]
    public class NetCodeBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
//             bool isFrontend = true;
// #if UNITY_EDITOR
//             var curSceneName = SceneManager.GetActiveScene().name;
//             isFrontend = curSceneName.Equals("Frontend", StringComparison.InvariantCultureIgnoreCase);
// #endif
//             if (isFrontend)
//             {
//                 AutoConnectPort = 0;
//                 CreateLocalWorld("LocalWorld");
//             }
            // else
            // {
            //     AutoConnectPort = 7979;
            //     CreateDefaultClientServerWorlds();
            // }
            AutoConnectPort = 0;
            return false;
        }
    }
}