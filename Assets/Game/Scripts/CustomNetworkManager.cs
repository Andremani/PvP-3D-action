using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Andremani.Pvp3DAction.UI;

namespace Andremani.Pvp3DAction
{
    public class CustomNetworkManager : NetworkManager
    {
        [Header("Custom Settings")]
        [Tooltip("Has effect only when 'Random' player spawn method chosen")]
        [SerializeField] private bool allowPlayersStartPositionsRepeating = true;
        [SerializeField] private NicknamePanelUI nicknamePanel;

        private Dictionary<NetworkConnectionToClient, Transform> startPositionsInUse = new Dictionary<NetworkConnectionToClient, Transform>();

        public static event System.Action<NetworkConnectionToClient, Player> OnServerAddPlayerEvent;
        public static event System.Action<NetworkConnectionToClient, Player> OnServerDisconnectEvent;
        public static event System.Action OnClientConnectEvent;
        public static event System.Action OnClientDisconnectEvent;

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();

            NetworkServer.RegisterHandler<CreateCharacterMessage>(OnServerCreateCharacter);
        }

        [Client]
        public override void OnClientConnect()
        {
            base.OnClientConnect();

            #region ValidatePlayerCreation() - internal internal checks for invalid inputs
            bool ValidatePlayerCreation()
            {
                // ensure valid ready connection
                if (NetworkClient.connection == null)
                {
                    Debug.LogError("AddPlayer requires a valid NetworkClient.connection.");
                    return false;
                }

                // UNET checked 'if readyConnection != null'.
                // in other words, we need a connection and we need to be ready.
                if (!NetworkClient.ready)
                {
                    Debug.LogError("AddPlayer requires a ready NetworkClient.");
                    return false;
                }

                if (NetworkClient.connection.identity != null)
                {
                    Debug.LogError("NetworkClient.AddPlayer: a PlayerController was already added. Did you call AddPlayer twice?");
                    return false;
                }

                return true;
            }
            #endregion
            if (!ValidatePlayerCreation())
            {
                return;
            }
            OnClientConnectEvent?.Invoke();

            CreateCharacterMessage characterMessage = new CreateCharacterMessage
            {
                nickname = nicknamePanel.GetNickname(),
            };

            NetworkClient.Send(characterMessage);
        }

        [Server]
        private void OnServerCreateCharacter(NetworkConnectionToClient currentConnection, CreateCharacterMessage message)
        {
            #region ValidatePlayerCreation() - internal checks for invalid inputs
            bool ValidatePlayerCreation()
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("The PlayerPrefab is empty on the NetworkManager. Please setup a PlayerPrefab object.");
                    return false;
                }

                if (!playerPrefab.TryGetComponent(out NetworkIdentity _))
                {
                    Debug.LogError("The PlayerPrefab does not have a NetworkIdentity. Please add a NetworkIdentity to the player prefab.");
                    return false;
                }

                if (currentConnection.identity != null)
                {
                    Debug.LogError("There is already a player for this connection.");
                    return false;
                }
                return true;
            }
            #endregion
            if(!ValidatePlayerCreation())
            {
                return;
            }

            #region NicknameCheckForRepetition(string nickname) - returning final variant of nickname
            string NicknameCheckForRepetition(string nickname)
            {
                int counter = 2; //appendix to nickname
                playersNicknameClashCheck:
                foreach (var connection in NetworkServer.connections.Values)
                {
                    if (connection == currentConnection)
                    {
                        break;
                    }
                    if (connection.identity.GetComponent<Player>().Nickname == nickname)
                    {
                        nickname = nickname + " " + counter;
                        counter++;
                        goto playersNicknameClashCheck;
                    };
                }
                return nickname;
            }
            #endregion
            message.nickname = NicknameCheckForRepetition(message.nickname);
            
            Transform startPos = GetStartPosition();
            GameObject playerObject = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            playerObject.name = $"{playerPrefab.name} [connId={currentConnection.connectionId}] [nickname={message.nickname}]";

            Player player;
            if(!playerObject.TryGetComponent<Player>(out player))
            {
                Debug.LogError("There are no Player component in root of player object prefab");
                return;
            }
            player.Nickname = message.nickname;

            if (NetworkServer.AddPlayerForConnection(currentConnection, playerObject))
            {
                AddStartPositionInUse(currentConnection, startPos);

                OnServerAddPlayerEvent?.Invoke(currentConnection, player);
            }
        }

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
                        if (randomIndex >= startPositions.Count)
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

        [Server]
        public void AddStartPositionInUse(NetworkConnectionToClient currentConnection, Transform startPos)
        {
            if (!startPositionsInUse.ContainsValue(startPos))
            {
                startPositionsInUse.Add(currentConnection, startPos);
            }
        }

        [ServerCallback]
        public void RespawnPlayers()
        {
            startPositionsInUse.Clear();

            foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
            {
                NetworkIdentity identity = connection?.identity;
                Player player;
                if (identity.TryGetComponent<Player>(out player))
                {
                    RespawnPlayer(connection, player); //why need i using ReplacePlayerForConnection? Alternatives for respawing?
                }
                else
                {
                    Debug.LogWarning("There are no 'Player' component on main client' NetworkIdentity while it is expected to be here");
                }
            }
        }

        [ServerCallback]
        private void RespawnPlayer(NetworkConnectionToClient conn, Player oldPlayer)
        {
            Transform startingPosition = GetStartPosition();
            AddStartPositionInUse(conn, startingPosition);

            GameObject newPlayerObject = Instantiate(playerPrefab);

            newPlayerObject.transform.position = startingPosition.position;
            newPlayerObject.transform.rotation = startingPosition.rotation;

            newPlayerObject.GetComponent<Player>().Nickname = oldPlayer.Nickname;

            NetworkServer.ReplacePlayerForConnection(conn, newPlayerObject, true);

            oldPlayer.gameObject.SetActive(false);
            Destroy(oldPlayer.gameObject, 0.1f);
        }

        [Server]
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Player player = conn?.identity?.GetComponent<Player>();
            if (player != null)
            {
                OnServerDisconnectEvent?.Invoke(conn, player);
            }
            else
            {
                Debug.LogError("Connection is null / identity is null / No Player component near NetworkIdentity");
            }

            startPositionsInUse.Remove(conn);
            base.OnServerDisconnect(conn); //do not try to access Player object after this, it will be destroyed here
        }

        public override void OnClientDisconnect()
        {
            OnClientDisconnectEvent?.Invoke();
        }
    }
}