using UnityEngine;

namespace Assets.Scripts
{
    class FlappyPlayer : MonoBehaviour
    {
        private Score score = new Score();

        public int GetScore
        {
            get
            {
                return score.Amount;
            }
        }
    }
}
