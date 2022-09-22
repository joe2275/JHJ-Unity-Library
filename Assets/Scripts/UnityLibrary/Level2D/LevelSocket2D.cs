using System;
using UnityEngine;

namespace Level2D
{
    [Serializable]
    public class LevelSocket2D
    {
        [SerializeField] private int socketKey;
        [SerializeField] private int[] plugArray;
        [SerializeField] private Vector2 localPosition;
        [SerializeField] private GameObject blockObject;

        private bool mCanTryConnect = true;

        /// <summary>
        /// Socket Key 프로퍼티 <br/>
        /// Level Socket의 형태 및 용도 별 구분 번호
        /// </summary>
        public int SocketKey => socketKey;

        /// <summary>
        /// Plug Count 프로퍼티 <br/>
        /// 연결될 수 있는 Level Socket Key 들의 배열
        /// </summary>
        public int PlugCount => plugArray.Length;

        /// <summary>
        /// Local Position 프로퍼티 <br/>
        /// Level Socket의 Level Frame 기준 로컬 좌표
        /// </summary>
        public Vector2 LocalPosition
        {
            get => localPosition;
            set => localPosition = value;
        }
        
        public bool CanTryConnect
        {
            get => mCanTryConnect;
            set => mCanTryConnect = value;
        }

        /// <summary>
        /// Get Plug Key 함수 <br/>
        /// 전달된 index 의 위치에 존재하는 Plug Key 값을 반환 
        /// </summary>
        public int GetPlug(int index)
        {
            return plugArray[index];
        }

        /// <summary>
        /// Block 함수 <br/>
        /// 전달된 block 값에 따라 소켓을 막는 오브젝트를 활성/비활성화
        /// </summary>
        public void Block(bool block)
        {
            blockObject.SetActive(block);
        }
    }
}