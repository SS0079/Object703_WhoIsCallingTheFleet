using System;
using System.Reflection;
using System.Text;
using Object703.Test;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace KittyDOTS
{
    public struct SimpleBlobArray<T> where T : unmanaged
    {
        public BlobArray<T> array;
    }
    
    public struct FixBlobAsset<T> where T : unmanaged
    {
        public BlobString word;
        public T data;
    }

    public struct FixBlobArray<T> where T : unmanaged
    {
        public BlobArray<FixBlobAsset<T>> array;
    }
    
    /// <summary>
    /// 猫猫Blob封装！ 让你告别屎一样的Blob体验
    /// </summary>
    public static class BlobHelper
    {
        
        public static BlobAssetReference<T> CreateSimpleStructReference<T>(T data) where T : unmanaged
        {
            // Create a new builder that will use temporary memory to construct the blob asset
            var builder = new BlobBuilder(Allocator.Temp);

            // Construct the root object for the blob asset. Notice the use of `ref`
            ref T dataRef = ref builder.ConstructRoot<T>();
            
            // Now fill the constructed root with the data
            dataRef = data;

            // Now copy the data from the builder into its final place, which will
            // use the persistent allocator
            var result = builder.CreateBlobAssetReference<T>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
        
        public static BlobAssetReference<SimpleBlobArray<T>> CreateSimpleArrayReference<T>(T[] data) where T : unmanaged
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref SimpleBlobArray<T> root = ref builder.ConstructRoot<SimpleBlobArray<T>>();
            BlobBuilderArray<T> arrayBuilder = builder.Construct(ref root.array, data);
            var result = builder.CreateBlobAssetReference<SimpleBlobArray<T>>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        public static BlobAssetReference<FixBlobAsset<T>> CreateFixBlobAssetReference<T>(T data, string stringData) where T : unmanaged
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref FixBlobAsset<T> fixDataRef = ref builder.ConstructRoot<FixBlobAsset<T>>();
            // if (stringData.Length>0)
            // {
            //     var sb = new StringBuilder();
            //     sb.Append(stringData[0]);
            //     for (int i = 1; i < stringData.Length; i++)
            //     {
            //         sb.Append($",{stringData[i]}");
            //     }
            // }
            builder.AllocateString(ref fixDataRef.word,stringData);
            fixDataRef.data = data;
            var result = builder.CreateBlobAssetReference<FixBlobAsset<T>>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        public static BlobAssetReference<FixBlobArray<T>> CreateFixArrayReference<T>(T[] data, string[] strings) where T : unmanaged
        {
            if (data.Length!=strings.Length)
            {
                Debug.LogWarning($"number of data and string mismatch");
                return default;
            }
            var builder = new BlobBuilder(Allocator.Temp);
            ref var fixArrayRef = ref builder.ConstructRoot<FixBlobArray<T>>();
            var arrayBuilder = builder.Allocate(ref fixArrayRef.array,data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                builder.AllocateString(ref arrayBuilder[i].word,strings[i]);
                arrayBuilder[i].data = data[i];
            }
            var result = builder.CreateBlobAssetReference<FixBlobArray<T>>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }
        
        public static T Get<T>(this BlobAssetReference<SimpleBlobArray<T>> sba, int index) where T : unmanaged
        {
            return sba.Value.array[index];
        }

        public static ref FixBlobAsset<T> Get<T>(this BlobAssetReference<FixBlobArray<T>> fba, int index) where T : unmanaged
        {
            return ref fba.Value.array[index];
        }
        
        public static int Length<T>(this BlobAssetReference<SimpleBlobArray<T>> sba) where T : unmanaged
        {
            return sba.Value.array.Length;
        }

        public static int Length<T>(this BlobAssetReference<FixBlobArray<T>> fba) where T : unmanaged
        {
            return fba.Value.array.Length;
        }
    }
}