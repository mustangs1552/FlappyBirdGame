namespace PersonalProjectSite.Models
{
    public class ReviewsModel
    {
        public int ReviewID { get; set; }
        public int GameID { get; set; }
        public string ReviewUsername { get; set; }
        public string ReviewMsg { get; set; }
        public int ReviewScore { get; set; }
    }
}
