using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    class GameController : MonoBehaviour
    {
        #region Variables
        public static GameController SINGLETON = null;

        [SerializeField] private float levelSpeed = 1;
        [SerializeField] private int totalPillars = 3;
        [SerializeField] private Pillar pillarPrefab = null;
        [SerializeField] private UIManager uiManager = null;

        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
        }
        public int PlayerScore
        {
            get
            {
                return player.GetScore;
            }
        }

        private FlappyPlayer player = null;
        private List<Pillar> pillars = new List<Pillar>();
        private float timeBetweenPillars = -1;
        private bool isPlaying = false;
        private bool isStarted = false;
        #endregion

        public void StartGame()
        {
            if (!isStarted && !isPlaying)
            {
                isPlaying = true;
                isStarted = true;
                player.StartGame();

                SpawnPillar();
                uiManager.HideScreens();
            }
        }
        public void EndGame()
        {
            isPlaying = false;
            player.Die();
            CancelInvoke("SpawnPillar");
            uiManager.ShowEndScreen();
        }

        public void ReloadLvl()
        {
            SceneManager.LoadScene(0);
        }

        private void Setup()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<FlappyPlayer>();
            if (player == null) Debug.LogError("Didn't find player!");

            if (pillarPrefab == null) Debug.LogError("No pillar prefab set!");

            uiManager.ShowStartScreen();
        }
        private void SpawnPillar()
        {
            if(pillars.Count < totalPillars)
            {
                Pillar newPillar = Instantiate(pillarPrefab).GetComponent<Pillar>();
                if (newPillar != null)
                {
                    pillars.Add(newPillar);
                    newPillar.Speed = levelSpeed;
                    if(pillars.Count == totalPillars) CancelInvoke("SpawnPillar");

                    if (timeBetweenPillars <= 0)
                    {
                        float dist = newPillar.GetDistToOtherside();
                        float distBetweenPillars = dist / (float)totalPillars;
                        timeBetweenPillars = distBetweenPillars / levelSpeed;
                        InvokeRepeating("SpawnPillar", timeBetweenPillars, timeBetweenPillars);
                    }
                }
                else Debug.LogError("No pillar component found on spawned pillar!");
            }
            else
            {
                foreach(Pillar pillar in pillars)
                {
                    if(!pillar.gameObject.activeInHierarchy)
                    {
                        pillar.gameObject.SetActive(true);
                        return;
                    }
                }

                CancelInvoke("SpawnPillar");
            }
        }

        private void Awake()
        {
            if (GameController.SINGLETON == null) SINGLETON = this;
            else Debug.LogError("More than one GameController singletons found!");

            Setup();
        }
        private void Update()
        {
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) StartGame();
        }
    }
}
