using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Andremani.Pvp3DAction.Enums;

namespace Andremani.Pvp3DAction.PlayerRelated
{
    public class PlayersClashSystem : NetworkBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private PlayersClashDetector playersClashDetector;
        [SerializeField] private PlayerVisualsManager visualsManager;
        [Space]
        [SerializeField] private PlayersClashBothInDashBehaviour bothInDashBehaviour;
        [SerializeField] private float invulnurabilityDuration;

        [SyncVar(hook = nameof(OnInvulnerabilityStateChanged))] private bool isInvulnerable;

        public static event Action<Player, Player> DashCrushServerEvent;

        private void OnValidate()
        {
            if (invulnurabilityDuration < 0f)
            {
                invulnurabilityDuration = 0f;
            }
        }

        [ServerCallback]
        private void Start()
        {
            playersClashDetector.OnCollideWithPlayer += OnCollideWithPlayer;
        }

        [ServerCallback]
        private void OnCollideWithPlayer(Player otherPlayer)
        {
            if(player.MovementController.IsDashing && otherPlayer.MovementController.IsDashing)
            {
                if(bothInDashBehaviour == PlayersClashBothInDashBehaviour.BothGetDamaged)
                {
                    TryDashCrush(player, otherPlayer);
                    TryDashCrush(otherPlayer, player);
                }
                else
                {
                    return;
                }
            }

            //if values are different from each other
            if (player.MovementController.IsDashing ^ otherPlayer.MovementController.IsDashing) 
            {
                if (player.MovementController.IsDashing)
                {
                    TryDashCrush(player, otherPlayer);
                }
                else if (otherPlayer.MovementController.IsDashing)
                {
                    TryDashCrush(otherPlayer, player);
                }
            }
        }

        [ServerCallback]
        private void TryDashCrush(Player attacker, Player defender)
        {
            if(!defender.ClashSystem.isInvulnerable)
            {
                OnSuccessfulDashCrush(attacker, defender);
            }
        }

        [ServerCallback]
        private void OnSuccessfulDashCrush(Player winner, Player loser)
        {
            OnPlayerGetDamaged(loser);
            DashCrushServerEvent?.Invoke(winner, loser);
        }

        [ServerCallback]
        private void OnPlayerGetDamaged(Player damagedPlayer)
        {
            damagedPlayer.ClashSystem.isInvulnerable = true;
            //hook of 'isInvulnerable' triggered
        }

        private void OnInvulnerabilityStateChanged(bool oldValue, bool newValue)
        {
            //Debug.Log("Hook fired! Old value: " + oldValue + ", New value: " + newValue);
            if(newValue == true)
            {
                //Debug.Log(player.gameObject.name + " TurnedRed");
                StartCoroutine(InvulnerabilityCooldown(invulnurabilityDuration));
                StartCoroutine(visualsManager.TriggerDamagedVisuals(invulnurabilityDuration));
            }
        }

        private IEnumerator InvulnerabilityCooldown(float duration)
        {
            yield return new WaitForSeconds(duration);
            isInvulnerable = false;
        }
    }
}