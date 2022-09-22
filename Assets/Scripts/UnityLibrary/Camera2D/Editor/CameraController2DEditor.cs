using UnityEditor;
using UnityEngine;

namespace Camera2D
{
    [CustomEditor(typeof(CameraController2D))]
    public class CameraController2DEditor : Editor
    {
        private SerializedProperty mWorldCameraProp;
        private SerializedProperty mUiCameraProp;
        private SerializedProperty mKeyProp;
        private SerializedProperty mSmoothnessProp;
        private SerializedProperty mFollowProp;
        private SerializedProperty mFollowZoneHalfWidthProp;
        private SerializedProperty mFollowZoneHalfHeightProp;
        private SerializedProperty mFollowInertiaDistanceProp;
        private SerializedProperty mFollowInertiaSpeedProp;

        public override void OnInspectorGUI()
        {
            UpdateCameraControllerProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            InitProperties();
        }

        private void OnSceneGUI()
        {
            if (target is not CameraController2D controller)
            {
                return;
            }
            
            Vector2 position = controller.transform.position;
            float width = controller.FollowZoneHalfWidth;
            float height = controller.FollowZoneHalfHeight;
            
            Handles.DrawSolidRectangleWithOutline(new Rect(new Vector2(position.x - width, position.y - height), new Vector2(width * 2.0f, height * 2.0f)), Color.clear, Color.red);

            float inertiaDistance = controller.FollowInertiaDistance;
            Vector3 leftInertia = new Vector3(position.x - inertiaDistance, position.y);
            Vector3 rightInertia = new Vector3(position.x + inertiaDistance, position.y);
            Vector3 inertia = new Vector3(position.x + controller.HorizontalInertia, position.y);

            Handles.color = Color.green;
            Handles.DrawDottedLine(leftInertia, rightInertia, 3.0f);
            Handles.color = Color.red;
            Handles.DrawLine(position, inertia);
        }

        private void UpdateCameraControllerProperties()
        {
            EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
            mWorldCameraProp.objectReferenceValue = EditorGUILayout.ObjectField("World",
                mWorldCameraProp.objectReferenceValue, typeof(Camera), true);
            mUiCameraProp.objectReferenceValue =
                EditorGUILayout.ObjectField("UI", mUiCameraProp.objectReferenceValue, typeof(Camera), true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            mSmoothnessProp.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Smoothness", mSmoothnessProp.floatValue), 1.0f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Follow", EditorStyles.boldLabel);
            mFollowProp.objectReferenceValue =
                EditorGUILayout.ObjectField("Follow", mFollowProp.objectReferenceValue, typeof(Transform), true);
            mFollowZoneHalfWidthProp.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Follow Zone Width", mFollowZoneHalfWidthProp.floatValue), 0.0f);
            mFollowZoneHalfHeightProp.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Follow Zone Height", mFollowZoneHalfHeightProp.floatValue), 0.0f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inertia", EditorStyles.boldLabel);
            mFollowInertiaDistanceProp.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Inertia Distance", mFollowInertiaDistanceProp.floatValue), 0.0f);
            mFollowInertiaSpeedProp.floatValue =
                Mathf.Max(EditorGUILayout.FloatField("Inertia Speed", mFollowInertiaSpeedProp.floatValue), 0.0f);
        }

        private void InitProperties()
        {
            mWorldCameraProp = serializedObject.FindProperty("worldCamera");
            mUiCameraProp = serializedObject.FindProperty("uiCamera");
            mSmoothnessProp = serializedObject.FindProperty("smoothness");
            mFollowProp = serializedObject.FindProperty("follow");
            mFollowZoneHalfWidthProp = serializedObject.FindProperty("followZoneHalfWidth");
            mFollowZoneHalfHeightProp = serializedObject.FindProperty("followZoneHalfHeight");
            mFollowInertiaDistanceProp = serializedObject.FindProperty("followInertiaDistance");
            mFollowInertiaSpeedProp = serializedObject.FindProperty("followInertiaSpeed");
        }
    }
}