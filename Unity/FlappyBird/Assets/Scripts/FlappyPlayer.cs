using UnityEngine;

namespace Assets.Scripts
{
    class FlappyPlayer : MonoBehaviour
    {
        [SerializeField] private float flapForce = 10;
        [SerializeField] private float rotationMultiplier = 1;
        [SerializeField] private float minY = -10;
        [SerializeField] private float maxY = 10;
        [SerializeField] private float disableY = 20;
        [SerializeField] private Sprite[] birds = null;

        private Score score = null;

        public int GetScore
        {
            get
            {
                return score.Amount;
            }
        }

        public void StartGame()
        {
            GetComponent<Rigidbody2D>().isKinematic = false;
            Flap();
        }
        public void PauseGame()
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        public void Die()
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.up * flapForce;
            Destroy(GetComponent<Collider2D>());
        }

        public void UploadScore(string username)
        {
            score.StartUploadScore(username);
        }

        private void Flap()
        {
            if (GameController.SINGLETON.IsPlaying && transform.position.y <= maxY && (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")))
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.up * flapForce;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(GameController.SINGLETON.IsPlaying && collision.gameObject.tag == "ScoreTrigger")
            {
                score.Amount++;
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(GameController.SINGLETON.IsPlaying && collision.gameObject.tag == "Hazard")
            {
                GameController.SINGLETON.EndGame();
            }
        }

        private void PickRandomBird()
        {
            if (birds != null && birds.Length > 0)
            {
                int randNum = Random.Range(0, birds.Length);
                GetComponent<SpriteRenderer>().sprite = birds[randNum];
            }
        }

        private void Awake()
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            score = GetComponent<Score>();
            if (score == null) Debug.LogError("No score object found!");

            PickRandomBird();
        }
        private void Update()
        {
            Flap();

            if (GameController.SINGLETON.IsPlaying && transform.position.y <= minY) GameController.SINGLETON.EndGame();
            if (transform.position.y < disableY)
            {
                Destroy(gameObject.GetComponent<Rigidbody2D>());
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, GetComponent<Rigidbody2D>().velocity.y * rotationMultiplier);
            }
        }
    }
}
