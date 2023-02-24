using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andremani.Pvp3DAction
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraRootTransform;

        private PlayerInput input;
        private Transform player;

        public void Init(PlayerInput playerInput, Transform playerTransform)
        {
            input = playerInput;
            player = playerTransform;
        }

        private void LateUpdate()
        {
            cameraRootTransform.position = player.position;
            cameraRootTransform.rotation = Quaternion.Euler(input.MouseYRotationAngle, input.MouseXRotationAngle, 0);
        }
    }
}