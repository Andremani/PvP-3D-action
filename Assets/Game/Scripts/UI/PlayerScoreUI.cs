using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Andremani.Pvp3DAction.UI
{
    public class PlayerScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI score;

        public string PlayerNameText 
        { 
            set
            {
                playerNameText.text = value;
            }
        }
        public int Score 
        { 
            set
            {
                score.text = value.ToString();
            }
        }
    }
}