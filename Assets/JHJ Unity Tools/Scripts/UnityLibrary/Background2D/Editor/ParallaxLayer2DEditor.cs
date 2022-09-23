using UnityEditor;
using UnityEngine;

namespace Background2D
{
    [CustomEditor(typeof(ParallaxLayer2D))]
    public class ParallaxLayer2DEditor : Editor
    {
        private SerializedProperty mLeftBottomPointProp;
        private SerializedProperty mRightTopPointProp;

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
            mLeftBottomPointProp = serializedObject.FindProperty("leftBottomPoint");
            mRightTopPointProp = serializedObject.FindProperty("rightTopPoint");
        }

        private void DrawProperties()
        {
            if (target is not ParallaxLayer2D layer)
            {
                return;
            }
            
            Vector2 position = layer.transform.position;
            Vector2 leftBottomPoint = mLeftBottomPointProp.vector2Value;
            Vector2 rightTopPoint = mRightTopPointProp.vector2Value;
            Vector2 leftBottom = position + leftBottomPoint;
            Vector2 rightTop = position + rightTopPoint;
            
            Handles.DrawSolidRectangleWithOutline(new Rect(leftBottom, rightTopPoint - leftBottomPoint), Color.clear, Color.white);
            leftBottom = Handles.PositionHandle(leftBottom, Quaternion.identity);
            rightTop = Handles.PositionHandle(rightTop, Quaternion.identity);
            
            Handles.Label(leftBottom, "Layer Left Bottom");
            Handles.Label(rightTop, "Layer Right Top");

            mLeftBottomPointProp.vector2Value = leftBottom - position;
            mRightTopPointProp.vector2Value = rightTop - position;
        }
    }
}