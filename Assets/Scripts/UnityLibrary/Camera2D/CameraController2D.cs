using System.Collections.Generic;
using UnityEngine;

namespace Camera2D
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class CameraController2D : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private float smoothness = 10.0f;
        [SerializeField] private Transform follow;
        [SerializeField] private float followZoneHalfWidth = 12f;
        [SerializeField] private float followZoneHalfHeight = 8f;
        [SerializeField] private float followInertiaDistance = 3f;
        [SerializeField] private float followInertiaSpeed = 3f;

        private bool mIsFollow;
        private float mFollowPrevPositionX;
        private float mHorizontalInertia;
        private Vector2 mDestination;
        private readonly List<CameraBoundary2D> mTriggeredBoundaryList = new List<CameraBoundary2D>();

        public Vector2 Destination
        {
            get => mDestination;
            set => mDestination = value;
        }

        /// <summary>
        /// World Camera 프로퍼티
        /// </summary>
        public Camera WorldCamera => worldCamera;

        /// <summary>
        /// UI Camera 프로퍼티
        /// </summary>
        public Camera UICamera => uiCamera;

        /// <summary>
        /// Smoothness 프로퍼티 <br/>
        /// 카메라가 목표지점으로 이동할 때의 부드러움
        /// </summary>
        public float Smoothness
        {
            get => smoothness;
            set => smoothness = Mathf.Max(value, 1.0f);
        }

        /// <summary>
        /// Follow 프로퍼티 <br/>
        /// 카메라가 따라다녀야 할 대상의 Transform
        /// </summary>
        public Transform Follow
        {
            get => follow;
            set
            {
                if (value is null)
                {
                    mIsFollow = false;
                    return;
                }

                mIsFollow = true;
                follow = value;
                mFollowPrevPositionX = follow.position.x;
                mHorizontalInertia = 0.0f;
            }
        }

        /// <summary>
        /// Follow Zone Half Width 프로퍼티 <br/>
        /// Follow 대상이 존재해야 하는 영역의 가로 길이의 절반
        /// </summary>
        public float FollowZoneHalfWidth
        
        {
            get => followZoneHalfWidth;
            set => followZoneHalfWidth = Mathf.Max(value, 0.0f);
        }

        /// <summary>
        /// Follow Zone Half Height 프로퍼티 <br/>
        /// Follow 대상이 존재해야 하는 영역의 세로 길이의 절반
        /// </summary>
        public float FollowZoneHalfHeight
        {
            get => followZoneHalfHeight;
            set => followZoneHalfHeight = Mathf.Max(value, 0.0f);
        }

        /// <summary>
        /// Follow Inertia Distance 프로퍼티 <br/>
        /// Follow 대상의 움직임에 따라 이동하는 카메라 중심점의 가로 길이의 절반
        /// </summary>
        public float FollowInertiaDistance
        {
            get => followInertiaDistance;
            set => followInertiaDistance = Mathf.Max(value, 0f);
        }

        /// <summary>
        /// Follow Inertia Speed 프로퍼티 <br/>
        /// Follow 대상의 움직임에 따라 이동하는 카메라 중심점의 속도
        /// </summary>
        public float FollowInertiaSpeed
        {
            get => followInertiaSpeed;
            set => followInertiaSpeed = Mathf.Max(value, 0.0f);
        }

        /// <summary>
        /// Inertia 프로퍼티 <br/>
        /// Follow 대상 기준 카메라 중심점 위치 (X좌표 Offset)
        /// </summary>
        public float HorizontalInertia => mHorizontalInertia;

        /// <summary>
        /// Vertical Size 프로퍼티 <br/>
        /// World Camera의 수직 크기의 반
        /// </summary>
        public float HalfHeight => worldCamera.orthographicSize;

        /// <summary>
        /// Horizontal Size 프로퍼티 <br/>
        /// World Camera의 수평 크기의 반
        /// </summary>
        public float HalfWidth => HalfHeight * worldCamera.pixelWidth / worldCamera.pixelHeight;

        private void Awake()
        {
            InitFields();
        }

        private void FixedUpdate()
        {
            Vector2 destination, nextPosition;
            
            if (mIsFollow)
            {
                destination = FilterInertia(follow.position);
                destination = FilterBoundaryLine(destination);
                nextPosition = GetNextPosition(destination);
                nextPosition = FilterFollowZone(destination, nextPosition);
                
            }
            else
            {
                destination = FilterBoundaryLine(Destination);
                nextPosition = GetNextPosition(destination);
                
            }
            
            Transform myTransform = transform;
            Vector3 position = myTransform.position;
            myTransform.position = new Vector3(nextPosition.x, nextPosition.y, position.z);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            CameraBoundary2D boundary2D = col.GetComponent<CameraBoundary2D>();
            if (boundary2D is null) return;

            mTriggeredBoundaryList.Add(boundary2D);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            CameraBoundary2D boundary2D = other.GetComponent<CameraBoundary2D>();
            mTriggeredBoundaryList.Remove(boundary2D);
        }

        private void InitFields()
        {
            if (follow != null)
            {
                mIsFollow = true;
                mFollowPrevPositionX = follow.position.x;
            }
        }

        private Vector2 FilterInertia(Vector2 followPosition)
        {
            float differenceX = followPosition.x - mFollowPrevPositionX;
            mFollowPrevPositionX = followPosition.x;

            if (differenceX > Mathf.Epsilon)
            {
                if (mHorizontalInertia < -Mathf.Epsilon)
                {
                    mHorizontalInertia = 0.0f;
                }
                else
                {
                    mHorizontalInertia = Mathf.Min(mHorizontalInertia + differenceX * followInertiaSpeed * Time.fixedDeltaTime,
                        followInertiaDistance);
                }
            }
            else if (differenceX < -Mathf.Epsilon)
            {
                if (mHorizontalInertia > Mathf.Epsilon)
                {
                    mHorizontalInertia = 0.0f;
                }
                else
                {
                    mHorizontalInertia = Mathf.Max(mHorizontalInertia + differenceX * followInertiaSpeed * Time.fixedDeltaTime,
                        -followInertiaDistance);
                }
            }

            return new Vector2(followPosition.x + mHorizontalInertia, followPosition.y);
        }

        private Vector2 GetNextPosition(Vector2 destination)
        {
            Vector2 nextPosition = transform.position;
            nextPosition += (destination - nextPosition) / smoothness;
            
            Vector2 zoneMin = new Vector2(destination.x - followZoneHalfWidth, destination.y - followZoneHalfHeight);
            Vector2 zoneMax = new Vector2(destination.x + followZoneHalfWidth, destination.y + followZoneHalfHeight);

            if (nextPosition.x < zoneMin.x)
            {
                nextPosition.x = zoneMin.x;
            }
            else if (nextPosition.x > zoneMax.x)
            {
                nextPosition.x = zoneMax.x;
            }

            if (nextPosition.y < zoneMin.y)
            {
                nextPosition.y = zoneMin.y;
            }
            else if (nextPosition.y > zoneMax.y)
            {
                nextPosition.y = zoneMax.y;
            }

            return nextPosition;
        }

        private Vector2 FilterFollowZone(Vector2 destination, Vector2 nextPosition)
        {
            Vector2 zoneMin = new Vector2(destination.x - followZoneHalfWidth, destination.y - followZoneHalfHeight);
            Vector2 zoneMax = new Vector2(destination.x + followZoneHalfWidth, destination.y + followZoneHalfHeight);

            if (nextPosition.x < zoneMin.x)
            {
                nextPosition.x = zoneMin.x;
            }
            else if (nextPosition.x > zoneMax.x)
            {
                nextPosition.x = zoneMax.x;
            }

            if (nextPosition.y < zoneMin.y)
            {
                nextPosition.y = zoneMin.y;
            }
            else if (nextPosition.y > zoneMax.y)
            {
                nextPosition.y = zoneMax.y;
            }

            return nextPosition;
        }

        private Vector2 FilterBoundaryLine(Vector2 destination)
        {
            float verticalHalfSize = HalfHeight;
            bool bottomOverlapped = false;
            bool topOverlapped = false;
            float bottomY = destination.y - verticalHalfSize;
            float topY = destination.y + verticalHalfSize;
            float horizontalHalfSize = HalfWidth;
            bool leftOverlapped = false;
            bool rightOverlapped = false;
            float leftX = destination.x - horizontalHalfSize;
            float rightX = destination.x + horizontalHalfSize;

            foreach (CameraBoundary2D boundary in mTriggeredBoundaryList)
            {
                if (boundary.OverlapHorizontal(destination, HalfWidth))
                {
                    float correctedX = boundary.GetHorizontalLineX(destination.y);
                    if (correctedX < destination.x)
                    {
                        if (leftX < correctedX)
                        {
                            leftOverlapped = true;
                            leftX = correctedX;
                        }
                    }
                    else
                    {
                        if (rightX > correctedX)
                        {
                            rightOverlapped = true;
                            rightX = correctedX;
                        }
                    }
                }
                
                if (boundary.OverlapVerticalLine(destination, HalfHeight))
                {
                    float correctedY = boundary.GetVerticalLineY(destination.x);
                    if (correctedY < destination.y)
                    {
                        if (bottomY < correctedY)
                        {
                            bottomOverlapped = true;
                            bottomY = correctedY;
                        }
                    }
                    else
                    {
                        if (topY > correctedY)
                        {
                            topOverlapped = true;
                            topY = correctedY;
                        }
                    }
                }
            }

            if (leftOverlapped)
            {
                if (rightOverlapped)
                {
                    destination.x = (leftX + rightX) * 0.5f;
                }
                else
                {
                    destination.x = leftX + horizontalHalfSize;
                }
            }
            else
            {
                if (rightOverlapped)
                {
                    destination.x = rightX - horizontalHalfSize;
                }
            }

            if (bottomOverlapped)
            {
                if (topOverlapped)
                {
                    destination.y = (bottomY + topY) * 0.5f;
                }
                else
                {
                    destination.y = bottomY + verticalHalfSize;
                }
            }
            else
            {
                if (topOverlapped)
                {
                    destination.y = topY - verticalHalfSize;
                }
            }

            return destination;
        }
    }
}