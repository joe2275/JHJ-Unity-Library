using UnityEngine;

namespace Level2D
{
    public class LevelLayer2D : MonoBehaviour
    {
        [SerializeField] private int frameKey;
        [SerializeField] private Vector2 leftBottom;

        /// <summary>
        /// Frame Key 프로퍼티 <br/>
        /// Level Layer가 배치될 수 있는 Level Frame 키 값
        /// </summary>
        public int FrameKey => frameKey;

        /// <summary>
        /// Left Bottom Point 프로퍼티 <br/>
        /// Level Layer의 좌측 하단 로컬 좌표
        /// </summary>
        public Vector2 LeftBottom
        {
            get => leftBottom;
            set => leftBottom = value;
        }
    }
}