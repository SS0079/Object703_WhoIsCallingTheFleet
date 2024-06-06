using System;
using KittyHelpYouOut;
using UnityEditor;
using UnityEngine;

namespace Object703.Authoring.Editor
{
    [CustomEditor(typeof(CanMoveAuthoring))]
    public class CanMoveAuthoringEditor : UnityEditor.Editor
    {
        private SerializedProperty moveStyleProp;
        private SerializedProperty arrowMoveProp;
        private SerializedProperty shipMoveProp;
        // private SerializedProperty hoverMoveProp;
        private void OnEnable()
        {
            moveStyleProp = this.Find("style");
            arrowMoveProp = this.Find("arrowMoveConfig");
            shipMoveProp = this.Find("shipMoveConfig");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
            
            serializedObject.ApplyModifiedProperties();
        }

        private void SerializeShipMove(SerializedProperty prop)
        {
            EditorGUILayout.LabelField("Move");
            EditorGUI.indentLevel++;
            {
                prop.ShowRelativeField("moveSpeedPerSecond","MoveSpeed")
                    .ShowRelativeField("moveDampMotion","MotionDamp")
                    .ShowRelativeField("moveDampStop","StopDamp");
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.LabelField("Rotate");
            EditorGUI.indentLevel++;
            {
                prop.ShowRelativeField("rotateDegreePerSecond","RotateSpeed")
                    .ShowRelativeField("rotateDampMotion","MotionDamp")
                    .ShowRelativeField("rotateDampStop","StopDamp");
            }
            EditorGUI.indentLevel--;

        }

        private void SerializeArrowMove(SerializedProperty prop)
        {
            prop.ShowRelativeField("speedPerSecond","MoveSpeed");
        }
    }
}