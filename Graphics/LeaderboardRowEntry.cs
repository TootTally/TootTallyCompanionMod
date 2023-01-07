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
        public Text rank, username, score, percent, grade, maxcombo;
        public bool hasBackground;
        public string replayId;

        public void ConstructLeaderboardEntry(GameObject singleScore, Text rank, Text username, Text score, Text percent, Text grade, Text maxcombo, bool hasBackground = false)
        {
            this.singleScore = singleScore;
            this.rank = rank;
            this.username = username;
            this.score = score;
            this.percent = percent;
            this.grade = grade;
            this.maxcombo = maxcombo;
            this.hasBackground = hasBackground;
            this.singleScore.transform.Find("Image").gameObject.SetActive(hasBackground); //yep... ¯\_(ツ)_/¯
        }

        public void ToggleBackground()
        {
            hasBackground = !hasBackground;
            this.singleScore.transform.Find("Image").gameObject.SetActive(hasBackground);
        }

    }
}
