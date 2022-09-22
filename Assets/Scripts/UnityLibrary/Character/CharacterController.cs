using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterController : MonoBehaviour
    {
        [Header("Movement")] [SerializeField] private bool isFlying;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float acceleration = 30f;
        [SerializeField] private float accelerationInAir = 10f;
        [SerializeField] private float jumpPower = 5f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundRadius = 0.1f;
        [SerializeField] private float groundHeight = 0.2f;
        [SerializeField] private float slopeRadius = 0.5f;
        [SerializeField] private float slopeUpHeight = 0.5f;
        [SerializeField] private float slopeDownHeight = 1.5f;

        [Header("Directions")] [SerializeField]
        private Transform body;

        [SerializeField] private Transform center;
        [SerializeField] private Transform feet;
        [SerializeField] private Transform forward;
        [SerializeField] private Transform right;
        [SerializeField] private Transform up;

        private Rigidbody mRigidbody;
        private bool mIsGrounded;

        private Vector3 mMovement;
        private bool mJump;
        private bool mCanJump = true;
        private float mJumpPowerTimes;

        private bool mDragChanged;
        private float mDrag;

        /// <summary>
        /// Rigidbody 프로퍼티
        /// </summary>
        public Rigidbody Rigidbody => mRigidbody;

        /// <summary>
        /// Is Grounded 프로퍼티 <br/>
        /// 땅을 밟고 있는가를 반환
        /// </summary>
        public bool IsGrounded => mIsGrounded;

        /// <summary>
        /// Max Speed 프로퍼티 <br/>
        /// 이동 시 최대 속도
        /// </summary>
        public float MaxSpeed
        {
            get => maxSpeed;
            set => maxSpeed = value;
        }

        /// <summary>
        /// Acceleration 프로퍼티 <br/>
        /// 이동 시 가속도
        /// </summary>
        public float Acceleration
        {
            get => acceleration;
            set => acceleration = value;
        }

        /// <summary>
        /// Acceleration In Air 프로퍼티 <br/>
        /// 공중에서 이동 시 가속도
        /// </summary>
        public float AccelerationInAir
        {
            get => accelerationInAir;
            set => accelerationInAir = value;
        }

        /// <summary>
        /// Jump Power 프로퍼티 <br/>
        /// 점프 시의 순간 힘(가속도)
        /// </summary>
        public float JumpPower
        {
            get => jumpPower;
            set => jumpPower = value;
        }

        /// <summary>
        /// Center 프로퍼티 <br/>
        /// 캐릭터의 중심 월드 좌표
        /// </summary>
        public Vector3 CenterPosition => center.position;

        /// <summary>
        /// Feet 프로퍼티 <br/>
        /// 캐릭터의 발 위치 좌표
        /// </summary>
        public Vector3 FeetPosition => feet.position;

        /// <summary>
        /// Body 프로퍼티 <br/>
        /// 메인 콜라이더, 렌더러가 존재하는 상위 Transform 
        /// </summary>
        public Transform Body => body;

        /// <summary>
        /// Forward 프로퍼티 <br/>
        /// 캐릭터의 전방 벡터
        /// </summary>
        public Vector3 Forward => forward.position - center.position;

        /// <summary>
        /// Right 프로퍼티 <br/>
        /// 캐릭터의 우측 벡터
        /// </summary>
        public Vector3 Right => right.position - center.position;

        /// <summary>
        /// Up 프로퍼티 <br/>
        /// 캐릭터의 상단 벡터
        /// </summary>
        public Vector3 Up => up.position - center.position;

        /// <summary>
        /// Set Movement 함수 <br/>
        /// 이동하기 위한 움직임(movement)를 설정하는 함수 <br/>
        /// Kinematic Rigidbody 가 아닌 경우에는 y 값이 사용되지 않는다. 
        /// </summary>
        public void SetMovement(Vector3 movement, Space space = Space.World)
        {
            if (space == Space.World)
            {
                mMovement = movement;
                return;
            }

            Vector3 centerPosition = center.position;
            Vector3 forwardVec = forward.position - centerPosition;
            Vector3 rightVec = right.position - centerPosition;
            Vector3 upVec = up.position - centerPosition;

            mMovement = forwardVec * movement.z + rightVec * movement.x + upVec * movement.y;
        }

        /// <summary>
        /// Set Jump 함수 <br/>
        /// 점프를 수행하는 함수로
        /// powerTimes 로 점프 힘 배수를 추가로 설정할 수 있다. 
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
            mRigidbody = GetComponent<Rigidbody>();
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

            Vector3 feetPosition = feet.position;
            Vector3 centerPosition = center.position;
            Vector3 feetVec = feetPosition - centerPosition;
            feetVec.Normalize();

            Ray ray = new Ray(feetPosition, feetVec);
            bool groundResult =
                Physics.SphereCast(ray, groundRadius, out RaycastHit groundHit, groundHeight, groundLayer);

            if (!mCanJump)
            {
                mIsGrounded = false;
                mCanJump = !groundResult;
                return;
            }

            if (!mIsGrounded || mMovement.sqrMagnitude <= Mathf.Epsilon)
            {
                mIsGrounded = groundResult;
                return;
            }

            Vector3 movement = mMovement.normalized;

            Vector3 slopeStart = feetPosition - feetVec * slopeUpHeight + movement * slopeRadius;
            ray = new Ray(slopeStart, feetVec);
            bool slopeResult =
                Physics.Raycast(ray, out RaycastHit slopeHit, slopeUpHeight + slopeDownHeight, groundLayer);

            if (!slopeResult)
            {
                mIsGrounded = groundResult;
                return;
            }

            Vector3 slopeHitPoint = slopeHit.point;
            if ((slopeHitPoint - slopeStart).sqrMagnitude <= Mathf.Epsilon)
            {
                mIsGrounded = groundResult;
                return;
            }

            Vector3 hitVec;
            if (groundResult)
            {
                hitVec = slopeHitPoint - groundHit.point;
                Debug.DrawLine(groundHit.point - feetVec, slopeHitPoint - feetVec, Color.red);
            }
            else
            {
                hitVec = slopeHitPoint - feetPosition;
                Debug.DrawLine(feetPosition - feetVec, slopeHitPoint - feetVec, Color.red);
            }
            hitVec.Normalize();

            float movementY = mMovement.y;
            mMovement.y = 0f;
            mMovement = hitVec * mMovement.magnitude;
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
            Vector3 velocity = mRigidbody.velocity;
            Vector3 newVelocity = velocity;

            if (isFlying)
            {
                newVelocity += mMovement.normalized * (accelerationInAir * Time.fixedDeltaTime);

                if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed * mMovement.sqrMagnitude)
                {
                    newVelocity = Vector3.ClampMagnitude(newVelocity, velocity.magnitude);
                }

                mRigidbody.velocity = newVelocity;
                mMovement = Vector3.zero;
                return;
            }

            if (mIsGrounded)
            {
                newVelocity += mMovement.normalized * (acceleration * Time.fixedDeltaTime);

                if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed * mMovement.sqrMagnitude)
                {
                    newVelocity = Vector3.ClampMagnitude(newVelocity, velocity.magnitude);
                }

                mRigidbody.velocity = newVelocity;
                mMovement = Vector3.zero;
                return;
            }

            newVelocity.y = 0f;
            Vector3 prevVelocity = newVelocity;
            
            newVelocity += mMovement.normalized * (accelerationInAir * Time.fixedDeltaTime);

            if (newVelocity.sqrMagnitude > maxSpeed * maxSpeed * mMovement.sqrMagnitude)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, prevVelocity.magnitude);
            }

            newVelocity.y += velocity.y;
            mRigidbody.velocity = newVelocity;
            mMovement = Vector3.zero;
        }

        private void UpdateJump()
        {
            Vector3 velocity = mRigidbody.velocity;
            
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