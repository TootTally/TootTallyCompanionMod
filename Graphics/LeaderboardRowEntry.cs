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
            rank.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 45);

            this.username = username;
            username.GetComponent<RectTransform>().sizeDelta = new Vector2(125, 45);

            this.score = score;
            score.GetComponent<RectTransform>().sizeDelta = new Vector2(125, 45);

            this.percent = percent;
            percent.GetComponent<RectTransform>().sizeDelta = new Vector2(45, 45);

            this.grade = grade;
            grade.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 45);

            this.maxcombo = maxcombo;
            maxcombo.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 45);

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
