using Unity.Burst;
using Unity.Entities;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
namespace Object703.Utility
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct LoadStartSceneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = false;
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0)) return;
            SceneManager.LoadScene(0);
        }
    }
}
#endif