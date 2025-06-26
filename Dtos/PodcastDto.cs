namespace PodcastAppProcject.Dtos
{
    public class PodcastDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AudioUrl { get; set; }
        public string ImageUrl { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public string UploaderUserName { get; set; }
    }


}
