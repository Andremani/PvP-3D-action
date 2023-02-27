using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andremani.Pvp3DAction
{
    public class InitialInput : MonoBehaviour
    {
        void Start()
        {
#if UNITY_EDITOR
#else
            Cursor.lockState = CursorLockMode.Confined;
#endif
        }
    }
}