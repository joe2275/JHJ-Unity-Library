using UnityEditor;
using UnityEngine;

namespace Character2D
{
    [CustomEditor(typeof(CharacterController2D))]
    public class CharacterController2DEditor : Editor
    {
        private SerializedProperty mBodyProp;
        private SerializedProperty mFeetProp;
        private SerializedProperty mCenterProp;
        private SerializedProperty mForwardProp;
        private SerializedProperty mUpProp;
        
        private SerializedProperty mGroundRadiusProp;
        private SerializedProperty mGroundHeightProp;
        private SerializedProperty mSlopeRadiusProp;
        private SerializedProperty mSlopeUpHeightProp;
        private SerializedProperty mSlopeDownHeightProp;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdateProperties();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnEnable()
        {
            InitProperties();
        }

        private void OnSceneGUI()
        {
            DrawProperties();
        }

        private void DrawProperties()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (mCenterProp.objectReferenceValue is not Transform center)
            {
                return;
            }

            Vector3 centerPosition = center.position;
            Handles.Label(centerPosition, "Center");

            if (mForwardProp.objectReferenceValue is Transform forward)
            {
                Vector3 forwardPosition = forward.position;
                Vector3 forwardVec = forwardPosition - centerPosition;

                Handles.color = Handles.xAxisColor;
                Handles.ArrowHandleCap(0, centerPosition + forwardVec * 0.5f, Quaternion.LookRotation(forwardVec), 0.5f, EventType.Repaint);
                Handles.Label(forwardPosition, "Forward");
            }

            if (mUpProp.objectReferenceValue is Transform up)
            {
                Vector3 upPosition = up.position;
                Vector3 upVec = upPosition - centerPosition;

                Handles.color = Handles.yAxisColor;
                Handles.ArrowHandleCap(0, centerPosition + upVec * 0.5f, Quaternion.LookRotation(upVec), 0.5f, EventType.Repaint);
                Handles.Label(upPosition, "Up");
            }

            if (mFeetProp.objectReferenceValue is Transform feet)
            {
                Vector3 feetPosition = feet.position;
                
                Handles.color = Color.red;
                Handles.SphereHandleCap(0, feetPosition, Quaternion.identity, mGroundRadiusProp.floatValue, EventType.Repaint);
                Handles.Label(feetPosition, "Feet");
                Vector3 feetVec = (feetPosition - centerPosition).normalized;
                Vector3 groundSpherePosition = feetPosition + feetVec * mGroundHeightProp.floatValue;
                Handles.DrawLine(feetPosition, groundSpherePosition);
                Handles.SphereHandleCap(0, groundSpherePosition, Quaternion.identity, mGroundRadiusProp.floatValue, EventType.Repaint);
                Handles.Label(groundSpherePosition, "Grounded");

                Handles.color = new Color(1f, 0f, 0f, 0.3f);
                
                Vector3 slopeHeightPosition = feetPosition - feetVec * mSlopeUpHeightProp.floatValue;
                Handles.DrawWireDisc(slopeHeightPosition, -feetVec, mSlopeRadiusProp.floatValue, 5f);
                Handles.Label(slopeHeightPosition + Vector3.right * mSlopeRadiusProp.floatValue, "Slope Height");

                Vector3 slopeDownHeightPosition = feetPosition + feetVec * mSlopeDownHeightProp.floatValue;
                Handles.DrawWireDisc(slopeDownHeightPosition, -feetVec, mSlopeRadiusProp.floatValue, 5f);
                Handles.Label(slopeDownHeightPosition + Vector3.right * mSlopeRadiusProp.floatValue, "Slope Down Height");
            }
        }

        private void UpdateProperties()
        {
            Transform body = mBodyProp.objectReferenceValue as Transform;
            if (body is null)
            {
                return;
            }
            
            Transform center = mCenterProp.objectReferenceValue as Transform;
            if (center is null)
            {
                center = body.Find("Center");
                if (center is null)
                {
                    center = new GameObject("Center").transform;
                    center.SetParent(body);
                    center.localPosition = new Vector3(0f, 0.5f, 0f);
                    center.localRotation = Quaternion.identity;
                    center.localScale = Vector3.one;
                }

                mCenterProp.objectReferenceValue = center;
            }

            if (mFeetProp.objectReferenceValue is null)
            {
                Transform feet = body.Find("Feet");
                if (feet is null)
                {
                    feet = new GameObject("Feet").transform;
                    feet.SetParent(body);
                    feet.localPosition = new Vector3(0f, -0.5f, 0f);
                    feet.localRotation = Quaternion.identity;
                    feet.localScale = Vector3.one;
                }

                mFeetProp.objectReferenceValue = feet;
            }

            if (mForwardProp.objectReferenceValue is null)
            {
                Transform forward = center.Find("Forward");
                if (forward is null)
                {
                    forward = new GameObject("Forward").transform;
                    forward.SetParent(center);
                    forward.localPosition = new Vector3(1f, 0f, 0f);
                    forward.localRotation = Quaternion.identity;
                    forward.localScale = Vector3.one;
                }

                mForwardProp.objectReferenceValue = forward;
            }

            if (mUpProp.objectReferenceValue is null)
            {
                Transform up = center.Find("Up");
                if (up is null)
                {
                    up = new GameObject("Up").transform;
                    up.SetParent(center);
                    up.localPosition = new Vector3(0f, 1f, 0f);
                    up.localRotation = Quaternion.identity;
                    up.localScale = Vector3.one;
                }

                mUpProp.objectReferenceValue = up;
            }
        }

        private void InitProperties()
        {
            mBodyProp = serializedObject.FindProperty("body");
            mFeetProp = serializedObject.FindProperty("feet");
            mCenterProp = serializedObject.FindProperty("center");
            mForwardProp = serializedObject.FindProperty("forward");
            mUpProp = serializedObject.FindProperty("up");
            mGroundRadiusProp = serializedObject.FindProperty("groundRadius");
            mGroundHeightProp = serializedObject.FindProperty("groundHeight");
            mSlopeRadiusProp = serializedObject.FindProperty("slopeRadius");
            mSlopeUpHeightProp = serializedObject.FindProperty("slopeUpHeight");
            mSlopeDownHeightProp = serializedObject.FindProperty("slopeDownHeight");
        }
    }
}