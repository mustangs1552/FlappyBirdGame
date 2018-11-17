using UnityEngine;

namespace Assets.Scripts
{
    class ScrollingObj : MonoBehaviour
    {
        [SerializeField] protected float maxLeft = -10;
        [SerializeField] protected float maxRight = 10;

        private float speed = -1;

        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        protected virtual void InitialReset()
        {
            ResetToOtherSide();
        }

        protected virtual void ResetToOtherSide()
        {
            transform.position = new Vector3(maxRight, transform.position.y, transform.position.z);
        }
        private void CheckBoundries()
        {
            if (transform.position.x < maxLeft) ResetToOtherSide();
        }
        private void MoveLeft()
        {
            if (GameController.SINGLETON.IsPlaying && speed != -1)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
                CheckBoundries();
            }
            else if(speed == -1) Debug.LogError("No speed set!");
        }

        private void OnEnable()
        {
            InitialReset();
        }
        private void Start()
        {
            InitialReset();
        }
        private void Update()
        {
            MoveLeft();
        }
    }
}
