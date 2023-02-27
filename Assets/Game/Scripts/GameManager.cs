using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Andremani.Pvp3DAction.PlayerRelated;

namespace Andremani.Pvp3DAction
{
    public class GameManager : NetworkBehaviourSingleton<GameManager>
    {
        [SerializeField] private int initialScore = 0;
        [SerializeField] private int winningScore = 3;
        [SerializeField] private float resetGameAfterWinTimer = 5f;

        public readonly SyncDictionary<string, int> playerScores = new SyncDictionary<string, int>();

        public event System.Action<string, int> OnPlayerAddedToScoreCount;
        public event System.Action<string> OnPlayerExcludedFromScoreCount;
        public event System.Action<string, int> OnScoreUpdated;
        public event System.Action<string> OnWin;
        public event System.Action OnResetScore;

        public override void Awake()
        {
            base.Awake();

            Pvp3DActionNetworkManager.OnServerAddPlayerEvent += OnServerPlayerAdded;
            Pvp3DActionNetworkManager.OnServerDisconnectEvent += OnServerPlayerDisconnected;

            PlayersClashSystem.DashCrushServerEvent += OnDashCrush;
        }

        private void OnServerInitialized()
        {
            playerScores.Clear();
        }

        [ServerCallback]
        private void OnServerPlayerAdded(NetworkConnectionToClient conn, Player player)
        {
            playerScores.Add(player.Nickname, initialScore);
            RpcOnPlayerAddedToScoreCount(player.Nickname, initialScore);
        }

        [ServerCallback]
        private void OnServerPlayerDisconnected(NetworkConnectionToClient conn, Player player)
        {
            playerScores.Remove(player.Nickname);
            RpcPlayerExcludedFromScoreCount(player.Nickname);
        }

        [ServerCallback]
        private void OnDashCrush(Player winner, Player loser)
        {
            AddScore(winner, 1);
        }

        [ServerCallback]
        private void AddScore(Player player, int amount)
        {
            playerScores[player.Nickname] += amount;
            RpcOnScoreUpdated(player.Nickname, playerScores[player.Nickname]);
            //Debug.Log("+"+amount+" point(s) for " + player.gameObject.name + "! Current points: " + playerScores[player]);

            if(playerScores[player.Nickname] >= winningScore)
            {
                RpcOnWin(player.Nickname);
                StartCoroutine(ResetGame(resetGameAfterWinTimer));
            }
        }

        [ServerCallback]
        private IEnumerator ResetGame(float resetTime)
        {
            yield return new WaitForSeconds(resetTime);

            ResetScore();

            Pvp3DActionNetworkManager networkManagerInstance = (Pvp3DActionNetworkManager)Pvp3DActionNetworkManager.singleton;
            networkManagerInstance.RespawnPlayers();
        }

        [ServerCallback]
        private void ResetScore()
        {
            List<string> players = new List<string>(playerScores.Keys);
            foreach (string key in players)
            {
                playerScores[key] = 0;
            }
            RpcOnResetScore();
        }

        [ClientRpc]
        private void RpcOnPlayerAddedToScoreCount(string playerNickname, int initialScore)
        {
            OnPlayerAddedToScoreCount?.Invoke(playerNickname, initialScore);
        }

        [ClientRpc]
        private void RpcPlayerExcludedFromScoreCount(string playerNickname)
        {
            OnPlayerExcludedFromScoreCount?.Invoke(playerNickname);
        }

        [ClientRpc]
        private void RpcOnScoreUpdated(string playerNickname, int newScore)
        {
            OnScoreUpdated?.Invoke(playerNickname, newScore);
        }

        [ClientRpc]
        private void RpcOnWin(string playerNickname)
        {
            OnWin?.Invoke(playerNickname);
        }

        [ClientRpc]
        private void RpcOnResetScore()
        {
            OnResetScore?.Invoke();
        }
    }
}