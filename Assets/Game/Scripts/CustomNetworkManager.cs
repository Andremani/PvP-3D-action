using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace Andremani.Pvp3DAction
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Custom Settings")]
        [Tooltip("Has effect only when 'Random' player spawn method chosen")]
        [SerializeField] private bool allowPlayersStartPositionsRepeating = true;
        private Dictionary<NetworkConnectionToClient, Transform> startPositionsInUse = new Dictionary<NetworkConnectionToClient, Transform>();

        public static event Action OnClientDisconnectEvent;

        public override Transform GetStartPosition()
        {
            // first remove any dead transforms
            startPositions.RemoveAll(t => t == null);

            if (startPositions.Count == 0)
                return null;


            if (playerSpawnMethod == PlayerSpawnMethod.Random)
            {
                int randomIndex = UnityEngine.Random.Range(0, startPositions.Count);

                if (allowPlayersStartPositionsRepeating)
                {
                    return startPositions[randomIndex];
                }
                else
                {
                    int initialIndex = randomIndex;
                    do
                    {
                        if (!startPositionsInUse.ContainsValue(startPositions[randomIndex]))
                        {
                            return startPositions[randomIndex];
                        }
                        randomIndex++;
                        if(randomIndex >= startPositions.Count)
                        {
                            randomIndex = 0;
                        }
                    } while (randomIndex != initialIndex);

                    Debug.LogWarning("Free starting points ended! List has been reset...");
                    startPositionsInUse.Clear();

                    return startPositions[randomIndex];
                }
            }
            else //if (playerSpawnMethod == PlayerSpawnMethod.RoundRobin)
            {
                Transform startPosition = startPositions[startPositionIndex];
                startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
                return startPosition;
            }
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            if(NetworkServer.AddPlayerForConnection(conn, player))
            {
                if (!startPositionsInUse.ContainsValue(startPos))
                {
                    startPositionsInUse.Add(conn, startPos);
                }
            }
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            startPositionsInUse.Remove(conn);
        }

        public override void OnClientDisconnect()
        {
            OnClientDisconnectEvent?.Invoke();
        }
    }
}