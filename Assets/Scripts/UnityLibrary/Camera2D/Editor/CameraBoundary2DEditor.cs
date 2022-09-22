using UnityEditor;
using UnityEngine;

namespace Camera2D
{
    [CustomEditor(typeof(CameraBoundary2D))]
    public class CameraBoundary2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            UpdateBoundaryProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (target is not CameraBoundary2D boundary)
            {
                return;
            }

            Vector3 position = boundary.transform.position;
            Vector3 startPoint = boundary.StartPoint;
            Vector3 endPoint = boundary.EndPoint;
            startPoint += position;
            endPoint += position;
            Handles.Label(startPoint, "Start");
            Handles.Label(endPoint, "End");
            startPoint = Handles.PositionHandle(startPoint, Quaternion.identity);
            endPoint = Handles.PositionHandle(endPoint, Quaternion.identity);
            Handles.DrawLine(startPoint, endPoint);
            startPoint -= position;
            endPoint -= position;
            boundary.StartPoint = startPoint;
            boundary.EndPoint = endPoint;
        }

        private void UpdateBoundaryProperties()
        {
            SerializedProperty startProp = serializedObject.FindProperty("start");
            SerializedProperty endProp = serializedObject.FindProperty("end");
            
            EditorGUILayout.LabelField("Boundary Line", EditorStyles.boldLabel);
            startProp.vector2Value = EditorGUILayout.Vector2Field("Start Point", startProp.vector2Value);
            endProp.vector2Value = EditorGUILayout.Vector2Field("End Point", endProp.vector2Value);
        }
    }
}