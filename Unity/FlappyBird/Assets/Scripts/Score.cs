using UnityEngine;
using System.Collections.Generic;
using MattRGeorge;

namespace Assets.Scripts
{
    public class Score : MonoBehaviour
    {
        public PortfolioSiteAccess siteAccess = null;
        public bool isDebug = false;

        private int amount = 0;

        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
                if (amount < 0) amount = 0;
            }
        }

        public void StartUploadScore(string username)
        {
            siteAccess.StartUploadScore(username, "Points", amount);
        }

        public bool OnGotScores(List<ScoreValue> scores)
        {
            if (scores == null) return false;

            if (scores.Count == 0) Debug.Log("No scores!");
            else
            {
                string debugStr = "Scores received:";
                foreach (ScoreValue score in scores)
                {
                    debugStr += "\n\t- " + score.playerName + ", " + score.scoreAmount + ", " + score.scoreType;
                }
                Debug.Log(debugStr);
            }

            return true;
        }
        public void Start()
        {
            if (isDebug)
            {
                siteAccess.StartGettingScores(OnGotScores);
                siteAccess.StartGettingScores("Bat", OnGotScores);
                siteAccess.StartGettingTopScores(10, OnGotScores);
                siteAccess.StartGettingTopScores("Matt", 10, OnGotScores);
            }
        }
    }
}