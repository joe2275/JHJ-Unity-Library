using UnityEditor;
using UnityEngine;

namespace Level2D
{
    [CustomEditor(typeof(LevelFrame2D))]
    public class LevelFrame2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SetLevelFrameProperties();
            serializedObject.ApplyModifiedProperties();
        }
        
        private void OnSceneGUI()
        {
            if (target is not LevelFrame2D frame)
            {
                return;
            }

            Vector3 position = frame.transform.position;
            Vector3 localLeftBottom = frame.LeftBottom;
            Vector3 localRightTop = frame.RightTop;
            Vector3 leftBottom = position + localLeftBottom;
            Vector3 rightTop = position + localRightTop;
            
            Handles.DrawSolidRectangleWithOutline(new Rect(leftBottom, (localRightTop - localLeftBottom)), Color.clear, Color.yellow);
            leftBottom = Handles.PositionHandle(leftBottom, Quaternion.identity);
            rightTop = Handles.PositionHandle(rightTop, Quaternion.identity);
            Handles.Label(leftBottom, "Frame Left Bottom");
            Handles.Label(rightTop, "Frame Right Top");

            if (leftBottom.x > rightTop.x)
            {
                leftBottom.x = rightTop.x;
            }

            if (leftBottom.y > rightTop.y)
            {
                leftBottom.y = rightTop.y;
            }

            frame.LeftBottom = leftBottom - position;
            frame.RightTop = rightTop - position;

            for (SocketDirection2D dir = 0; dir < SocketDirection2D.Count; dir++)
            {
                for (int i = 0; i < frame.GetSocketCount(dir); i++)
                {
                    LevelSocket2D socket = frame.GetSocket(dir, i);
                    Vector3 socketLocalPosition = socket.LocalPosition;
                    Vector3 socketPosition = position + socketLocalPosition;
                    
                    socketPosition = Handles.PositionHandle(socketPosition, Quaternion.identity);
                    Handles.Label(socketPosition, $"{dir.ToString()} Socket [{i}]");

                    socket.LocalPosition = socketPosition - position;
                }
            }
        }

        private void SetLevelFrameProperties()
        {
            GUIStyle titleStyle = new GUIStyle
            {
                fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }
            };

            GUIStyle labelStyle = new GUIStyle
            {
                fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.white }
            };

            GUIStyle categoryStyle = new GUIStyle
            {
                fontSize = 15, normal = { textColor = Color.white }
            };

            float viewWidth = EditorGUIUtility.currentViewWidth - 20f;
            
            SerializedProperty frameKeyProp = serializedObject.FindProperty("frameKey");
            SerializedProperty leftBottomProp = serializedObject.FindProperty("leftBottom");
            SerializedProperty rightTopProp = serializedObject.FindProperty("rightTop");
            SerializedProperty socketArrayByDirectionProp = serializedObject.FindProperty("socketArrayByDirection");

            if (socketArrayByDirectionProp.arraySize < (int)SocketDirection2D.Count)
            {
                socketArrayByDirectionProp.arraySize = (int)SocketDirection2D.Count;
                return;
            }

            GUILayout.Label("Level Frame", titleStyle);
            
            GUILayout.Space(20);

            frameKeyProp.intValue = EditorGUILayout.IntField("Frame Key", frameKeyProp.intValue);
            
            GUILayout.Space(20);
            
            GUILayout.Label("Area", labelStyle);
            Vector2 leftBottomPoint = EditorGUILayout.Vector2Field("Left Bottom", leftBottomProp.vector2Value);
            Vector2 rightTopPoint = EditorGUILayout.Vector2Field("Right Top", rightTopProp.vector2Value);
            if (rightTopPoint.x < leftBottomPoint.x)
            {
                leftBottomPoint.x = rightTopPoint.x;
            }

            if (rightTopPoint.y < leftBottomPoint.y)
            {
                leftBottomPoint.y = rightTopPoint.y;
            }

            leftBottomProp.vector2Value = leftBottomPoint;
            rightTopProp.vector2Value = rightTopPoint;
            
            GUILayout.Space(20);
            
            GUILayout.Label("Level Socket", labelStyle);
            for (SocketDirection2D i = 0; i < SocketDirection2D.Count; i++)
            {
                SerializedProperty socketArrayOnDirectionProp = socketArrayByDirectionProp.GetArrayElementAtIndex((int)i);
                SerializedProperty socketArrayProp = socketArrayOnDirectionProp.FindPropertyRelative("array");
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.ToString(), categoryStyle, GUILayout.Width((viewWidth - 6f) * 0.5f));
                if (socketArrayProp.arraySize == 0)
                {
                    if (GUILayout.Button("Add", GUILayout.Width((viewWidth - 6f) * 0.5f)))
                    {
                        socketArrayProp.InsertArrayElementAtIndex(socketArrayProp.arraySize);
                    }
                }

                GUILayout.EndHorizontal();

                int insertIndex = -1;
                int deleteIndex = -1;
                
                for (int j = 0; j < socketArrayProp.arraySize; j++)
                {
                    SerializedProperty socketProp = socketArrayProp.GetArrayElementAtIndex(j);
                    SerializedProperty socketKeyProp = socketProp.FindPropertyRelative("socketKey");
                    SerializedProperty blockObjectProp = socketProp.FindPropertyRelative("blockObject");
                    SerializedProperty localPositionProp = socketProp.FindPropertyRelative("localPosition");
                    SerializedProperty plugArrayProp = socketProp.FindPropertyRelative("plugArray");

                    socketKeyProp.intValue =
                        EditorGUILayout.IntField("Key", socketKeyProp.intValue);
                    blockObjectProp.objectReferenceValue = EditorGUILayout.ObjectField("Block",
                        blockObjectProp.objectReferenceValue, typeof(GameObject), true);
                    localPositionProp.vector2Value =
                        EditorGUILayout.Vector2Field("Local Position", localPositionProp.vector2Value);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Plug Keys", GUILayout.Width((viewWidth - 9f) / 3f));
                    if (GUILayout.Button("Add", GUILayout.Width((viewWidth - 9f) / 3f)))
                    {
                        plugArrayProp.InsertArrayElementAtIndex(plugArrayProp.arraySize);
                    }
                    if (GUILayout.Button("Remove", GUILayout.Width((viewWidth - 9f) / 3f)) && plugArrayProp.arraySize > 0)
                    {
                        plugArrayProp.DeleteArrayElementAtIndex(plugArrayProp.arraySize - 1);
                    }
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    for (int k = 0; k < plugArrayProp.arraySize; k++)
                    {
                        SerializedProperty plugProp = plugArrayProp.GetArrayElementAtIndex(k);

                        plugProp.intValue = EditorGUILayout.IntField(plugProp.intValue,
                            GUILayout.Width((viewWidth - plugArrayProp.arraySize * 3f) / plugArrayProp.arraySize));
                    }
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Insert", GUILayout.Width((viewWidth - 6f) * 0.5f)))
                    {
                        insertIndex = j;
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width((viewWidth - 6f) * 0.5f)))
                    {
                        deleteIndex = j;
                    }
                    
                    GUILayout.EndHorizontal();
                }

                if (insertIndex > -1)
                {
                    socketArrayProp.InsertArrayElementAtIndex(insertIndex);
                }

                if (deleteIndex > -1)
                {
                    socketArrayProp.DeleteArrayElementAtIndex(deleteIndex);
                }
                
                GUILayout.Space(30);
            }
        }
    }
}