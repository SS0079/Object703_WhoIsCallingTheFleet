using System;
using KittyDOTS;
using Unity.Entities;
using UnityEngine;

namespace Object703.Test
{
    [DisallowMultipleComponent]
    public class TestBlobCreater : MonoBehaviour
    {
        public A[] ints;
        public A[] ints2;
        public A int3;
        public string word;
        public int count;
        public B[] bs;
        class TestBlobCreaterBaker : Baker<TestBlobCreater>
        {
            public override void Bake(TestBlobCreater authoring)
            {
                var self = GetEntity(TransformUsageFlags.None);
                // var blobArrayReference = BlobHelper.CreateSimpleArrayReference(authoring.ints);
                // var blobAssetReference2 = BlobHelper.CreateSimpleArrayReference(authoring.ints2);
                // var blobComponent = new TeatBlobArrayComponent
                // {
                //     value = blobArrayReference,
                //     value2 = blobAssetReference2
                // };
                // AddComponent(self,blobComponent);
                // var blobAssetReference3 = BlobHelper.CreateSimpleStructReference(authoring.int3);
                // var blobComponent2 = new TestBlobAssetComponent
                // {
                //     value = blobAssetReference3
                // };
                // AddComponent(self,blobComponent2);
                // var bar4 = BlobHelper.CreateFixBlobAssetReference(authoring.count, authoring.word);
                // var bc3 = new TestFixBlobAssetComponent
                // {
                //     data = bar4
                // };
                // AddComponent(self,bc3);
                // var localA = new A[authoring.bs.Length];
                // var localWords = new string[authoring.bs.Length];
                // for (int i = 0; i < localA.Length; i++)
                // {
                //     localA[i] = authoring.bs[i].data;
                //     localWords[i] = authoring.bs[i].word;
                // }
                // var bar5 = BlobHelper.CreateFixArrayReference(localA, localWords);
                // var bc4 = new TestFixBlobArrayComponent
                // {
                //     data = bar5
                // };
                // AddComponent(self,bc4);
            }
        }
    }

    [Serializable]
    public struct A
    {
        public int a;
        public int b;
    }

    [Serializable]
    public struct B
    {
        public string word;
        public A data;
    }
    
    public struct TeatBlobArrayComponent : IComponentData
    {
        public BlobAssetReference<SimpleBlobArray<A>> value;
        public BlobAssetReference<SimpleBlobArray<A>> value2;
    }
    public struct TestBlobAssetComponent : IComponentData
    {
        public BlobAssetReference<A> value;
    }

    public struct TestFixBlobAssetComponent : IComponentData
    {
        public BlobAssetReference<FixBlobAsset<int>> data;
    }

    public struct TestFixBlobArrayComponent : IComponentData
    {
        public BlobAssetReference<FixBlobArray<A>> data;
    }

}