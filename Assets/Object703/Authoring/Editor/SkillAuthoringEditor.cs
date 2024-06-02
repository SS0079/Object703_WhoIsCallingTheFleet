using System;
using Object703.Core.Skill;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Object703.Authoring.Editor
{
    [CustomEditor(typeof(SkillAuthoring))]
    public class SkillAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty skillProp;
        private void OnEnable()
        {
            skillProp = serializedObject.FindProperty("skill");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Skill",EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //serialize skill
            //serialize skill slot
            var slotProp = skillProp.FindPropertyRelative("slot");
            EditorGUILayout.PropertyField(slotProp, new GUIContent("Slot"));
            //serialize skill type
            var skillType = skillProp.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(skillType, new GUIContent("Type"));
            var type = (SkillAuthoring.SkillType)skillType.enumValueIndex;
            //serialize rest of skill according to skill type
            
            switch (type)
            {
                case SkillAuthoring.SkillType.Shot:
                    SerializeShotSkill(skillProp);
                    break;
                case SkillAuthoring.SkillType.Teleport:
                    SerializeTeleportSkill(skillProp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        private void SerializeShotSkill(SerializedProperty skillElement)
        {
            var commonData = skillElement.FindPropertyRelative("data");
            var radiusProp = commonData.FindPropertyRelative("radius");
            EditorGUILayout.PropertyField(radiusProp, new GUIContent("Radius"));
            var rangeProp = commonData.FindPropertyRelative("range");
            EditorGUILayout.PropertyField(rangeProp, new GUIContent("Range"));
            var delayBetweenActivateProp = commonData.FindPropertyRelative("coolDown");
            EditorGUILayout.PropertyField(delayBetweenActivateProp, new GUIContent("coolDown"));
            var lifeSpanProp = commonData.FindPropertyRelative("lifeSpan");
            EditorGUILayout.PropertyField(lifeSpanProp, new GUIContent("LifeSpan"));
            
            var spawnPrefabProp = skillElement.FindPropertyRelative("spawnPrefab");
            EditorGUILayout.PropertyField(spawnPrefabProp, new GUIContent("SpawnPrefab"));
        }
        
        private void SerializeTeleportSkill(SerializedProperty skillElement)
        {
            var commonData = skillElement.FindPropertyRelative("data");
            var radiusProp = commonData.FindPropertyRelative("radius");
            EditorGUILayout.PropertyField(radiusProp, new GUIContent("Radius"));
            var rangeProp = commonData.FindPropertyRelative("range");
            EditorGUILayout.PropertyField(rangeProp, new GUIContent("Range"));
            var delayBetweenActivateProp = commonData.FindPropertyRelative("coolDown");
            EditorGUILayout.PropertyField(delayBetweenActivateProp, new GUIContent("coolDown"));
            var lifeSpanProp = commonData.FindPropertyRelative("lifeSpan");
            EditorGUILayout.PropertyField(lifeSpanProp, new GUIContent("LifeSpan"));
        }
    }
}