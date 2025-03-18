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
            Console.WriteLine($"Initial admin setup attempt: Användarnamn={register.Användarnamn}, Roll={register.Roll}");
            if (_context.Användare.Any(u => u.Roll == Roller.Admin))
            {
                Console.WriteLine("Setup failed: An Admin already exists");
                return BadRequest(new { error = "En Admin-användare finns redan!" });
            }

            if (register.Roll != Roller.Admin)
            {
                Console.WriteLine("Setup failed: First user must be Admin");
                return BadRequest(new { error = "Den första användaren måste vara en Admin!" });
            }

            if (_context.Användare.Any(u => u.Användarnamn == register.Användarnamn))
            {
                Console.WriteLine($"Setup failed: Username '{register.Användarnamn}' already exists");
                return BadRequest(new { error = $"Användarnamnet '{register.Användarnamn}' finns redan!" });
            }

            try
            {
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
                var response = new { Token = token, Roll = användare.Roll.ToString() };
                Console.WriteLine($"Setup successful: Admin '{register.Användarnamn}' created, Response={System.Text.Json.JsonSerializer.Serialize(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Setup failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid skapande av admin. Kontakta support." });
            }
        }

        [HttpPost("registrera")]
        [Authorize(Roles = "Admin")]
        public IActionResult Registrera([FromBody] RegisterDTO register)
        {
            Console.WriteLine($"Register attempt: Användarnamn={register.Användarnamn}, Roll={register.Roll}");
            if (_context.Användare.Any(u => u.Användarnamn == register.Användarnamn))
            {
                Console.WriteLine($"Register failed: Username '{register.Användarnamn}' already exists");
                return BadRequest(new { error = $"Användarnamnet '{register.Användarnamn}' finns redan!" });
            }

            try
            {
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
                Console.WriteLine($"Register successful: User '{register.Användarnamn}' created");
                return Ok(new { message = "Användare registrerad!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid registrering av användare. Kontakta support." });
            }
        }

        [HttpPost("login")]
        public IActionResult LoggaIn([FromBody] LoginDTO login)
        {
            Console.WriteLine($"Login attempt: Användarnamn={login.Användarnamn}, Lösenord provided={login.Lösenord != null}");
            if (string.IsNullOrWhiteSpace(login.Användarnamn) || string.IsNullOrWhiteSpace(login.Lösenord))
            {
                Console.WriteLine("Login failed: Missing username or password");
                return BadRequest(new { error = "Användarnamn och lösenord krävs." });
            }

            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == login.Användarnamn);
            if (användare == null)
            {
                Console.WriteLine($"Login failed: User '{login.Användarnamn}' not found");
                return Unauthorized(new { error = "Fel användarnamn eller lösenord." });
            }

            if (användare.Låst)
            {
                Console.WriteLine($"Login failed: User '{login.Användarnamn}' is locked");
                return Unauthorized(new { error = "Kontot är låst." });
            }

            if (!BCrypt.Net.BCrypt.Verify(login.Lösenord, användare.LösenordHash))
            {
                Console.WriteLine($"Login failed: Incorrect password for '{login.Användarnamn}'");
                return Unauthorized(new { error = "Fel användarnamn eller lösenord." });
            }

            try
            {
                var token = SkapaJwtToken(användare);
                var response = new { Token = token, Roll = användare.Roll.ToString() };
                Console.WriteLine($"Login successful: Response={System.Text.Json.JsonSerializer.Serialize(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: Token generation error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid generering av token. Kontakta support." });
            }
        }

        [HttpDelete("ta-bort-användare/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult TaBortAnvändare(string användarnamn)
        {
            Console.WriteLine($"Delete user attempt: Användarnamn={användarnamn}");
            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null)
            {
                Console.WriteLine($"Delete failed: User '{användarnamn}' not found");
                return NotFound(new { error = $"Användaren '{användarnamn}' finns inte." });
            }

            try
            {
                _context.Användare.Remove(användare);
                _context.SaveChanges();
                Console.WriteLine($"Delete successful: User '{användarnamn}' removed");
                return Ok(new { message = "Användare borttagen!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid borttagning av användare. Kontakta support." });
            }
        }

        [HttpPut("lås-användare/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult LåsAnvändare(string användarnamn, [FromBody] bool låst)
        {
            Console.WriteLine($"Lock user attempt: Användarnamn={användarnamn}, Låst={låst}");
            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null)
            {
                Console.WriteLine($"Lock failed: User '{användarnamn}' not found");
                return NotFound(new { error = $"Användaren '{användarnamn}' finns inte." });
            }

            try
            {
                användare.Låst = låst;
                _context.SaveChanges();
                Console.WriteLine($"Lock successful: User '{användarnamn}' is now {(låst ? "låst" : "olåst")}");
                return Ok(new { message = $"Användare '{användarnamn}' är nu {(låst ? "låst" : "olåst")}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lock failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid låsning av användare. Kontakta support." });
            }
        }

        [HttpPut("ändra-lösenord/{användarnamn}")]
        [Authorize(Roles = "Admin")]
        public IActionResult ÄndraLösenord(string användarnamn, [FromBody] string nyttLösenord)
        {
            Console.WriteLine($"Change password attempt: Användarnamn={användarnamn}");
            if (string.IsNullOrWhiteSpace(nyttLösenord))
            {
                Console.WriteLine("Change password failed: New password is empty");
                return BadRequest(new { error = "Nytt lösenord får inte vara tomt!" });
            }

            var användare = _context.Användare.FirstOrDefault(u => u.Användarnamn == användarnamn);
            if (användare == null)
            {
                Console.WriteLine($"Change password failed: User '{användarnamn}' not found");
                return NotFound(new { error = $"Användaren '{användarnamn}' finns inte." });
            }

            try
            {
                användare.LösenordHash = BCrypt.Net.BCrypt.HashPassword(nyttLösenord);
                _context.SaveChanges();
                Console.WriteLine($"Change password successful: Password updated for '{användarnamn}'");
                return Ok(new { message = $"Lösenordet för användare '{användarnamn}' har uppdaterats!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid uppdatering av lösenord. Kontakta support." });
            }
        }

        private string SkapaJwtToken(Användare användare)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, användare.Användarnamn ?? "Unknown"),
                new Claim(ClaimTypes.Role, användare.Roll.ToString()),
                new Claim("Roll", användare.Roll.ToString())
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