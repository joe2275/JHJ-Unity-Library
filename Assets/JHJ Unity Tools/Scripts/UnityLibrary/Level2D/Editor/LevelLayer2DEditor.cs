using System;
using UnityEditor;
using UnityEngine;

namespace Level2D
{
    [CustomEditor(typeof(LevelLayer2D))]
    public class LevelLayer2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SetLevelLayerProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (target is not LevelLayer2D layer)
            {
                return;
            }

            Vector3 position = layer.transform.position;
            Vector3 localLeftBottom = layer.LeftBottom;
            Vector3 leftBottom = position + localLeftBottom;
            
            
            leftBottom = Handles.PositionHandle(leftBottom, Quaternion.identity);
            Handles.Label(leftBottom, "Layer Left Bottom");

            layer.LeftBottom = leftBottom - position;
        }

        private void SetLevelLayerProperties()
        {
            GUIStyle titleStyle = new GUIStyle
            {
                fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }
            };

            SerializedProperty frameKeyProp = serializedObject.FindProperty("frameKey");
            SerializedProperty leftBottomProp = serializedObject.FindProperty("leftBottom");

            GUILayout.Label("Level Layer", titleStyle);
            
            GUILayout.Space(20);

            frameKeyProp.intValue = EditorGUILayout.IntField("Frame Key", frameKeyProp.intValue);
            
            GUILayout.Space(20);

            leftBottomProp.vector2Value =
                EditorGUILayout.Vector2Field("Left Bottom Point", leftBottomProp.vector2Value);
        }
    }
}