using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.Graphics
{
    public class LeaderboardRowEntry : MonoBehaviour
    {
        public GameObject singleScore;
        public Text rank, username, score;
        public bool hasBackground;

        public void ConstructLeaderboardEntry(GameObject singleScore, Text rank, Text username, Text score, bool hasBackground)
        {
            this.singleScore = singleScore;
            this.rank = rank;
            this.username = username;
            this.score = score;
            this.singleScore.transform.Find("Image").gameObject.SetActive(hasBackground); //yep... ¯\_(ツ)_/¯
        }

    }
}
