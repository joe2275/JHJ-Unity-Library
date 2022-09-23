using System;
using UnityEngine;
using CharacterController = Character.CharacterController;

namespace Samples
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputSample))]
    public class PlayerSample : MonoBehaviour
    {
        [Header("Character")] 
        [SerializeField] private float runRate = 2f;

        [Header("Animation")] 
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private float blendSpeed = 5f;
        
        [Header("Camera")] 
        [SerializeField] private Transform cameraFollow;
        
        private readonly int mAnimationSpeedParam = Animator.StringToHash("Speed");
        private readonly int mAnimationGroundedParam = Animator.StringToHash("Grounded");
        private readonly int mAnimationJumpParam = Animator.StringToHash("Jump");
        private float mPrevAnimationSpeed;
        private CharacterController mController;
        private PlayerInputSample mInput;
        private bool mIsRunning;


        private void Awake()
        {
            mController = GetComponent<CharacterController>();
            mInput = GetComponent<PlayerInputSample>();
        }

        private void Update()
        {
            ApplyMovement();
            ApplyJump();
            ApplyAnimationParams();
        }
        
        private void ApplyMovement()
        {
            Vector2 moveInput = mInput.MoveInput;

            Vector3 cameraForward = cameraFollow.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            Vector3 cameraRight = cameraFollow.right;

            Vector3 movement = cameraForward * moveInput.y + cameraRight * moveInput.x;
            mController.SetMovement(movement);
            
            if (mInput.RunInput)
            {
                if (mIsRunning)
                {
                    return;
                }

                mIsRunning = true;
                mController.MaxSpeed *= runRate;
            }
            else
            {
                if (!mIsRunning)
                {
                    return;
                }

                mIsRunning = false;
                mController.MaxSpeed /= runRate;
            }
        }
        
        private void ApplyJump()
        {
            if (!mController.IsGrounded || !mInput.JumpInput)
            {
                return;
            }

            mController.SetJump();
            characterAnimator.SetTrigger(mAnimationJumpParam);
        }
        
        private void ApplyAnimationParams()
        {
            float curSpeed = mInput.MoveInput.magnitude;
            if (!mIsRunning)
            {
                curSpeed *= 0.4f;
            }
            float speed = Mathf.MoveTowards(mPrevAnimationSpeed, curSpeed, blendSpeed * Time.deltaTime);
            mPrevAnimationSpeed = speed;
            characterAnimator.SetFloat(mAnimationSpeedParam, speed);
            characterAnimator.SetBool(mAnimationGroundedParam, mController.IsGrounded);
        }
    }
}