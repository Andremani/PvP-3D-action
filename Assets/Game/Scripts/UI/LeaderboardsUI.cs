using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Andremani.Pvp3DAction.UI
{
    public class LeaderboardsUI : MonoBehaviour
    {
        [SerializeField] Transform listParentTransform;
        [SerializeField] PlayerScoreUI playerScoreUIPrefab;
        private Dictionary<string, PlayerScoreUI> playerScores = new Dictionary<string, PlayerScoreUI>();

        private void Start()
        {
            if (listParentTransform == null || playerScoreUIPrefab == null)
            {
                Debug.LogError("Null reference in Leaderdoards!");
            }
            gameObject.SetActive(false);

            Pvp3DActionNetworkManager.OnClientConnectEvent += OnClientConnect;
            Pvp3DActionNetworkManager.OnClientDisconnectEvent += OnClientDisconnect;

            GameManager.I.OnPlayerAddedToScoreCount += AddPlayerToLeaderboard;
            GameManager.I.OnPlayerExcludedFromScoreCount += RemovePlayerFromLeaderboard;
            GameManager.I.OnScoreUpdated += UpdateScore;
            GameManager.I.OnResetScore += ResetScore;
        }

        private void ResetScore()
        {
            foreach (var playerScoreUI in playerScores.Values)
            {
                playerScoreUI.Score = 0;
            }
        }

        private void OnClientConnect()
        {
            gameObject.SetActive(true);
            StartCoroutine(DelayedLeaderboardsUpdate());
        }

        private IEnumerator DelayedLeaderboardsUpdate()
        {
            yield return new WaitForSeconds(0.1f);
            
            foreach (var record in GameManager.I.playerScores)
            {
                AddPlayerToLeaderboard(record.Key, record.Value);
            }
            listParentTransform.gameObject.SetActive(true);
        }

        private void OnClientDisconnect()
        {
            gameObject.SetActive(false);
            foreach(var playerScore in playerScores)
            {
                Destroy(playerScore.Value.gameObject);
            }
            playerScores.Clear();
        }

        private void AddPlayerToLeaderboard(string playerNickname, int initialScore)
        {
            if(playerScores.ContainsKey(playerNickname))
            {
                return;
            }

            PlayerScoreUI newPlayerScoreUI = Instantiate(playerScoreUIPrefab, listParentTransform);
            playerScores.Add(playerNickname, newPlayerScoreUI);

            newPlayerScoreUI.PlayerNameText = playerNickname;
            newPlayerScoreUI.Score = initialScore;
        }

        private void RemovePlayerFromLeaderboard(string playerNickname)
        {
            if (!playerScores.ContainsKey(playerNickname))
            {
                return;
            }

            Destroy(playerScores[playerNickname].gameObject);
            playerScores.Remove(playerNickname);
        }

        private void UpdateScore(string playerNickname, int newScore)
        {
            if (!playerScores.ContainsKey(playerNickname))
            {
                return;
            }

            playerScores[playerNickname].PlayerNameText = playerNickname;
            playerScores[playerNickname].Score = newScore;
        }
    }
}