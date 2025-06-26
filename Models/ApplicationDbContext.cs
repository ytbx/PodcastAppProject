using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PodcastAppProcject.Models
{
    public class ApplicationDbContext:IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }

        public DbSet<Podcast> Podcasts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Podcast>()
                .HasOne(p => p.Uploader)
                .WithMany(u => u.UploadedPodcasts)
                .HasForeignKey(p => p.UploaderId);
        }


    }
}
