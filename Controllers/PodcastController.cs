using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastAppProcject.Models;
using System.Security.Claims;
using PodcastAppProcject.Dtos;

namespace PodcastAppProcject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PodcastController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PodcastController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var podcasts = await _context.Podcasts
                .Include(p => p.Uploader)
                .ToListAsync();

            var result = podcasts.Select(p => new PodcastDto
            {
                Id = p.Id,
                Title = p.Title,
                AudioUrl = p.AudioUrl,
                ImageUrl = p.ImageUrl,
                ViewCount = p.ViewCount,
                LikeCount = p.LikeCount,
                UploaderUserName = p.Uploader?.UserName
            }).ToList();

            return Ok(result);
        }




        [HttpPost]
        
        public async Task<IActionResult> Create([FromForm] PodcastCreateDto dto)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null) return Unauthorized();

           
            var imageUrl = await SaveFile(dto.ImageFile, "images");
            var audioUrl = await SaveFile(dto.AudioFile, "audios");

            var podcast = new Podcast
            {
                Title = dto.Title,
                ImageUrl = imageUrl,
                AudioUrl = audioUrl,
                ViewCount = 0,
                LikeCount = 0,
                UploaderId = userId
            };

            _context.Podcasts.Add(podcast);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = podcast.Id }, podcast);
        }

     
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var podcast = await _context.Podcasts.Include(p => p.Uploader).FirstOrDefaultAsync(p => p.Id == id);
            if (podcast == null) return NotFound();
            return Ok(podcast);
        }

        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            var uploadDir = Path.Combine(_env.WebRootPath, folderName);
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

          
            return $"/{folderName}/{fileName}";
        }
    }

    public class PodcastCreateDto
    {
        public string Title { get; set; }

        public IFormFile ImageFile { get; set; }
        public IFormFile AudioFile { get; set; }
    }
}
