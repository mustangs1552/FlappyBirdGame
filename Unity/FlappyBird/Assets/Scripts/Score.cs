using UnityEngine;
using MattRGeorge;

namespace Assets.Scripts
{
    public class Score : MonoBehaviour
    {
        public PortfolioSiteAccess siteAccess = null;

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
    }
}