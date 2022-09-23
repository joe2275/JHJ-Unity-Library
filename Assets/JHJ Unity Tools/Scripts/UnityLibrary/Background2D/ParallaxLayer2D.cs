using UnityEngine;

namespace Background2D
{
    public class ParallaxLayer2D : MonoBehaviour
    {
        [Header("Layer Area")]
        [SerializeField] private Vector2 leftBottomPoint;
        [SerializeField] private Vector2 rightTopPoint;

        /// <summary>
        /// Left Bottom Point 프로퍼티 <br/>
        /// 이미지의 왼쪽 하단 로컬 좌표
        /// </summary>
        public Vector2 LeftBottomPoint
        {
            get => leftBottomPoint;
            set => leftBottomPoint = value;
        }

        /// <summary>
        /// Right Top Point 프로퍼티 <br/>
        /// 이미지의 오른쪽 상단 로컬 좌표
        /// </summary>
        public Vector2 RightTopPoint
        {
            get => rightTopPoint;
            set => rightTopPoint = value;
        }
    }
}
