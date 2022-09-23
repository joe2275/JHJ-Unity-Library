using UnityEngine;

namespace Background2D
{
    public class ParallaxBackground2D : MonoBehaviour
    {
        [Header("World")]
        [SerializeField] private Vector2 leftBottom;
        [SerializeField] private Vector2 rightTop;
        [Header("Target")]
        [SerializeField] private Camera targetCamera;

        private bool mExistTarget;
        private ParallaxLayer2D[] mLayers;
        
        /// <summary>
        /// Left Bottom 프로퍼티 <br/>
        /// 게임 월드의 좌측 하단 월드 좌표
        /// </summary>
        public Vector2 LeftBottom
        {
            get => leftBottom;
            set => leftBottom = value;
        }

        /// <summary>
        /// Right Top 프로퍼티 <br/>
        /// 게임 월드의 우측 상단 월드 좌표
        /// </summary>
        public Vector2 RightTop
        {
            get => rightTop;
            set => rightTop = value;
        }

        /// <summary>
        /// Target Camera 프로퍼티 <br/>
        /// Parallax Background 를 모여줄 대상 (따라다닐 대상)으로
        /// 주로 카메라를 등록해야 한다. 
        /// </summary>
        public Camera TargetCamera
        {
            get => targetCamera;
            set
            {
                if (value is null)
                {
                    mExistTarget = false;
                    return;
                }

                mExistTarget = true;
                targetCamera = value;
            }
        }

        private Vector2 WorldSize => rightTop - leftBottom;

        private void Awake()
        {
            mLayers = GetComponentsInChildren<ParallaxLayer2D>(true);
            if (targetCamera != null)
            {
                mExistTarget = true;
            }
        }

        private void FixedUpdate()
        {
            if (!mExistTarget)
            {
                return; 
            }
            
            UpdateParallaxLayers();
        }

        private void UpdateParallaxLayers()
        {
            Vector2 targetPosition = targetCamera.transform.position;
            Vector2 cameraHalfSize =
                new Vector2(targetCamera.orthographicSize * targetCamera.pixelWidth / targetCamera.pixelHeight,
                    targetCamera.orthographicSize);
            Vector2 size = WorldSize - cameraHalfSize * 2.0f;
            Vector2 pivot = (targetPosition - leftBottom - cameraHalfSize) / size;

            foreach (ParallaxLayer2D layer in mLayers)
            {
                Transform layerTransform = layer.transform;
                Vector2 layerLeftBottomPoint = layer.LeftBottomPoint + cameraHalfSize;
                Vector2 layerSize = layer.RightTopPoint - layerLeftBottomPoint - cameraHalfSize;
                
                layerTransform.position = targetPosition - pivot * layerSize - layerLeftBottomPoint;
            }
        }
    }
}