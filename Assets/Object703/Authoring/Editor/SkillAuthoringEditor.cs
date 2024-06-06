using System;
using KittyHelpYouOut;
using UnityEditor;

namespace Object703.Authoring.Editor
{
    [CustomEditor(typeof(SkillAuthoring))]
    public class SkillAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty slotProp;
        private SerializedProperty skillTypeProp;
        private SerializedProperty commonDataProp;
        private SerializedProperty spawnPrefabProp;
        private void OnEnable()
        {
            slotProp = this.Find("slot");
            skillTypeProp = this.Find("type");
            commonDataProp = this.Find("data");
            spawnPrefabProp = this.Find("spawnPrefab");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Skill",EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //serialize skill
            //serialize skill slot
            slotProp.ShowField();
            //serialize skill type
            skillTypeProp.ShowField();
            var type = (SkillAuthoring.SkillType)skillTypeProp.enumValueIndex;
            //serialize rest of skill according to skill type
            
            switch (type)
            {
                case SkillAuthoring.SkillType.Shot:
                    SerializeCommonData(commonDataProp);
                    spawnPrefabProp.ShowField();
                    break;
                case SkillAuthoring.SkillType.Teleport:
                    SerializeCommonData(commonDataProp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        private void SerializeCommonData(SerializedProperty commonData)
        {
            commonData.ShowRelativeField("radius")
                .ShowRelativeField("range")
                .ShowRelativeField("coolDown")
                .ShowRelativeField("lifeSpan");
        }
    }
}