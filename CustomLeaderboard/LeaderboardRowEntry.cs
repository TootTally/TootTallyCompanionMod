using System;
using System.Collections.Generic;
using System.Text;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.CustomLeaderboard
{
    public class LeaderboardRowEntry : MonoBehaviour
    {
        public GameObject singleScore;
        public Text rank, username, score, percent, grade, maxcombo;
        public Image imageStrip;
        public bool hasBackground;
        public string replayId;
        public int rowId;

        public void ConstructLeaderboardEntry(GameObject singleScore, Text rank, Text username, Text score, Text percent, Text grade, Text maxcombo, bool hasBackground = false)
        {
            this.singleScore = singleScore;
            this.rank = rank;
            rank.GetComponent<RectTransform>().sizeDelta = new Vector2(35, 35);
            rank.alignment = TextAnchor.MiddleLeft;

            this.username = username;
            username.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 35);
            username.alignment = TextAnchor.MiddleLeft;

            this.score = score;
            score.GetComponent<RectTransform>().sizeDelta = new Vector2(175, 35);
            score.alignment = TextAnchor.MiddleRight;

            this.percent = percent;
            percent.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 35);
            percent.alignment = TextAnchor.MiddleRight;

            this.grade = grade;
            grade.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);

            this.maxcombo = maxcombo;
            maxcombo.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 35);
            maxcombo.alignment = TextAnchor.MiddleRight;

            this.hasBackground = hasBackground;
            imageStrip = this.singleScore.transform.Find("Image").gameObject.GetComponent<Image>();
            imageStrip.color = Theme.rowEntryImageColor;
            this.singleScore.transform.Find("Image").gameObject.SetActive(hasBackground); //yep... ¯\_(ツ)_/¯
        }

        public void ToggleBackground()
        {
            hasBackground = !hasBackground;
            this.singleScore.transform.Find("Image").gameObject.SetActive(hasBackground);
        }

    }
}
