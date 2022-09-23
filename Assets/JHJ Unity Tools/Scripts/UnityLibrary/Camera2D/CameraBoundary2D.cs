using UnityEngine;

namespace Camera2D
{
    public class CameraBoundary2D : MonoBehaviour
    {
        [SerializeField] private Vector2 start;
        [SerializeField] private Vector2 end;

        /// <summary>
        /// Start Point 프로퍼티 <br/>
        /// Camera Boundary Line의 Start Point
        /// </summary>
        public Vector2 StartPoint
        {
            get => start;
            set
            {
                start = value;
                UpdateColliderProperties();
            }
        }

        /// <summary>
        /// End Point 프로퍼티 <br/>
        /// Camera Boundary Line의 End Point
        /// </summary>
        public Vector2 EndPoint
        {
            get => end;
            set
            {
                end = value;
                UpdateColliderProperties();
            }
        }
        
        private CircleCollider2D mCollider;
        private bool mExistCollider;

        /// <summary>
        /// Overlap Vertical Line 함수
        /// 카메라의 center 좌표 기준으로 수직 크기인 vertical Half Size를 이용해서
        /// Camera Boundary 와 겹치는지를 판단하여 결과를 반환
        /// </summary>
        public bool OverlapVerticalLine(Vector2 center, float verticalHalfSize)
        {
            Vector2 position = transform.position;
            Vector2 startPosition = position + start;
            Vector2 endPosition = position + end;

            float minY, maxY;

            if (start.y < end.y)
            {
                minY = startPosition.y;
                maxY = endPosition.y;
            }
            else
            {
                minY = endPosition.y;
                maxY = startPosition.y;
            }

            bool horizontalOverlap = (center.x - startPosition.x) * (center.x - endPosition.x) < Mathf.Epsilon;
            bool verticalOverlap = (center.y - verticalHalfSize - maxY) * (center.y + verticalHalfSize - minY) <
                                   Mathf.Epsilon;
            return horizontalOverlap && verticalOverlap;
        }

        /// <summary>
        /// Overlap Horizontal Line 함수
        /// 카메라의 center 좌표 기준으로 수평 크기인 horizontal Half Size를 이용해서
        /// Camera Boundary 와 겹치는지를 판단하여 결과를 반환
        /// </summary>
        public bool OverlapHorizontal(Vector2 center, float horizontalHalfSize)
        {
            Vector2 position = transform.position;
            Vector2 startPosition = position + start;
            Vector2 endPosition = position + end;

            float minX, maxX;

            if (start.x < end.x)
            {
                minX = startPosition.x;
                maxX = endPosition.x;
            }
            else
            {
                minX = endPosition.x;
                maxX = startPosition.x;
            }

            bool verticalOverlap = (center.y - startPosition.y) * (center.y - endPosition.y) < Mathf.Epsilon;
            bool horizontalOverlap = (center.x - horizontalHalfSize - maxX) * (center.x + horizontalHalfSize - minX) <
                                     Mathf.Epsilon;
            return verticalOverlap && horizontalOverlap;
        }

        /// <summary>
        /// Get Horizontal Line X 함수
        /// 만약 카메라의 Horizontal Line 이 겹친다면, 겹쳐진 좌표의 x 값을 반환
        /// </summary>
        public float GetHorizontalLineX(float horizontalLineY)
        {
            Vector2 position = transform.position;
            Vector2 startPosition = position + start;
            Vector2 endPosition = position + end;

            return (horizontalLineY - startPosition.y) * (endPosition.x - startPosition.x) / (endPosition.y - startPosition.y) +
                   startPosition.x;
        }

        /// <summary>
        /// Get Vertical Line Y 함수
        /// 만약 카메라의 Vertical Line 이 겹친다면, 겹쳐진 좌표의 y 값을 반환  
        /// </summary>
        public float GetVerticalLineY(float verticalLineX)
        {
            Vector2 position = transform.position;
            Vector2 startPosition = position + start;
            Vector2 endPosition = position + end;

            return (verticalLineX - startPosition.x) * (endPosition.y - startPosition.y) / (endPosition.x - startPosition.x) +
                   startPosition.y;
        }
        
        private void Awake()
        {
            InitCircleCollider();
            UpdateColliderProperties();
        }

        private void InitCircleCollider()
        {
            mCollider = gameObject.AddComponent<CircleCollider2D>();
            mExistCollider = true;
        }

        private void UpdateColliderProperties()
        {
            if (!mExistCollider)
            {
                return;
            }
            
            Vector2 offset = new Vector2((start.x + end.x) * 0.5f, (start.y + end.y) * 0.5f);
            float diffX = start.x - end.x;
            float diffY = start.y - end.y;
            float radius = Mathf.Sqrt(diffX * diffX + diffY * diffY) * 0.5f;
            mCollider.offset = offset;
            mCollider.radius = radius;
        }
    }
}
