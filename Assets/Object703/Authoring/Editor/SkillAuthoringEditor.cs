﻿using System;
using KittyHelpYouOut;
using UnityEditor;

namespace Object703.Authoring.Editor
{
    [CustomEditor(typeof(SkillAuthoring))]
    public class SkillAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty netConfigProp;
        private SerializedProperty slotProp;
        private SerializedProperty skillTypeProp;
        private SerializedProperty commonDataProp;
        private SerializedProperty spawnPrefabProp;
        private void OnEnable()
        {
            netConfigProp = this.Find("netConfig");
            slotProp = this.Find("slot");
            skillTypeProp = this.Find("type");
            commonDataProp = this.Find("data");
            spawnPrefabProp = this.Find("spawnPrefab");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            netConfigProp.ShowField();
            if (netConfigProp.objectReferenceValue!=null)
            {
                EditorGUI.indentLevel++;
                //serialize skill slot
                slotProp.ShowField();
                //serialize skill type
                skillTypeProp.ShowField();
                var type = (SkillAuthoring.SkillType)skillTypeProp.enumValueIndex;
                //serialize rest of skill according to skill type
                
                commonDataProp.ShowField("commonData");
                switch (type)
                {
                    case SkillAuthoring.SkillType.Shot:
                        spawnPrefabProp.ShowField();
                        break;
                    case SkillAuthoring.SkillType.Teleport:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                EditorGUI.indentLevel--;
            }
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
};