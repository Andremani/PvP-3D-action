using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andremani.Pvp3DAction
{
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private Transform playerOrientationTransform;
        [SerializeField] private Rigidbody playerRigidbody;
        [Space]
        [SerializeField] private float speed = 1f;

        private PlayerInput input;

        public void Init(PlayerInput playerInput)
        {
            input = playerInput;
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
    }
}