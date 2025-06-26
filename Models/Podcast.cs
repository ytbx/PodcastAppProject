using System.ComponentModel.DataAnnotations.Schema;

namespace PodcastAppProcject.Models
{
    public class Podcast
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }

        public string AudioUrl { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public string UploaderId { get; set; }


        [ForeignKey(nameof(UploaderId))]
        public User? Uploader { get; set; }
        public List<Podcast> LikedByUsers { get; set; } = new();
    }

}
