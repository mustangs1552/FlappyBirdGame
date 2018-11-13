namespace Assets.Scripts
{
    public class Score
    {
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
    }
}
