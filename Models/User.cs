using Microsoft.AspNetCore.Identity;

namespace PodcastAppProcject.Models
{
    public class User:IdentityUser
    {
        public string Nick {  get; set; }
        public List<Podcast> UploadedPodcasts { get; set; } = new();
        public List<Podcast> LikedPodcasts { get; set; } = new();
    }
}
