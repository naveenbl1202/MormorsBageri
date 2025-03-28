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
using MormorsBageri.Services;
using System.Security.Cryptography;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private static readonly Dictionary<string, (string Token, DateTime Expiration)> _resetTokens = new Dictionary<string, (string, DateTime)>(); // In-memory token store

        public AuthController(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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
                var response = new { Token = token, Roll = användare.Roll.ToString() }; // Explicitly convert to string
                Console.WriteLine($"Setup successful: Admin '{register.Användarnamn ?? "Unknown"}' created, Response={System.Text.Json.JsonSerializer.Serialize(response)}");
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
            Console.WriteLine($"Register attempt: Användarnamn={register.Användarnamn}, Roll={register.Roll}, Email={register.Email}, Låst={register.Låst}");
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
                Console.WriteLine($"Register successful: User '{register.Användarnamn ?? "Unknown"}' created by Admin '{User.Identity.Name}'");
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
                var response = new { Token = token, Roll = användare.Roll.ToString() }; // Explicitly convert to string
                Console.WriteLine($"Login successful: User '{login.Användarnamn ?? "Unknown"}' logged in, Response={System.Text.Json.JsonSerializer.Serialize(response)}");
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
            Console.WriteLine($"Delete user attempt: Användarnamn={användarnamn} by Admin '{User.Identity.Name}'");
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
                Console.WriteLine($"Delete successful: User '{användarnamn}' removed by Admin '{User.Identity.Name}'");
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
            Console.WriteLine($"Lock user attempt: Användarnamn={användarnamn}, Låst={låst} by Admin '{User.Identity.Name}'");
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
                Console.WriteLine($"Lock successful: User '{användarnamn}' is now {(låst ? "låst" : "olåst")} by Admin '{User.Identity.Name}'");
                return Ok(new { message = $"Användare '{användarnamn}' är nu {(låst ? "låst" : "olåst")}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lock failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid låsning av användare. Kontakta support." });
            }
        }

        [HttpPut("ändra-lösenord/{användarnamn}")]
        [Authorize]
        public IActionResult ÄndraLösenord(string användarnamn, [FromBody] string nyttLösenord)
        {
            Console.WriteLine($"Change password attempt: Användarnamn={användarnamn} by user '{User.Identity.Name}'");
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

            var currentUser = User.FindFirst(ClaimTypes.Name)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUser != användarnamn && currentUserRole != Roller.Admin.ToString())
            {
                Console.WriteLine($"Change password failed: User '{currentUser}' is not authorized to change password for '{användarnamn}'");
                return Forbid("Du har inte behörighet att ändra lösenord för denna användare.");
            }

            try
            {
                användare.LösenordHash = BCrypt.Net.BCrypt.HashPassword(nyttLösenord);
                _context.SaveChanges();
                Console.WriteLine($"Change password successful: Password updated for '{användarnamn}' by user '{currentUser}'");
                return Ok(new { message = $"Lösenordet för användare '{användarnamn}' har uppdaterats!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Change password failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid uppdatering av lösenord. Kontakta support." });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            Console.WriteLine("Get current user attempt");
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Get current user failed: User not authenticated");
                return Unauthorized(new { error = "Användare inte autentiserad." });
            }

            var user = _context.Användare.FirstOrDefault(u => u.Användarnamn == username);
            if (user == null)
            {
                Console.WriteLine($"Get current user failed: User '{username}' not found");
                return NotFound(new { error = $"Användare med användarnamn {username} hittades inte." });
            }

            Console.WriteLine($"Get current user successful: User '{username}' found");
            return Ok(user);
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            Console.WriteLine($"Forgot password attempt: Email={request.Email}");
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                Console.WriteLine("Forgot password failed: Email is empty");
                return BadRequest(new { error = "Email-adress krävs." });
            }

            var user = _context.Användare.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                Console.WriteLine($"Forgot password failed: User with email '{request.Email}' not found");
                return NotFound(new { error = $"Användare med email '{request.Email}' hittades inte." });
            }

            try
            {
                // Generate a reset token
                var token = Guid.NewGuid().ToString();
                var expiration = DateTime.UtcNow.AddHours(1); // Token expires in 1 hour
                _resetTokens[user.Användarnamn!] = (token, expiration);

                // Generate the reset link
                var resetLink = $"http://localhost:5173/reset-password?token={token}&username={user.Användarnamn}";

                // Send the email
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    Console.WriteLine($"Forgot password failed: User '{user.Användarnamn}' has no email address");
                    return BadRequest(new { error = "Användaren har ingen email-adress registrerad." });
                }

                _emailService.SendEmail(
                    user.Email,
                    "Återställ ditt lösenord",
                    $"Hej {user.Användarnamn},\n\n" +
                    $"Du har begärt att återställa ditt lösenord. Klicka på följande länk för att återställa ditt lösenord:\n" +
                    $"{resetLink}\n\n" +
                    $"Länken är giltig i 1 timme. Om du inte begärde denna återställning, ignorera detta email.\n\n" +
                    "Vänliga hälsningar,\nMormors Bageri"
                );

                Console.WriteLine($"Forgot password successful: Reset email sent to '{user.Email}' with token '{token}'");
                return Ok(new { message = "En länk för att återställa ditt lösenord har skickats till din email." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Forgot password failed: Error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid skickande av återställningsemail. Kontakta support." });
            }
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO request)
        {
            Console.WriteLine($"Reset password attempt: Username={request.Username}, Token={request.Token}");
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                Console.WriteLine("Reset password failed: Missing required fields");
                return BadRequest(new { error = "Användarnamn, token och nytt lösenord krävs." });
            }

            var user = _context.Användare.FirstOrDefault(u => u.Användarnamn == request.Username);
            if (user == null)
            {
                Console.WriteLine($"Reset password failed: User '{request.Username}' not found");
                return NotFound(new { error = $"Användaren '{request.Username}' finns inte." });
            }

            if (!_resetTokens.TryGetValue(request.Username, out var tokenData) || tokenData.Token != request.Token)
            {
                Console.WriteLine($"Reset password failed: Invalid token for user '{request.Username}'");
                return BadRequest(new { error = "Ogiltig eller utgången token." });
            }

            if (tokenData.Expiration < DateTime.UtcNow)
            {
                Console.WriteLine($"Reset password failed: Token expired for user '{request.Username}'");
                _resetTokens.Remove(request.Username); // Clean up expired token
                return BadRequest(new { error = "Token har gått ut. Begär en ny återställningslänk." });
            }

            try
            {
                user.LösenordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _context.SaveChanges();
                _resetTokens.Remove(request.Username); // Clean up used token
                Console.WriteLine($"Reset password successful: Password updated for '{request.Username}'");
                return Ok(new { message = "Lösenordet har återställts!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset password failed: Database error - {ex.Message}");
                return StatusCode(500, new { error = "Fel vid återställning av lösenord. Kontakta support." });
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