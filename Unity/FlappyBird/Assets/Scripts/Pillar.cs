using UnityEngine;

namespace Assets.Scripts
{
    class Pillar : ScrollingObj
    {
        [SerializeField] private float maxHeight = 5;
        [SerializeField] private float minHeight = -5;

        public float GetDistToOtherside()
        {
            return maxRight - maxLeft;
        }

        private void ResetPosition()
        {
            float randNum = Random.Range(minHeight, maxHeight);
            transform.position = new Vector3(maxRight, randNum, transform.position.z);
        }

        protected override void InitialReset()
        {
            ResetPosition();
        }
        protected override void ResetToOtherSide()
        {
            ResetPosition();
        }
    }
}
