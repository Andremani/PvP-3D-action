using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Andremani.Pvp3DAction
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private float minYRotationAngle;
        [SerializeField] private float maxYRotationAngle;
        [field: Space]
        [field: SerializeField] public float MouseSensitivity { get; private set; } = 1f;
        [HideInInspector] public bool isMouseXRotationLocked = false;
        public Vector3 MovementInput { get; private set; }

        public float MouseXRotationAngle { get; private set; }
        public float MouseYRotationAngle { get; private set; }

        public event Action LMBClick;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            ScanMouseInput();
            ScanMovementInput();
        }

        private void ScanMouseInput()
        {
            if (!isMouseXRotationLocked)
            {
                MouseXRotationAngle += Input.GetAxis("Mouse X") * MouseSensitivity;
            }
            MouseYRotationAngle -= Input.GetAxis("Mouse Y") * MouseSensitivity;
            MouseYRotationAngle = Mathf.Clamp(MouseYRotationAngle, minYRotationAngle, maxYRotationAngle);

            if (Input.GetMouseButtonDown(0))
            {
                LMBClick?.Invoke();
            }
        }

        private void ScanMovementInput()
        {
            MovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            if (MovementInput.sqrMagnitude > 1)
            {
                MovementInput.Normalize();
            }
        }
    }
}