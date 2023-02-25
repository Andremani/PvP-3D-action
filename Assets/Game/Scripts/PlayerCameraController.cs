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

        private void Awake()
        {
            enabled = false;
            CustomNetworkManager.OnClientDisconnectEvent += OnDisconnectedFromServer;
        }

        public void Init(PlayerInput playerInput, Transform playerTransform)
        {
            input = playerInput;
            player = playerTransform;

            enabled = true;
        }

        private void LateUpdate()
        {
            cameraRootTransform.position = player.position;
            cameraRootTransform.rotation = Quaternion.Euler(input.MouseYRotationAngle, input.MouseXRotationAngle, 0);
        }

        private void OnDisconnectedFromServer()
        {
            enabled = false;
        }
    }
}