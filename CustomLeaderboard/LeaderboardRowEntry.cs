using TMPro;
using TootTally.Graphics;
using UnityEngine;
using UnityEngine.UI;

namespace TootTally.CustomLeaderboard
{
    public class LeaderboardRowEntry : MonoBehaviour
    {
        public GameObject singleScore;
        public TMP_Text rank, username, score, percent, grade, maxcombo;
        public Image imageStrip;
        public bool hasBackground;
        public string replayId;
        public int rowId;

        public void ConstructLeaderboardEntry(GameObject singleScore, TMP_Text rank, TMP_Text username, TMP_Text score, TMP_Text percent, TMP_Text grade, TMP_Text maxcombo, bool hasBackground = false)
        {
            this.singleScore = singleScore;
            this.rank = rank;
            rank.GetComponent<RectTransform>().sizeDelta = new Vector2(45, 35);
            rank.alignment = TextAlignmentOptions.MidlineLeft;

            this.username = username;
            username.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 35);
            username.alignment = TextAlignmentOptions.MidlineLeft;

            this.score = score;
            score.GetComponent<RectTransform>().sizeDelta = new Vector2(170, 35);
            score.alignment = TextAlignmentOptions.MidlineRight;

            this.percent = percent;
            percent.GetComponent<RectTransform>().sizeDelta = new Vector2(85, 35);
            percent.alignment = TextAlignmentOptions.MidlineRight;

            this.grade = grade;
            grade.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 35);

            this.maxcombo = maxcombo;
            maxcombo.GetComponent<RectTransform>().sizeDelta = new Vector2(55, 35);
            maxcombo.alignment = TextAlignmentOptions.MidlineRight;

            this.hasBackground = hasBackground;
            imageStrip = this.singleScore.transform.Find("Image").gameObject.GetComponent<Image>();
            imageStrip.gameObject.SetActive(hasBackground); //yep... ¯\_(ツ)_/¯
        }

        public void ToggleBackground()
        {
            hasBackground = !hasBackground;
            imageStrip.gameObject.SetActive(hasBackground);
        }

        public void UpdateTheme()
        {
            imageStrip.color = GameTheme.themeColors.leaderboard.rowEntry;
            rank.color = GameTheme.themeColors.leaderboard.headerText;
            username.color = score.color = percent.color = grade.color = maxcombo.color = GameTheme.themeColors.leaderboard.text;
            rank.outlineColor = username.outlineColor = score.outlineColor = percent.outlineColor = grade.outlineColor = maxcombo.outlineColor = GameTheme.themeColors.leaderboard.textOutline;
        }
    }
}
