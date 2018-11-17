using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject startScreen = null;
        [SerializeField] private GameObject endScreen = null;
        [SerializeField] private Text score = null;

        public void HideScreens()
        {
            startScreen.SetActive(false);
            endScreen.SetActive(false);
        }
        public void ShowStartScreen()
        {
            HideScreens();
            startScreen.SetActive(true);
        }
        public void ShowEndScreen()
        {
            HideScreens();
            endScreen.SetActive(true);
        }

        private void Update()
        {
            score.text = GameController.SINGLETON.PlayerScore.ToString();
        }
    }
}
