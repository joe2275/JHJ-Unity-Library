using System;
using UnityEngine;
using UnityEngine.InputSystem;
using CharacterController = Character.CharacterController;

namespace Samples
{
    public class PlayerCameraSample : MonoBehaviour
    {
        [Header("Camera")] 
        [SerializeField] private Vector2 rotationSpeed = new Vector2(10f, 10f);
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float minPitch = -70f;
        
        [Header("Character")]
        [SerializeField] private float bodyRotationSpeed = 540f;
        [SerializeField] private CharacterController controller;
        [SerializeField] private PlayerInputSample input;

        private void Update()
        {
            ApplyCameraRotation();
            RotateBody();
        }

        private void ApplyCameraRotation()
        {
            Vector2 rotateInput = input.RotateInput;
            transform.Rotate(Vector3.up, rotateInput.x * rotationSpeed.x * Time.fixedDeltaTime, Space.World);
            transform.Rotate(transform.right, -rotateInput.y * rotationSpeed.y * Time.fixedDeltaTime, Space.World);
            
            Vector3 localEuler = transform.localEulerAngles;

            if (localEuler.x > 180f)
            {
                float processedMinPitch = 360f + minPitch;
                if (localEuler.x < processedMinPitch)
                {
                    localEuler.x = processedMinPitch;
                }
            }
            else
            {
                if (localEuler.x > maxPitch)
                {
                    localEuler.x = maxPitch;
                }
            }

            localEuler.z = 0f;
            transform.localEulerAngles = localEuler;
        }
        
        private void RotateBody()
        {
            Vector2 moveInput = input.MoveInput;
            if (Mathf.Abs(moveInput.x) <= Mathf.Epsilon && Mathf.Abs(moveInput.y) <= Mathf.Epsilon)
            {
                return;
            }
            
            Vector3 forwardVec = transform.forward;
            forwardVec.y = 0f;
            forwardVec.Normalize();
            Vector3 rightVec = transform.right;
            Vector3 movement = forwardVec * moveInput.y + rightVec * moveInput.x;
            
            Quaternion rotation = controller.transform.rotation;
            controller.transform.rotation = Quaternion.RotateTowards(rotation, Quaternion.Euler(movement), bodyRotationSpeed * Time.deltaTime);
        }
    }
}