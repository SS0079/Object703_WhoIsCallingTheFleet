using UnityEngine;

namespace Object703.Utility
{
    public class TileMaker : MonoBehaviour
    {
        public int Column;
        public int Row;
        public Vector2 Spacing;
        public GameObject Prefab;
        
        [ContextMenu("Draw")]
        public void Draw()
        {
            var center = this.transform.position;
            var rowStart = center.x - Spacing.x * Column / 2f;
            var rowEnd = center.x + Spacing.x * Column / 2f;
            var columnStart = center.z - Spacing.y * Row / 2f;
            var columnEnd = center.z + Spacing.y * Row / 2f;
            for (float i = rowStart; i <= rowEnd; i+=Spacing.x)
            {
                for (float j = columnStart ; j <= columnEnd; j+=Spacing.y)
                {
                    var localTile = Instantiate(Prefab, new Vector3(i, center.y, j), this.transform.rotation, transform);
                    localTile.isStatic = true;
                }
            }
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            for (int i = this.transform.childCount-1; i >= 0; i--)
            {
                DestroyImmediate(this.transform.GetChild(i).gameObject);
            }
        }
    }

}