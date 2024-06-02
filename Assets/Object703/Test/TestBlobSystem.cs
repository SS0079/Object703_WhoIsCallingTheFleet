// using KittyDOTS;
// using Unity.Burst;
// using Unity.Entities;
// using Random = Unity.Mathematics.Random;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace Object703.Test
// {
//     [BurstCompile]
//     [RequireMatchingQueriesForUpdate]
//     public partial struct TestBlobSystem : ISystem
//     {
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//             
//         }
//
//         public void OnUpdate(ref SystemState state)
//         {
//             if (Input.GetKeyDown(KeyCode.L))
//             {
//                 // foreach (var blobComponent in SystemAPI.Query<RefRO<TeatBlobArrayComponent>>())
//                 // {
//                 //     var length = blobComponent.ValueRO.value.Length();
//                 //     for (int i = 0; i < length; i++)
//                 //     {
//                 //
//                 //         Debug.Log(blobComponent.ValueRO.value.Get(i).b);
//                 //         Debug.Log(blobComponent.ValueRO.value.Value.array[i].b);
//                 //     }
//                 //     length = blobComponent.ValueRO.value2.Value.array.Length;
//                 //     for (int i = 0; i < length; i++)
//                 //     {
//                 //         Debug.Log(blobComponent.ValueRO.value2.Value.array[i].a);
//                 //         Debug.Log(blobComponent.ValueRO.value2.Value.array[i].b);
//                 //     }
//                 // }
//                 // foreach (var blobAssetComponent in SystemAPI.Query<RefRO<TestBlobAssetComponent>>())
//                 // {
//                 //     Debug.Log(blobAssetComponent.ValueRO.value.Value.a);
//                 //     Debug.Log(blobAssetComponent.ValueRO.value.Value.b);
//                 // }
//                 // foreach (var blobAssetComponent in SystemAPI.Query<RefRO<TestFixBlobAssetComponent>>())
//                 // {
//                 //     string valueStringValue =blobAssetComponent.ValueRO.data.Value.stringValue.ToString();
//                 //     Debug.Log(valueStringValue);
//                 //     Debug.Log(blobAssetComponent.ValueRO.data.Value.structValue);
//                 //     var split = valueStringValue.Split(',');
//                 //     for (int i = 0; i < split.Length; i++)
//                 //     {
//                 //         Debug.Log(split[i]);
//                 //     }
//                 // }
//                 foreach (var blobAssetComponent in SystemAPI.Query<RefRO<TestFixBlobArrayComponent>>())
//                 {
//                     for (int i = 0; i < blobAssetComponent.ValueRO.data.Length(); i++)
//                     {
//                         ref var fixBlobAsset = ref blobAssetComponent.ValueRO.data.Get(i);
//                         var s = fixBlobAsset.word.ToString();
//                         var structValue = fixBlobAsset.data;
//                         Debug.Log(s);
//                         Debug.Log(structValue.a);
//                         Debug.Log(structValue.b);
//                     }
//                 }
//             }
//         }
//
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state)
//         {
//
//         }
//     }
// }