using UnityEngine;

namespace KittyHelpYouOut
{
    public class MeshFixTool : MonoBehaviour
    {
        [ContextMenu("Fix")]
        public void Fix()
        {
            var meshFilters = this.GetComponentsInChildren<MeshFilter>();
            var meshColliders = this.GetComponentsInChildren<MeshCollider>();
            Mesh[] meshs = Resources.FindObjectsOfTypeAll<Mesh>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                var localFilter = meshFilters[i];
                var localName = localFilter.gameObject.name;
                for (int j = 0; j < meshs.Length; j++)
                {
                    var localMesh = meshs[j];
                    var localMeshName = localMesh.name;
                    if (localMeshName==localName)
                    {
                        localFilter.mesh = localMesh;
                    }
                }
            }
            for (int i = 0; i < meshColliders.Length; i++)
            {
                var localCollider = meshColliders[i];
                var localName = localCollider.gameObject.name;
                for (int j = 0; j < meshs.Length; j++)
                {
                    var localMesh = meshs[j];
                    var localMeshName = localMesh.name;
                    if (localMeshName==localName)
                    {
                        localCollider.sharedMesh = localMesh;
                    }
                }
            }
            
        }
    }
}