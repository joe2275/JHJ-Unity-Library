using UnityEditor;
using UnityEngine;

namespace Level2D
{
    [CustomEditor(typeof(LevelGenerator2D))]
    public class LevelGenerator2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SetLevelGeneratorProperties();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (target is not LevelGenerator2D generator)
            {
                return;
            }

            if (!generator.IsLimited)
            {
                return;
            }

            Vector3 position = generator.transform.position;
            Vector3 localLeftBottom = generator.LeftBottom;
            Vector3 localRightTop = generator.RightTop;
            Vector3 leftBottom = position + localLeftBottom;
            Vector3 rightTop = position + localRightTop;
            
            
            Handles.DrawSolidRectangleWithOutline(new Rect(leftBottom, localRightTop - localLeftBottom), Color.clear, Color.red);

            leftBottom = Handles.PositionHandle(leftBottom, Quaternion.identity);
            rightTop = Handles.PositionHandle(rightTop, Quaternion.identity);
            
            Handles.Label(leftBottom, "Left Bottom");
            Handles.Label(rightTop, "Right Top");

            localLeftBottom = leftBottom - position;
            localRightTop = rightTop - position;

            localRightTop = Vector3.Max(localLeftBottom, localRightTop);

            generator.LeftBottom = localLeftBottom;
            generator.RightTop = localRightTop;
        }

        private void SetLevelGeneratorProperties()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth - 60f;
            GUIStyle labelStyle = new GUIStyle
            {
                fontSize = 20, fontStyle = FontStyle.Bold, 
                normal =
                {
                    textColor = Color.white
                }
            };
            GUIStyle depthStyle = new GUIStyle
            {
                fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }
            };
            
            GUIStyle categoryStyle = new GUIStyle
            {
                fontSize = 15, normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter
            };

            GUILayoutOption labelWidthOption = GUILayout.Width(viewWidth / 2f);
            GUILayoutOption contentWidthOption = GUILayout.Width(viewWidth / 4f);

            SerializedProperty isLimitedProp = serializedObject.FindProperty("isLimited");
            SerializedProperty leftBottomProp = serializedObject.FindProperty("leftBottom");
            SerializedProperty rightTopProp = serializedObject.FindProperty("rightTop");
            SerializedProperty frameArrayProp = serializedObject.FindProperty("frameArray");
            SerializedProperty frameCountArrayProp = serializedObject.FindProperty("frameCountArray");
            SerializedProperty frameWeightArrayProp = serializedObject.FindProperty("frameWeightArray");
            SerializedProperty levelLayersArrayProp = serializedObject.FindProperty("layerInfoArray");

            int length = frameArrayProp.arraySize;
            
            GUILayout.Label("Area", labelStyle);

            isLimitedProp.boolValue = EditorGUILayout.Toggle("Is Limited", isLimitedProp.boolValue);
            leftBottomProp.vector2Value = EditorGUILayout.Vector2Field("Left Bottom", leftBottomProp.vector2Value);
            rightTopProp.vector2Value = Vector2.Max(EditorGUILayout.Vector2Field("Right Top", rightTopProp.vector2Value), leftBottomProp.vector2Value);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Level Frame", labelStyle, labelWidthOption);
            
            if (GUILayout.Button("Add Frame", labelWidthOption))
            {
                frameArrayProp.InsertArrayElementAtIndex(length);
                frameWeightArrayProp.InsertArrayElementAtIndex(length);
                frameCountArrayProp.InsertArrayElementAtIndex(length);
                length++;
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.Label("Prefab", categoryStyle, contentWidthOption);
            for (int i = 0; i < length; i++)
            {
                SerializedProperty frameProp = frameArrayProp.GetArrayElementAtIndex(i);
                frameProp.objectReferenceValue = EditorGUILayout.ObjectField(frameProp.objectReferenceValue, typeof(LevelFrame2D), true, contentWidthOption);
            }
            
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            
            GUILayout.Label("Weight", categoryStyle, contentWidthOption);
            for (int i = 0; i < length; i++)
            {
                SerializedProperty frameWeightProp = frameWeightArrayProp.GetArrayElementAtIndex(i);
                frameWeightProp.floatValue = Mathf.Max(EditorGUILayout.FloatField(frameWeightProp.floatValue, contentWidthOption), 0.0f);
            }
            
            
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            GUILayout.Label("Count", categoryStyle, contentWidthOption);
            for (int i = 0; i < length; i++)
            {
                SerializedProperty frameCountProp = frameCountArrayProp.GetArrayElementAtIndex(i);
                frameCountProp.intValue = EditorGUILayout.IntField(frameCountProp.intValue, contentWidthOption);
            }
            
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            
            GUILayout.Label("Delete", categoryStyle, contentWidthOption);
            int deleteIndex = -1;
            for (int i = 0; i < length; i++)
            {
                if (GUILayout.Button("Delete", contentWidthOption))
                {
                    deleteIndex = i;
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (deleteIndex > -1)
            {
                frameArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                frameWeightArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                frameCountArrayProp.DeleteArrayElementAtIndex(deleteIndex);
            }

            GUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            
            int depth = levelLayersArrayProp.arraySize;
            
            GUILayout.Label("Level Layer", labelStyle, labelWidthOption);
            if (levelLayersArrayProp.arraySize == 0 && GUILayout.Button("Add Depth", labelWidthOption))
            {
                levelLayersArrayProp.InsertArrayElementAtIndex(depth++);
            }

            GUILayout.EndHorizontal();

            int depthInsertIndex = -1;
            int depthDeleteIndex = -1;

            for (int i = 0; i < depth; i++)
            {
                SerializedProperty levelLayersProp = levelLayersArrayProp.GetArrayElementAtIndex(i);
                
                SerializedProperty layerArrayProp = levelLayersProp.FindPropertyRelative("layerArray");
                SerializedProperty layerWeightArrayProp = levelLayersProp.FindPropertyRelative("layerWeightArray");
                SerializedProperty layerCountArrayProp = levelLayersProp.FindPropertyRelative("layerCountArray");

                length = layerArrayProp.arraySize;

                GUILayout.BeginHorizontal();
                
                GUILayout.Label($"{i + 1}. Layers", depthStyle, contentWidthOption);
                if (GUILayout.Button("Insert Depth", contentWidthOption))
                {
                    depthInsertIndex = i;
                }

                if (GUILayout.Button("Delete Depth", contentWidthOption))
                {
                    depthDeleteIndex = i;
                }

                if (GUILayout.Button("Add Layer", contentWidthOption))
                {
                    layerArrayProp.InsertArrayElementAtIndex(length);
                    layerWeightArrayProp.InsertArrayElementAtIndex(length);
                    layerCountArrayProp.InsertArrayElementAtIndex(length);
                    length++;
                }
                
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                
                GUILayout.Label("Prefab", categoryStyle, contentWidthOption);
                for (int j = 0; j < length; j++)
                {
                    SerializedProperty layerProp = layerArrayProp.GetArrayElementAtIndex(j);
                    layerProp.objectReferenceValue = EditorGUILayout.ObjectField(layerProp.objectReferenceValue,
                        typeof(LevelLayer2D), true, contentWidthOption);
                }
                
                GUILayout.EndVertical();
                GUILayout.BeginVertical();

                GUILayout.Label("Weight", categoryStyle, contentWidthOption);
                for (int j = 0; j < length; j++)
                {
                    SerializedProperty layerWeightProp = layerWeightArrayProp.GetArrayElementAtIndex(j);
                    layerWeightProp.floatValue =
                        Mathf.Max(EditorGUILayout.FloatField(layerWeightProp.floatValue, contentWidthOption), 0.0f);
                }
                
                GUILayout.EndVertical();
                GUILayout.BeginVertical();

                GUILayout.Label("Count", categoryStyle, contentWidthOption);
                for (int j = 0; j < length; j++)
                {
                    SerializedProperty layerCountProp = layerCountArrayProp.GetArrayElementAtIndex(j);
                    layerCountProp.intValue = EditorGUILayout.IntField(layerCountProp.intValue, contentWidthOption);
                }
                
                GUILayout.EndVertical();
                GUILayout.BeginVertical();

                GUILayout.Label("Delete", categoryStyle, contentWidthOption);
                deleteIndex = -1;
                for (int j = 0; j < length; j++)
                {
                    if (GUILayout.Button("Delete", contentWidthOption))
                    {
                        deleteIndex = j;
                    }
                }
                
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                if (deleteIndex > -1)
                {
                    layerArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                    layerWeightArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                    layerCountArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                }
            }

            if (depthInsertIndex > -1)
            {
                levelLayersArrayProp.InsertArrayElementAtIndex(depthInsertIndex);
            }
            else if (depthDeleteIndex > -1)
            {
                levelLayersArrayProp.DeleteArrayElementAtIndex(depthDeleteIndex);
            }
        }
    }
}