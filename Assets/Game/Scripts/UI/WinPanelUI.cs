using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Andremani.Pvp3DAction.UI
{
    public class WinPanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI winningPlayerNicknameText;
        [SerializeField] private float showDuration = 4.5f;

        private void Start()
        {
            gameObject.SetActive(false);
            GameManager.I.OnWin += ShowWinner;
        }

        public void ShowWinner(string nickname)
        {
            winningPlayerNicknameText.text = nickname;
            gameObject.SetActive(true);
            StartCoroutine(HideCooldown(showDuration));
        }

        public IEnumerator HideCooldown(float duration)
        {
            yield return new WaitForSeconds(duration);
            gameObject.SetActive(false);
        }
    }
}