using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Andremani.Pvp3DAction.UI
{
    public class NicknamePanelUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private string defaultNickname = "Player";

        private void Start()
        {
            CustomNetworkManager.OnClientDisconnectEvent += OnDisconnectedFromServer;
            CustomNetworkManager.OnClientConnectEvent += OnConnectedToServer;
        }

        private void OnConnectedToServer()
        {
            gameObject.SetActive(false);
        }

        private void OnDisconnectedFromServer()
        {
            gameObject.SetActive(true);
        }

        public string GetNickname()
        {
            if (System.String.IsNullOrWhiteSpace(inputField.text))
            {
                return defaultNickname;
            }
            else
            {
                return inputField.text;
            }
        }
    }
}