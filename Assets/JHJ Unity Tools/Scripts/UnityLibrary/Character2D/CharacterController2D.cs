using UnityEngine;

namespace Character2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private bool isFlying;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 50f;
        [SerializeField] private float accelerationInAir = 10f;
        [SerializeField] private float jumpPower = 8f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundRadius = 0.1f;
        [SerializeField] private float groundHeight = 0.2f;
        [SerializeField] private float slopeRadius = 0.5f;
        [SerializeField] private float slopeUpHeight = 0.5f;
        [SerializeField] private float slopeDownHeight = 1.5f;

        [Header("Direction")] [SerializeField] private Transform body;
        [SerializeField] private Transform center;
        [SerializeField] private Transform feet;
        [SerializeField] private Transform forward;
        [SerializeField] private Transform up;

        private Rigidbody2D mRigidbody;
        private bool mIsGrounded;

        private Vector2 mMovement;
        private bool mJump;
        private bool mCanJump = true;
        private float mJumpPowerTimes;

        private bool mDragChanged;
        private float mDrag;

        /// <summary>
        /// Body 프로퍼티 <br/>
        /// Renderer, Collider 등이 들어있는 상위 오브젝트의 트랜트폼
        /// </summary>
        public Transform Body => body;
        /// <summary>
        /// Center Position 프로퍼티 <br/>
        /// Body 의 중심 월드 좌표
        /// </summary>
        public Vector2 CenterPosition => center.position;
        /// <summary>
        /// Feet Position 프로퍼티 <br/>
        /// Body의 발 위치의 월드 좌표
        /// </summary>
        public Vector2 FeetPosition => feet.position;
        /// <summary>
        /// Forward 프로퍼티 <br/>
        /// 월드 기준 Forward 벡터
        /// </summary>
        public Vector2 Forward => forward.position - center.position;
        /// <summary>
        /// Up 프로퍼티 <br/>
        /// 월드 기준 Up 벡터
        /// </summary>
        public Vector2 Up => up.position - center.position;

        /// <summary>
        /// Max Speed 프로퍼티 <br/>
        /// 최대 속도
        /// </summary>
        public float MaxSpeed
        {
            get => maxSpeed;
            set => maxSpeed = value;
        }

        /// <summary>
        /// Acceleration 프로퍼티 <br/>
        /// 가속도
        /// </summary>
        public float Acceleration
        {
            get => acceleration;
            set => acceleration = value;
        }

        /// <summary>
        /// Acceleration In Air 프로퍼티 <br/>
        /// 공중에서 가속도
        /// </summary>
        public float AccelerationInAir
        {
            get => accelerationInAir;
            set => accelerationInAir = value;
        }

        /// <summary>
        /// Jump Power 프로퍼티 <br/>
        /// 점프 시의 힘
        /// </summary>
        public float JumpPower
        {
            get => jumpPower;
            set => jumpPower = value;
        }

        /// <summary>
        /// Ground Layer 프로퍼티 <br/>
        /// Ground 로 인식할 레이어
        /// </summary>
        public LayerMask GroundLayer
        {
            get => groundLayer;
            set => groundLayer = value;
        }

        /// <summary>
        /// Ground Radius 프로퍼티 <br/>
        /// Ground 로 인식하는 충돌 감지 범위의 반지름
        /// </summary>
        public float GroundRadius
        {
            get => groundRadius;
            set => groundRadius = value;
        }

        /// <summary>
        /// Ground Height 프로퍼티 <br/>
        /// Ground 로 인식하는 발 기준 하단 높이
        /// </summary>
        public float GroundHeight
        {
            get => groundHeight;
            set => groundHeight = value;
        }

        /// <summary>
        /// Slope Radius 프로퍼티 <br/>
        /// 경사면을 인식하기 위한 중심 기준 반지름
        /// </summary>
        public float SlopeRadius
        {
            get => slopeRadius;
            set => slopeRadius = value;
        }

        /// <summary>
        /// Slope Up Height 프로퍼티 <br/>
        /// 오르막길로 인식하는 발 위치 기준 상단 높이
        /// </summary>
        public float SlopeUpHeight
        {
            get => slopeUpHeight;
            set => slopeUpHeight = value;
        }

        /// <summary>
        /// Slope Down Height 프로퍼티 <br/>
        /// 내리막길로 인식하는 발 위치 기준 하단 높이
        /// </summary>
        public float SlopeDownHeight
        {
            get => slopeDownHeight;
            set => slopeDownHeight = value;
        }

        /// <summary>
        /// Set Movement 함수 <br/>
        /// 다음 Fixed Update 루틴에서 적용할 움직임 값 
        /// </summary>
        public void SetMovement(Vector2 movement, Space space = Space.World)
        {
            if (space == Space.World)
            {
                mMovement = movement;
                return;
            }

            Vector2 forwardVec = Forward;
            Vector2 upVec = Up;

            mMovement = forwardVec * movement.x + upVec * movement.y;
        }

        /// <summary>
        /// Jump 함수 <br/>
        /// 다음 Fixed Update 루틴에서 적용할 점프 값
        /// </summary>
        public void SetJump(float powerTimes = 1f)
        {
            if (!mCanJump)
            {
                return;
            }

            mCanJump = false;
            mJump = true;
            mJumpPowerTimes = powerTimes;
        }

        private void Awake()
        {
            mRigidbody = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            UpdateGrounded();
        }

        private void FixedUpdate()
        {
            UpdateGrounded();
            UpdateDrag();
            UpdateMovement();
            UpdateJump();
        }

        private void UpdateGrounded()
        {
            if (isFlying)
            {
                return;
            }

            Vector2 feetPosition = feet.position;
            Vector2 centerPosition = center.position;
            Vector2 feetVec = feetPosition - centerPosition;
            feetVec.Normalize();

            RaycastHit2D groundHit =
                Physics2D.CircleCast(feetPosition, groundRadius, feetVec, groundHeight, groundLayer);

            if (!mCanJump)
            {
                mIsGrounded = false;
                mCanJump = !groundHit;
            }

            if (!mIsGrounded || mMovement.sqrMagnitude <= Mathf.Epsilon)
            {
                mIsGrounded = groundHit;
                return;
            }

            Vector2 movement = mMovement.normalized;
            
            Vector2 slopeStart = feetPosition - feetVec * slopeUpHeight + movement * slopeRadius;
            RaycastHit2D slopeHit = Physics2D.Raycast(slopeStart, feetVec, slopeUpHeight + slopeDownHeight, groundLayer);
            
            if (!slopeHit)
            {
                mIsGrounded = groundHit;
                return;
            }
            
            Vector2 slopeHitPoint = slopeHit.point;
            if ((slopeHitPoint - slopeStart).sqrMagnitude <= Mathf.Epsilon)
            {
                mIsGrounded = groundHit;
                return;
            }

            Vector2 hitVec;
            if (groundHit)
            {
                hitVec = slopeHitPoint - groundHit.point;
                // Debug.DrawLine(groundHit.point, slopeHitPoint, Color.red);
            }
            else
            {
                hitVec = slopeHitPoint - feetPosition;
                // Debug.DrawLine(feetPosition, slopeHitPoint, Color.red);
            }
            hitVec.Normalize();

            float movementY = mMovement.y;
            mMovement = hitVec * Mathf.Abs(mMovement.x);
            mMovement.y += movementY;
        }

        private void UpdateDrag()
        {
            if (mIsGrounded || isFlying)
            {
                if (!mDragChanged)
                {
                    return;
                }

                mRigidbody.drag = mDrag;
                mDragChanged = false;
                return;
            }

            if (mDragChanged)
            {
                return;
            }

            mDragChanged = true;
            mDrag = mRigidbody.drag;
            mRigidbody.drag = 0f;
        }

        private void UpdateMovement()
        {
            Vector2 velocity = mRigidbody.velocity;
            Vector2 newVelocity = velocity;

            if (isFlying)
            {
                newVelocity += mMovement.normalized * (accelerationInAir * Time.fixedDeltaTime);
            
                if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                {
                    newVelocity = Vector2.ClampMagnitude(newVelocity, velocity.magnitude);
                }
            
                mRigidbody.velocity = newVelocity;
                
                mMovement = Vector2.zero;
                return;
            }

            if (mIsGrounded)
            {
                newVelocity += mMovement.normalized * (acceleration * Time.fixedDeltaTime);
                
                if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                {
                    newVelocity = Vector2.ClampMagnitude(newVelocity, velocity.magnitude);
                }
                
                mRigidbody.velocity = newVelocity;
                mMovement = Vector2.zero;
                return;
            }
            
            newVelocity.y = 0f;
            Vector2 prevVelocity = newVelocity;
            if (mIsGrounded)
            {
                newVelocity += mMovement.normalized * (acceleration * Time.fixedDeltaTime);
            }
            else
            {
                newVelocity += mMovement.normalized * (accelerationInAir * Time.fixedDeltaTime);
            }
            
            if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                newVelocity = Vector2.ClampMagnitude(newVelocity, prevVelocity.magnitude);
            }

            newVelocity.y += velocity.y;
            mRigidbody.velocity = newVelocity;
            mMovement = Vector2.zero;
        }
        
        private void UpdateJump()
        {
            Vector2 velocity = mRigidbody.velocity;
            
            if (!mJump)
            {
                if (mCanJump || velocity.y > Mathf.Epsilon)
                {
                    return;
                }

                mCanJump = true;
                return;
            }
            
            velocity.y = jumpPower * mJumpPowerTimes;
            mRigidbody.velocity = velocity;
            mJump = false;
        }
    }
}