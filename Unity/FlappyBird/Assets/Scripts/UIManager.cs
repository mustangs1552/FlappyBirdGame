using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject startScreen = null;
        [SerializeField] private GameObject endScreen = null;
        [SerializeField] private Text score = null;
        [SerializeField] private InputField inputField = null;
        [SerializeField] private Button inputFieldButton = null;

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

        public void SaveHighScore()
        {
            if (inputField.text != null || inputField.text != "")
            {
                GameController.SINGLETON.UploadPlayerScore(inputField.text);
                inputField.enabled = false;
                inputFieldButton.enabled = false;
            }
        }

        private void Update()
        {
            score.text = GameController.SINGLETON.PlayerScore.ToString();
        }
    }
}
