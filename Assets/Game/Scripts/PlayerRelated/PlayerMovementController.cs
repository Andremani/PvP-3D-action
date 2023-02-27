using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Andremani.Pvp3DAction.Enums;

namespace Andremani.Pvp3DAction.PlayerRelated
{
    public class PlayerMovementController : NetworkBehaviour
    {
        [SerializeField] private Transform playerOrientationTransform;
        [SerializeField] private Rigidbody playerRigidbody;
        [Space]
        [SerializeField] private float speed = 1f;
        [SerializeField] private float dashSpeed = 50f;
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private DashMode dashMode;

        private PlayerInput input;
        private bool isDashReloading;

        [field: SyncVar] public bool IsDashing { get; private set; }

        private void Awake()
        {
            enabled = false;
        }

        public void Init(PlayerInput playerInput)
        {
            input = playerInput;

            enabled = true;
        }

        private void OnEnable()
        {
            if(input != null)
            {
                input.LMBClick += Dash;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.LMBClick -= Dash;
            }
        }

        private void Update()
        {
            playerOrientationTransform.rotation = Quaternion.Euler(0, input.MouseXRotationAngle, 0);
        }

        private void FixedUpdate()
        {
            Vector3 moveVector = playerOrientationTransform.TransformDirection(input.MovementInput) * speed;
            playerRigidbody.velocity = new Vector3(moveVector.x, playerRigidbody.velocity.y, moveVector.z);
        }

        private void Dash()
        {
            if(!isDashReloading)
            {
                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            Vector3 moveVector;
            switch(dashMode)
            {
                case DashMode.TowardsCurrentMovement:
                    {
                        Vector3 direction = input.MovementInput.normalized;
                        if (direction == Vector3.zero)
                        {
                            yield break;
                        }
                        moveVector = playerOrientationTransform.TransformDirection(direction) * dashSpeed;
                    };
                    break;
                case DashMode.TowardsForwardOrientation:
                    {
                        moveVector = playerOrientationTransform.forward * dashSpeed;
                    }
                    break;
                default:
                    {
                        goto case DashMode.TowardsForwardOrientation;
                    }
            }
            playerRigidbody.velocity = new Vector3(moveVector.x, playerRigidbody.velocity.y, moveVector.z);
            IsDashing = true;
            enabled = false;
            input.isMouseXRotationLocked = true;

            float dashTime = dashDistance / dashSpeed;
            yield return new WaitForSeconds(dashTime);

            IsDashing = false;
            enabled = true;
            input.isMouseXRotationLocked = false;
            isDashReloading = true;

            yield return new WaitForSeconds(dashCooldown);

            isDashReloading = false;
        }
    }
}