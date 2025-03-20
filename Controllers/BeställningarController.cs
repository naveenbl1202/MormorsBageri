// BeställningarController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;
using MormorsBageri.Enums;
using MormorsBageri.Services;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BeställningarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public BeställningarController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Säljare,Planerare")]
        public async Task<ActionResult<IEnumerable<Beställning>>> HämtaBeställningar(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? butikId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.Beställningar
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .Include(b => b.Butik)
                    .AsQueryable();

                if (butikId.HasValue)
                    query = query.Where(b => b.ButikId == butikId.Value);
                if (startDate.HasValue)
                    query = query.Where(b => b.Beställningsdatum >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(b => b.Beställningsdatum <= endDate.Value);

                var total = await query.CountAsync();
                var beställningar = await query
                    .OrderBy(b => b.Beställningsdatum)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                Console.WriteLine($"Fetched {beställningar.Count} beställningar: {System.Text.Json.JsonSerializer.Serialize(beställningar)}");

                return Ok(new
                {
                    TotalItems = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Data = beställningar
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching beställningar: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningar. Kontakta support." });
            }
        }

        [HttpGet("{butikId}")]
        [Authorize(Roles = "Admin,Säljare,Planerare")]
        public async Task<ActionResult<IEnumerable<Beställning>>> HämtaBeställningarFörButik(int butikId)
        {
            try
            {
                var beställningar = await _context.Beställningar
                    .Where(b => b.ButikId == butikId)
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .Include(b => b.Butik)
                    .ToListAsync();

                Console.WriteLine($"Fetched {beställningar.Count} beställningar for butik {butikId}: {System.Text.Json.JsonSerializer.Serialize(beställningar)}");
                return Ok(beställningar);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching beställningar för butik {butikId}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningar för butik. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> SkapaBeställning([FromBody] Beställning beställning)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var butik = await _context.Butiker.FirstOrDefaultAsync(b => b.ButikId == beställning.ButikId);
                if (butik == null)
                {
                    return BadRequest(new { error = "Butiken finns inte." });
                }
                if (butik.Låst == true)
                {
                    return BadRequest(new { error = $"Butiken '{butik.ButikNamn}' är låst och kan inte ta emot beställningar." });
                }

                beställning.Beställningsdatum = DateTime.Now;
                beställning.PreliminärtLeveransdatum = DateTime.Now.AddDays(2);
                _context.Beställningar.Add(beställning);
                await _context.SaveChangesAsync();

                var planerare = await _context.Användare.Where(u => u.Roll == Roller.Planerare).ToListAsync();
                var admins = await _context.Användare.Where(u => u.Roll == Roller.Admin).ToListAsync();
                foreach (var user in planerare.Concat(admins))
                {
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendEmailAsync(
                            user.Email,
                            $"Ny beställning #{beställning.BeställningId} skapad",
                            $"En ny beställning har skapats av {beställning.Säljare}. Vänligen granska och bekräfta.\nOrder ID: {beställning.BeställningId}"
                        );
                    }
                }

                Console.WriteLine($"Created beställning: {System.Text.Json.JsonSerializer.Serialize(beställning)}");
                return CreatedAtAction(nameof(HämtaBeställningar), new { id = beställning.BeställningId }, beställning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating beställning: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid skapande av beställning. Kontakta support." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Planerare")]
        public async Task<IActionResult> UppdateraBeställning(int id, [FromBody] Beställning uppdateradBeställning)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var beställning = await _context.Beställningar
                    .Include(b => b.Beställningsdetaljer)
                    .FirstOrDefaultAsync(b => b.BeställningId == id);
                if (beställning == null)
                {
                    return NotFound(new { error = $"Beställning med ID {id} hittades inte." });
                }

                beställning.Beställare = uppdateradBeställning.Beställare;
                beställning.PreliminärtLeveransdatum = uppdateradBeställning.PreliminärtLeveransdatum;
                beställning.Beställningsdetaljer = uppdateradBeställning.Beställningsdetaljer;
                await _context.SaveChangesAsync();

                var säljare = await _context.Användare.FirstOrDefaultAsync(u => u.Användarnamn == beställning.Säljare);
                if (säljare != null && !string.IsNullOrEmpty(säljare.Email))
                {
                    await _emailService.SendEmailAsync(
                        säljare.Email,
                        $"Beställning #{id} bekräftad",
                        $"Din beställning har bekräftats av en planerare.\nOrder ID: {id}"
                    );
                }

                Console.WriteLine($"Updated beställning {id}: {System.Text.Json.JsonSerializer.Serialize(beställning)}");
                return Ok(beställning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating beställning {id}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av beställning. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Planerare")]
        public async Task<IActionResult> TaBortBeställning(int id)
        {
            try
            {
                var beställning = await _context.Beställningar.FirstOrDefaultAsync(b => b.BeställningId == id);
                if (beställning == null)
                {
                    return NotFound(new { error = $"Beställning med ID {id} hittades inte." });
                }

                var säljare = await _context.Användare.FirstOrDefaultAsync(u => u.Användarnamn == beställning.Säljare);
                var admins = await _context.Användare.Where(u => u.Roll == Roller.Admin).ToListAsync();

                _context.Beställningar.Remove(beställning);
                await _context.SaveChangesAsync();

                if (säljare != null && !string.IsNullOrEmpty(säljare.Email))
                {
                    await _emailService.SendEmailAsync(
                        säljare.Email,
                        $"Beställning #{id} borttagen",
                        $"Din beställning har tagits bort av en planerare.\nOrder ID: {id}"
                    );
                }
                foreach (var admin in admins)
                {
                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        await _emailService.SendEmailAsync(
                            admin.Email,
                            $"Beställning #{id} borttagen",
                            $"Beställning #{id} har tagits bort av en planerare."
                        );
                    }
                }

                Console.WriteLine($"Deleted beställning with ID {id}");
                return Ok(new { message = $"Beställning med ID {id} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting beställning {id}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av beställning. Kontakta support." });
            }
        }
    }
}