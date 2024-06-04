using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Object703.Authoring.Editor
{
    [CustomEditor(typeof(CanMoveAuthoring))]
    public class CanMoveAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty moveStyleProp;
        private SerializedProperty netConfigProp;
        private SerializedProperty arrowMoveProp;
        private SerializedProperty shipMoveProp;
        // private SerializedProperty hoverMoveProp;
        private void OnEnable()
        {
            moveStyleProp = serializedObject.FindProperty("style");
            netConfigProp = serializedObject.FindProperty("netConfig");
            arrowMoveProp = serializedObject.FindProperty("arrowMoveConfig");
            shipMoveProp = serializedObject.FindProperty("shipMoveConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("NetCodeConfig",EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(netConfigProp, new GUIContent("Config"));
            EditorGUI.indentLevel--;
            if (netConfigProp.objectReferenceValue!=null)
            {
                // EditorGUILayout.LabelField("MoveConfig",EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(moveStyleProp, new GUIContent("Style"));
                var style = (CanMoveAuthoring.MoveStyle)moveStyleProp.enumValueIndex;
                switch (style)
                {
                    case CanMoveAuthoring.MoveStyle.Arrow:
                        SerializeArrowMove(arrowMoveProp);
                        break;
                    case CanMoveAuthoring.MoveStyle.Ship:
                        SerializeShipMove(shipMoveProp);
                        break;
                    case CanMoveAuthoring.MoveStyle.Hover:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void SerializeShipMove(SerializedProperty prop)
        {
            EditorGUILayout.LabelField("Move");
            EditorGUI.indentLevel++;
            {
                Wrapper_PropertyField(prop,"moveSpeedPerSecond","MoveSpeed");
                // var moveSpeedProp = prop.FindPropertyRelative("moveSpeedPerSecond");
                // EditorGUILayout.PropertyField(moveSpeedProp, new GUIContent("MoveSpeed"));
                Wrapper_PropertyField(prop,"moveDampMotion","MotionDamp");
                // var motionDampProp = prop.FindPropertyRelative("moveDampMotion");
                // EditorGUILayout.PropertyField(motionDampProp, new GUIContent("MotionDamp"));
                Wrapper_PropertyField(prop,"moveDampStop","StopDamp");
                // var stopDampProp = prop.FindPropertyRelative("moveDampStop");
                // EditorGUILayout.PropertyField(stopDampProp, new GUIContent("StopDamp"));
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.LabelField("Rotate");
            EditorGUI.indentLevel++;
            {
                Wrapper_PropertyField(prop,"rotateDegreePerSecond","RotateSpeed");
                // var rotateSpeedProp = prop.FindPropertyRelative("rotateDegreePerSecond");
                // EditorGUILayout.PropertyField(rotateSpeedProp, new GUIContent("RotateSpeed"));
                Wrapper_PropertyField(prop,"rotateDampMotion","MotionDamp");
                // var motionDampProp = prop.FindPropertyRelative("rotateDampMotion");
                // EditorGUILayout.PropertyField(motionDampProp, new GUIContent("MotionDamp"));
                Wrapper_PropertyField(prop,"rotateDampStop","StopDamp");
                // var stopDampProp = prop.FindPropertyRelative("rotateDampStop");
                // EditorGUILayout.PropertyField(stopDampProp, new GUIContent("StopDamp"));
            }
            EditorGUI.indentLevel--;

        }

        private void SerializeArrowMove(SerializedProperty prop)
        {
            Wrapper_PropertyField(prop,"speedPerSecond","MoveSpeed");
        }

        private void Wrapper_PropertyField(SerializedProperty prop,string propName, string propLabel)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative(propName), new GUIContent(propLabel));
        }
    }
}