using System.Collections.Generic;
using KittyHelpYouOut;
using UnityEngine;

namespace Object703.Utility
{
    public class Formation : MonoBehaviour
    {
        // public int Row,Column;
        public float Spacing;
        public Transform[] Pawns;

        [ContextMenu("Arrange")]
        public void Arrange()
        {
            if(Pawns.Length==0) return;
            var column = Mathf.CeilToInt(Mathf.Sqrt(Pawns.Length));
            var center = this.transform.position;
            var rowStart = center.x - Spacing * column / 2f;
            var rowEnd = center.x + Spacing * column / 2f;
            var columnStart = center.z - Spacing * column / 2f;
            var columnEnd = center.z + Spacing * column / 2f;
            var index = 0;
            for (float i = rowStart; i < rowEnd; i+=Spacing)
            {
                for (float j = columnStart; j < columnEnd; j+=Spacing)
                {
                    if (index>Pawns.Length-1) return;
                    Pawns[index].position = new Vector3(i, this.transform.position.y, j);
                    index++;
                }
            }
        }
    }
}