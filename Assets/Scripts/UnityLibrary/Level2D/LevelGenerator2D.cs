using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level2D
{
    /// <summary>
    /// Level Generator 클래스 <br/>
    /// Level Frame - Terrain이 배치된 오브젝트
    /// Level Layer - Frame 위에 배치될 오브젝트
    /// Generate Level 함수를 통해서 전달된 소켓 좌표를 기준으로 적절한 레벨을 생성한다.
    /// </summary>
    public class LevelGenerator2D : MonoBehaviour
    {
        [SerializeField] private bool isLimited;
        [SerializeField] private Vector2 leftBottom;
        [SerializeField] private Vector2 rightTop;
        
        [SerializeField] private LevelFrame2D[] frameArray;
        [SerializeField] private LevelLayerInfo[] layerInfoArray;

        [SerializeField] private float[] frameWeightArray;
        [SerializeField] private int[] frameCountArray;

        [Serializable]
        private class LevelLayerInfo
        {
            [SerializeField] private LevelLayer2D[] layerArray;
            [SerializeField] private float[] layerWeightArray;
            [SerializeField] private int[] layerCountArray;
            
            public LevelLayer2D[] LayerArray => layerArray;
            public float[] LayerWeightArray => layerWeightArray;
            public int[] LayerCountArray => layerCountArray;
        }
        
        private class SocketInFrame
        {
            public SocketInFrame(LevelFrame2D levelFrame, int frameIndex, LevelSocket2D levelSocket, SocketDirection2D socketDirection, int socketIndex)
            {
                Frame = levelFrame;
                FrameIndex = frameIndex;
                Socket = levelSocket;
                SocketDirection = socketDirection;
                SocketIndex = socketIndex;
            }

            public LevelFrame2D Frame { get; }
            public int FrameIndex { get; }
            public LevelSocket2D Socket { get; }
            public SocketDirection2D SocketDirection { get; }
            public int SocketIndex { get; }
        }

        private LevelCondition2D[] mFrameConditionArray;
        private LevelCondition2D[][] mLayerConditionArray;
        private int[] mFrameCountArray;
        private int[][] mLayerCountArray;

        private readonly List<LevelFrame2D> mGeneratedFrameList = new List<LevelFrame2D>();

        private readonly Dictionary<int, SocketInFrame[]>[] mSocketInFrameMapArray =
            new Dictionary<int, SocketInFrame[]>[(int)SocketDirection2D.Count];
        private Dictionary<int, int[]>[] mLayerIndexByFrameKeyMapArray;

        private readonly List<SocketInFrame> mConsideredSocketInFrameList = new List<SocketInFrame>();
        private readonly List<int> mConsideredLayerIndexList = new List<int>();
        private int mLayerOrder;
        private float mWeightSum;

        /// <summary>
        /// Is Limited 프로퍼티 <\br>
        /// 범위가 정해진 레벨 생성기인지 반환하는 프로퍼티
        /// </summary>
        public bool IsLimited => isLimited;

        /// <summary>
        /// Left Bottom 프로퍼티 <\br>
        /// 생성된 전체 레벨의 Local Left Bottom 좌표
        /// </summary>
        public Vector2 LeftBottom
        {
            get => leftBottom;
            set => leftBottom = value;
        }

        /// <summary>
        /// Right Top 프로퍼티 <\br>
        /// 생성된 전체 레벨의 Local Right Top 좌표
        /// </summary>
        public Vector2 RightTop
        {
            get => rightTop;
            set => rightTop = value;
        }

        /// <summary>
        /// Add Other Frame 함수 <br/>
        /// Level Generator에 입력되지 않은 Leve Frame을 입력하여
        /// Level 생성 전 Level 간 겹침을 방지하기 위해 반드시 선언해야 하는 함수 
        /// </summary>
        public void AddOtherFrame(LevelFrame2D generatedFrame)
        {
            mGeneratedFrameList.Add(generatedFrame);
        }

        /// <summary>
        /// Generate Level 함수 <br/>
        /// 전달된 direction 방향에 socketPosition 위치에 존재하는 socket을 기준으로 새로운 레벨을 하나 생성하는 함수로, 생성된 Level Frame을 반환
        /// </summary>
        public LevelFrame2D GenerateLevel(SocketDirection2D direction, LevelSocket2D socket, Vector2 socketPosition)
        {
            if (!socket.CanTryConnect)
            {
                return null;
            }
            socket.CanTryConnect = false;
            
            SocketDirection2D oppositeDirection = GetOppositeDirection(direction);

            ConsiderSocketInFrame(oppositeDirection, socket, socketPosition);
            SocketInFrame selectedSocketInFrame = SelectSocketInFrame();
            if (selectedSocketInFrame is null)
            {
                socket.Block(true);
                return null;
            }
            socket.Block(false);

            LevelFrame2D generatedFrame = GenerateFrame(selectedSocketInFrame, socketPosition);

            for (mLayerOrder = 0; mLayerOrder < mLayerIndexByFrameKeyMapArray.Length; mLayerOrder++)
            {
                ConsiderLayer(generatedFrame);
                int selectedLayerIndex = SelectLayer();
                if (selectedLayerIndex < 0)
                {
                    continue;
                }
                GenerateLayer(selectedLayerIndex, generatedFrame);
            }

            return generatedFrame;
        }

        private void Awake()
        {
            InitializeFields();
            InitializeLevelFrame();
            InitializeLevelLayer();
        }

        private void ConsiderSocketInFrame(SocketDirection2D oppositeDirection, LevelSocket2D socket,
            Vector2 socketPosition)
        {
            mWeightSum = 0f;

            Dictionary<int, SocketInFrame[]> socketInFrameMap = mSocketInFrameMapArray[(int)oppositeDirection];

            for (int i = 0; i < socket.PlugCount; i++)
            {
                int plug = socket.GetPlug(i);
                SocketInFrame[] socketInFrameArray = socketInFrameMap[plug];
                foreach (SocketInFrame socketInFrame in socketInFrameArray)
                {
                    int frameIndex = socketInFrame.FrameIndex;

                    if (mFrameConditionArray[frameIndex] is not null)
                    {
                        if (!mFrameConditionArray[frameIndex].CanGenerate())
                        {
                            continue;
                        }
                    }
                    
                    int frameCount = mFrameCountArray[frameIndex];

                    if (frameCount == 0)
                    {
                        continue;
                    }

                    LevelFrame2D consideredFrame = socketInFrame.Frame;
                    LevelSocket2D consideredSocket = socketInFrame.Socket;

                    Vector2 consideredFramePosition = socketPosition - consideredSocket.LocalPosition;
                    Vector2 consideredFrameLeftBottomPosition = consideredFramePosition + consideredFrame.LeftBottom;
                    Vector2 consideredFrameRightTopPosition = consideredFramePosition + consideredFrame.RightTop;

                    if (!CanGenerateFrame(consideredFrameLeftBottomPosition, consideredFrameRightTopPosition))
                    {
                        continue;
                    }

                    if (frameCount > 0)
                    {
                        mWeightSum += frameWeightArray[frameIndex] * frameCount / frameCountArray[frameIndex];
                    }
                    else
                    {
                        // frameCount가 음수일 경우 무제한 생성 가능
                        mWeightSum += frameWeightArray[frameIndex];
                    }
                    
                    mConsideredSocketInFrameList.Add(socketInFrame);
                }
            }
        }

        private SocketInFrame SelectSocketInFrame()
        {
            SocketInFrame selectedSocketInFrame = null;
            float choice = Random.Range(0f, mWeightSum);
            foreach (SocketInFrame socketInFrame in mConsideredSocketInFrameList)
            {
                int frameIndex = socketInFrame.FrameIndex;
                int frameCount = mFrameCountArray[frameIndex];

                float weight;
                if (frameCount > 0)
                {
                    weight = frameWeightArray[frameIndex] * frameCount / frameCountArray[frameIndex];
                }
                else
                {
                    weight = frameWeightArray[frameIndex];
                }

                if (choice > weight)
                {
                    choice -= weight;
                    continue;
                }

                selectedSocketInFrame = socketInFrame;
                break;
            }
            
            mConsideredSocketInFrameList.Clear();
            return selectedSocketInFrame;
        }

        private LevelFrame2D GenerateFrame(SocketInFrame socketInFrame, Vector2 socketPosition)
        {
            LevelFrame2D generatedFrame = Instantiate(socketInFrame.Frame, transform);
            generatedFrame.transform.position = socketPosition - socketInFrame.Socket.LocalPosition;
            
            mGeneratedFrameList.Add(generatedFrame);
            mFrameCountArray[socketInFrame.FrameIndex]--;
            generatedFrame.GetSocket(socketInFrame.SocketDirection, socketInFrame.SocketIndex).CanTryConnect = false;

            return generatedFrame;
        }

        private void ConsiderLayer(LevelFrame2D frame)
        {
            mWeightSum = 0f;

            Dictionary<int, int[]> layerIndexByFrameKeyMap = mLayerIndexByFrameKeyMapArray[mLayerOrder];
            int[] layerIndexArray = layerIndexByFrameKeyMap[frame.FrameKey];
            LevelLayerInfo layerInfo = layerInfoArray[mLayerOrder];

            foreach (int layerIndex in layerIndexArray)
            {
                if (mLayerConditionArray[mLayerOrder][layerIndex] is not null)
                {
                    if (!mLayerConditionArray[mLayerOrder][layerIndex].CanGenerate())
                    {
                        continue;
                    }
                }
                
                int layerCount = mLayerCountArray[mLayerOrder][layerIndex];

                if (layerCount == 0)
                {
                    continue;
                }

                if (layerCount > 0)
                {
                    mWeightSum += layerInfo.LayerWeightArray[layerIndex] * layerCount /
                                  layerInfo.LayerCountArray[layerIndex];
                }
                else
                {
                    mWeightSum += layerInfo.LayerWeightArray[layerIndex];
                }
                
                mConsideredLayerIndexList.Add(layerIndex);
            }
        }

        private int SelectLayer()
        {
            LevelLayerInfo layerInfo = layerInfoArray[mLayerOrder];
            int selectedLayerIndex = -1;
            float choice = Random.Range(0f, mWeightSum);

            foreach (int layerIndex in mConsideredLayerIndexList)
            {
                int layerCount = mLayerCountArray[mLayerOrder][layerIndex];

                float weight;
                if (layerCount > 0)
                {
                    weight = layerInfo.LayerWeightArray[layerIndex] * layerCount /
                             layerInfo.LayerCountArray[layerIndex];
                }
                else
                {
                    weight = layerInfo.LayerWeightArray[layerIndex];
                }

                if (choice > weight)
                {
                    choice -= weight;
                    continue;
                }

                selectedLayerIndex = layerIndex;
                break;
            }

            mConsideredLayerIndexList.Clear();
            return selectedLayerIndex;
        }

        private void GenerateLayer(int layerIndex, LevelFrame2D frame)
        {
            LevelLayer2D generatedLayer = Instantiate(layerInfoArray[mLayerOrder].LayerArray[layerIndex], frame.transform);
            Vector2 framePosition = frame.transform.position;
            generatedLayer.transform.position = framePosition + frame.LeftBottom - generatedLayer.LeftBottom;
        }
        
        private SocketDirection2D GetOppositeDirection(SocketDirection2D direction)
        {
            switch (direction)
            {
                case SocketDirection2D.Right:
                    return SocketDirection2D.Left;
                case SocketDirection2D.Left:
                    return SocketDirection2D.Right;
                case SocketDirection2D.Bottom:
                    return SocketDirection2D.Top;
                default:
                    return SocketDirection2D.Bottom;
            }
        }
        
        private bool CanGenerateFrame(Vector2 frameLeftBottom, Vector2 frameRightTop)
        {
            if (isLimited)
            {
                Vector2 position = transform.position;
                Vector2 leftBottomPosition = position + leftBottom;
                Vector2 rightTopPosition = position + rightTop;

                if (frameLeftBottom.x < leftBottomPosition.x || frameLeftBottom.y < leftBottomPosition.y ||
                    frameRightTop.x > rightTopPosition.x || frameRightTop.y > rightTopPosition.y)
                {
                    return false;
                }
            }

            foreach (LevelFrame2D generatedFrame in mGeneratedFrameList)
            {
                Vector2 generatedPosition = generatedFrame.transform.position;
                Vector2 generatedLeftBottom = generatedPosition + generatedFrame.LeftBottom;
                Vector2 generatedRightTop = generatedPosition + generatedFrame.RightTop;

                float overlapX = (frameRightTop.x - generatedLeftBottom.x) *
                                 (frameLeftBottom.x - generatedRightTop.x);
                float overlapY = (frameRightTop.y - generatedLeftBottom.y) *
                                 (frameLeftBottom.y - generatedRightTop.y);

                if (overlapX < -Mathf.Epsilon && overlapY < -Mathf.Epsilon)
                {
                    return false;
                }
            }

            return true;
        }
        
        private void InitializeFields()
        {
            mFrameCountArray = frameCountArray.Clone() as int[];
            mFrameConditionArray = new LevelCondition2D[frameArray.Length];
            for (int i = 0; i < (int)SocketDirection2D.Count; i++)
            {
                mSocketInFrameMapArray[i] = new Dictionary<int, SocketInFrame[]>();
            }

            for (int i = 0; i < frameArray.Length; i++)
            {
                mFrameConditionArray[i] = frameArray[i].GetComponent<LevelCondition2D>();
            }

            mLayerCountArray = new int[layerInfoArray.Length][];
            mLayerConditionArray = new LevelCondition2D[layerInfoArray.Length][];
            mLayerIndexByFrameKeyMapArray = new Dictionary<int, int[]>[layerInfoArray.Length];
            for (int i = 0; i < layerInfoArray.Length; i++)
            {
                LevelLayerInfo layerInfo = layerInfoArray[i];
                mLayerCountArray[i] = layerInfo.LayerCountArray.Clone() as int[];
                mLayerConditionArray[i] = new LevelCondition2D[layerInfo.LayerArray.Length];
                mLayerIndexByFrameKeyMapArray[i] = new Dictionary<int, int[]>();

                for (int j = 0; j < layerInfo.LayerArray.Length; j++)
                {
                    mLayerConditionArray[i][j] = layerInfo.LayerArray[j].GetComponent<LevelCondition2D>();
                }
            }
        }
        
        private void InitializeLevelFrame()
        {
            Dictionary<int, List<SocketInFrame>>[] socketInFrameMapArray =
                new Dictionary<int, List<SocketInFrame>>[(int)SocketDirection2D.Count];

            for (int i = 0; i < (int)SocketDirection2D.Count; i++)
            {
                socketInFrameMapArray[i] = new Dictionary<int, List<SocketInFrame>>();
            }

            for (int i = 0; i < frameArray.Length; i++)
            {
                LevelFrame2D frame = frameArray[i];

                for (int j = 0; j < (int)SocketDirection2D.Count; j++)
                {
                    SocketDirection2D socketDirection = (SocketDirection2D)j;
                    Dictionary<int, List<SocketInFrame>> socketInFrameMap = socketInFrameMapArray[j];

                    for (int k = 0; k < frame.GetSocketCount(socketDirection); k++)
                    {
                        LevelSocket2D socket = frame.GetSocket(socketDirection, k);
                        int socketKey = socket.SocketKey;

                        if (!socketInFrameMap.ContainsKey(socketKey))
                        {
                            socketInFrameMap.Add(socketKey, new List<SocketInFrame>());
                        }

                        SocketInFrame socketInFrame = new SocketInFrame(frame, i, socket, socketDirection, k);
                        socketInFrameMap[socketKey].Add(socketInFrame);
                    }
                }
            }

            for (int i = 0; i < (int)SocketDirection2D.Count; i++)
            {
                Dictionary<int, SocketInFrame[]> mSocketInFrameMap = mSocketInFrameMapArray[i];
                Dictionary<int, List<SocketInFrame>> socketInFrameMap = socketInFrameMapArray[i];

                foreach (KeyValuePair<int,List<SocketInFrame>> keyValuePair in socketInFrameMap)
                {
                    mSocketInFrameMap.Add(keyValuePair.Key, keyValuePair.Value.ToArray());
                }
            }
        }
        
        private void InitializeLevelLayer()
        {
            Dictionary<int, List<int>> layerIndexByFrameKeyMap = new Dictionary<int, List<int>>();

            for (int i = 0; i < layerInfoArray.Length; i++)
            {
                LevelLayer2D[] layerArray = layerInfoArray[i].LayerArray;

                for (int j = 0; j < layerArray.Length; j++)
                {
                    LevelLayer2D layer = layerArray[j];
                    int frameKey = layer.FrameKey;

                    if (!layerIndexByFrameKeyMap.ContainsKey(frameKey))
                    {
                        layerIndexByFrameKeyMap.Add(frameKey, new List<int>());
                    }

                    layerIndexByFrameKeyMap[frameKey].Add(j);
                }

                foreach (KeyValuePair<int,List<int>> keyValuePair in layerIndexByFrameKeyMap)
                {
                    mLayerIndexByFrameKeyMapArray[i].Add(keyValuePair.Key, keyValuePair.Value.ToArray());
                }

                layerIndexByFrameKeyMap.Clear();
            }
        }
    }
}