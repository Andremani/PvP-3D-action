using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Andremani.Pvp3DAction.PlayerRelated
{
    public class PlayersClashDetector : MonoBehaviour
    {
        public event Action<Player> OnCollideWithPlayer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player")) //7 is current "Player" layer
            {
                Player otherPlayer;
                if (other.attachedRigidbody.gameObject.TryGetComponent<Player>(out otherPlayer))
                {
                    OnCollideWithPlayer?.Invoke(otherPlayer);
                }
                else
                {
                    Debug.LogError("Player script not found on object with 'Player' layer, is Rigidbody in a right place?");
                }
                
            }
        }
    }
}