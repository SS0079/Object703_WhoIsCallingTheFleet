using System;
using System.Text;
using KittyHelpYouOut.ServiceClass;
using QFramework;
using UnityEditor;
using UnityEngine;

namespace KittyHelpYouOut
{
#if UNITY_EDITOR
    public static class UnityEditorHelper
    {
        public static SerializedProperty ShowRelativeField(this SerializedProperty prop,string propName, string propLabel=null)
        {
            if (propLabel==null)
            {
                propLabel = propName;
            }
            //simply return if prop label and prop name are null or empty
            if (propLabel.IsNullOrEmpty()) return prop;
            propLabel=propLabel.ToPascalCaseWithSpaces();
            EditorGUILayout.PropertyField(prop.FindPropertyRelative(propName), new GUIContent(propLabel));
            return prop;
        }

        public static SerializedProperty ShowField(this SerializedProperty prop, string propLabel = null)
        {
            if (propLabel==null)
            {
                propLabel = prop.name;
            }
            propLabel=propLabel.ToPascalCaseWithSpaces();
            EditorGUILayout.PropertyField(prop, new GUIContent(propLabel));
            return prop;
        }

        public static SerializedProperty Find(this Editor editor, string propName)
        {
            return editor.serializedObject.FindProperty(propName);
        }
    }
#endif
}