using UnityEditor;
using UnityEngine;

namespace Background2D
{
    [CustomEditor(typeof(ParallaxBackground2D))]
    public class ParallaxBackground2DEditor : Editor
    {
        private SerializedProperty mLeftBottomProp;
        private SerializedProperty mRightTopProp;

        private void OnEnable()
        {
            InitProperties();
        }

        private void OnSceneGUI()
        {
            DrawProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void InitProperties()
        {
            mLeftBottomProp = serializedObject.FindProperty("leftBottom");
            mRightTopProp = serializedObject.FindProperty("rightTop");
        }

        private void DrawProperties()
        {
            Vector2 worldLeftBottom = mLeftBottomProp.vector2Value;
            Vector2 worldRightTop = mRightTopProp.vector2Value;
            
            Handles.DrawSolidRectangleWithOutline(new Rect(worldLeftBottom, worldRightTop - worldLeftBottom), Color.clear, Color.red);

            worldLeftBottom = Handles.PositionHandle(worldLeftBottom, Quaternion.identity);
            worldRightTop = Handles.PositionHandle(worldRightTop, Quaternion.identity);
            
            Handles.Label(worldLeftBottom, "World Left Bottom");
            Handles.Label(worldRightTop, "World Right Top");

            mLeftBottomProp.vector2Value = worldLeftBottom;
            mRightTopProp.vector2Value = worldRightTop;
        }
    }
}