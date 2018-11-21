namespace PersonalProjectSite.Models
{
    public class ReviewsModel
    {
        public uint ReviewID { get; set; }
        public uint GameID { get; set; }
        public string ReviewUsername { get; set; }
        public string ReviewMsg { get; set; }
        public uint ReviewScore { get; set; }
    }
}
