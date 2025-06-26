using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PodcastAppProcject.Models;
using PodcastAppProcject.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace PodcastAppProcject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;

        public AuthController(
            IConfiguration config,
            ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            EmailService emailService)
        {
            _config = config;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        private async Task<string> GenerateToken(User user)
        {
            var key = _config["Jwt:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(string name, string nick, string password)
        {
            var check = await _userManager.FindByNameAsync(name);
            if (check != null)
                return BadRequest("Bu kullanıcı adı zaten alınmış.");

            var user = new User { UserName = name, Nick = nick };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "ADMIN"); // İstersen burada "USER" yapabilirsin

            return Ok(new { message = "Kayıt başarılı" });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(string name, string password)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var loginResult = await _signInManager.PasswordSignInAsync(name, password, false, false);

            if (!loginResult.Succeeded)
                return BadRequest("Giriş başarısız.");

            var token = await GenerateToken(user);

            return Ok(new { Token = token });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Bu e-posta adresine ait kullanıcı bulunamadı.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var resetLink = $"{model.ClientUrl}?email={model.Email}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                model.Email,
                "Şifre Sıfırlama",
                $"<p>Şifrenizi sıfırlamak için <a href='{resetLink}'>buraya tıklayın</a></p>"
            );

            return Ok("Şifre sıfırlama bağlantısı e-postanıza gönderildi.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            var decodedToken = HttpUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Şifreniz başarıyla sıfırlandı.");
        }

        public class ForgotPasswordDto
        {
            public string Email { get; set; }
            public string ClientUrl { get; set; }
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
