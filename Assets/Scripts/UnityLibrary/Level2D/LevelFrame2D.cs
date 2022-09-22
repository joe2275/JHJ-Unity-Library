using System;
using UnityEngine;

namespace Level2D
{
    public class LevelFrame2D : MonoBehaviour
    {
        [SerializeField] private int frameKey;
        
        [SerializeField] private Vector2 leftBottom;
        [SerializeField] private Vector2 rightTop;

        [SerializeField] private LevelSocketArray[] socketArrayByDirection = new LevelSocketArray[(int)SocketDirection2D.Count];
        
        [Serializable]
        private class LevelSocketArray
        {
            [SerializeField] private LevelSocket2D[] array;

            public LevelSocket2D this[int index] => array[index];

            public int Length => array.Length;
        }

        /// <summary>
        /// Frame Key 프로퍼티 <br/>
        /// Level Frame 형태 및 용도 별 구분 번호 
        /// </summary>
        public int FrameKey => frameKey;

        /// <summary>
        /// Left Bottom 프로퍼티 <br/>
        /// Level Frame의 Left Bottom 로컬 좌표
        /// </summary>
        public Vector2 LeftBottom
        {
            get => leftBottom;
            set => leftBottom = value;
        }

        /// <summary>
        /// Right Top 프로퍼티 <br/>
        /// Level Frame의 Right Top 로컬 좌표
        /// </summary>
        public Vector2 RightTop
        {
            get => rightTop;
            set => rightTop = value;
        }

        /// <summary>
        /// Get Socket Count 함수 <br/>
        /// 전달된 direction 방향에 존재하는 Level Socket의 개수를 반환 
        /// </summary>
        public int GetSocketCount(SocketDirection2D direction)
        {
            return socketArrayByDirection[(int)direction].Length;
        }

        /// <summary>
        /// Get Socket 함수 <br/>
        /// 전달된 direction 방향에 존재하는 index에 위치한 Level Socket을 반환
        /// </summary>
        public LevelSocket2D GetSocket(SocketDirection2D direction, int index)
        {
            return socketArrayByDirection[(int)direction][index];
        }
    }
}