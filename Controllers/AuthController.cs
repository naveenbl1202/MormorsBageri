using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MormorsBageri.Data;
using MormorsBageri.Models;
using MormorsBageri.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MormorsBageri.Enums;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("initial-admin-setup")]
        public IActionResult InitialAdminSetup([FromBody] RegisterDTO register)
        {
            if (_context.Användare.Any(u => u.Roll == Roller.Admin))
            {
                return BadRequest("En Admin-användare finns redan!");
            }

            if (register.Roll != Roller.Admin)
            {
                return BadRequest("Den första användaren måste vara en Admin!");
            }

            if (_context.Användare.Any(u => u.Användarnamn == register.Användarnamn))
            {
                return BadRequest("Användarnamnet finns redan!");
            }

            var användare = new Användare
            {
                Användarnamn = register.Användarnamn,
                LösenordHash = BCrypt.Net.BCrypt.HashPassword(register.Lösenord),
                Roll = Roller.Admin,
                Email = register.Email,
                Låst = false
            };

            _context.Användare.Add(användare);
            _context.SaveChanges();

            var token = SkapaJwtToken(användare);
            return Ok(new { Token = token });
        }

        [HttpPost("registrera")]
        [Authorize(Roles = "Admin")]
        public IActionResult Registrera([FromBody] RegisterDTO register)
        {
            if (_context.Användare.Any(u => u.Användarnamn == register.Användarnamn))
            {
                return BadRequest("Användarnamnet finns redan!");
            }

            var användare = new Användare
            {
                Användarnamn = register.Användarnamn,
                LösenordHash = BCrypt.Net.BCrypt.HashPassword(register.Lösenord),
                Roll = register.Roll,
                Email = register.Email,
                Låst = register.Låst
            };

            _context.Användare.Add(användare);
            _context.SaveChanges();
            return Ok("Användare registrerad!");
        }

        [HttpPost("login")]
        public IActionResult LoggaIn([FromBody] LoginDTO login)
        {
            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == login.Användarnamn);
            if (användare == null)
            {
                Console.WriteLine($"User '{login.Användarnamn}' not found.");
                return Unauthorized("Fel inloggning eller konto är låst");
            }
            if (användare.Låst)
            {
                Console.WriteLine($"User '{login.Användarnamn}' is locked.");
                return Unauthorized("Fel inloggning eller konto är låst");
            }
            if (!BCrypt.Net.BCrypt.Verify(login.Lösenord, användare.LösenordHash))
            {
                Console.WriteLine($"Password verification failed for user '{login.Användarnamn}'.");
                return Unauthorized("Fel inloggning eller konto är låst");
            }

            var token = SkapaJwtToken(användare);
            return Ok(new { Token = token });
        }

        [HttpDelete("ta-bort-användare/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult TaBortAnvändare(string användarnamn)
        {
            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null) return NotFound("Användaren finns inte");
            _context.Användare.Remove(användare);
            _context.SaveChanges();
            return Ok("Användare borttagen!");
        }

        [HttpPut("lås-användare/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult LåsAnvändare(string användarnamn, [FromBody] bool låst)
        {
            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null) return NotFound("Användaren finns inte");
            användare.Låst = låst;
            _context.SaveChanges();
            return Ok($"Användare {användarnamn} är nu {(låst ? "låst" : "olåst")}");
        }

        // New endpoint for Admin to change a user's password
        [HttpPut("ändra-lösenord/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult ÄndraLösenord(string användarnamn, [FromBody] string nyttLösenord)
        {
            if (string.IsNullOrWhiteSpace(nyttLösenord))
            {
                return BadRequest("Nytt lösenord får inte vara tomt!");
            }

            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null)
            {
                return NotFound("Användaren finns inte");
            }

            användare.LösenordHash = BCrypt.Net.BCrypt.HashPassword(nyttLösenord);
            _context.SaveChanges();
            return Ok($"Lösenordet för användare {användarnamn} har uppdaterats!");
        }

        private string SkapaJwtToken(Användare användare)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, användare.Användarnamn),
                new Claim(ClaimTypes.Role, användare.Roll.ToString())
            };

            var keyValue = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}