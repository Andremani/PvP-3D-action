using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Andremani.Pvp3DAction.PlayerRelated
{
    public class Player : NetworkBehaviour
    {
        [field: SerializeField] public PlayerInput Input { get; private set; }
        [field: SerializeField] public PlayerMovementController MovementController { get; private set; }
        [field: SerializeField] public PlayersClashSystem ClashSystem { get; private set; }

        [HideInInspector] public string Nickname { get; set; }

        public override void OnStartLocalPlayer()
        {
            Camera mainCamera = Camera.main;
            ThirdPersonViewCameraController cameraController = mainCamera?.GetComponent<ThirdPersonViewCameraController>();

            if (cameraController != null)
            {
                cameraController.Init(Input, transform);
            }
            else
            {
                Debug.LogError("Failed to find Main Camera!");
            }
            MovementController.Init(Input);
        }
    }
}