using UnityEngine;
using System.Collections.Generic;

namespace Assets.Scripts
{
    class GameController : MonoBehaviour
    {
        #region Variables
        public static GameController SINGLETON = null;

        [SerializeField] private float levelSpeed = 1;
        [SerializeField] private int totalPillars = 3;
        [SerializeField] private Pillar pillarPrefab = null;

        private FlappyPlayer player = null;
        private List<Pillar> pillars = new List<Pillar>();
        private float timeBetweenPillars = -1;
        #endregion

        private void Setup()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<FlappyPlayer>();
            if (player == null) Debug.LogError("Didn't find player!");

            if (pillarPrefab == null) Debug.LogError("No pillar prefab set!");

            SpawnPillar();
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
    }
}
