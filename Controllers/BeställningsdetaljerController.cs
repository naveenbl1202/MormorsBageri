using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BeställningsdetaljerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BeställningsdetaljerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /api/beställningsdetaljer
        // Fetches all Beställningsdetaljer
        [HttpGet]
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetAllBeställningsdetaljer()
        {
            try
            {
                var query = _context.Beställningsdetaljer
                    .Include(bd => bd.Produkt)
                    .AsQueryable();

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    Console.WriteLine($"User is Säljare, filtering by username: {username}");
                    if (!string.IsNullOrEmpty(username))
                    {
                        query = query.Where(bd => _context.Beställningar
                            .Where(b => b.Säljare == username)
                            .Select(b => b.BeställningId)
                            .Contains(bd.BeställningId));
                    }
                }

                var beställningsdetaljer = await query.ToListAsync();

                Console.WriteLine($"Fetched {beställningsdetaljer.Count} Beställningsdetaljer by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(beställningsdetaljer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Beställningsdetaljer: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningsdetaljer. Kontakta support." });
            }
        }

        // GET: /api/beställningsdetaljer/beställning/{beställningId}
        // Fetches all Beställningsdetaljer for a specific Beställning
        [HttpGet("beställning/{beställningId}")]
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetBeställningsdetaljerByBeställning(int beställningId)
        {
            try
            {
                var beställning = await _context.Beställningar
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .FirstOrDefaultAsync(b => b.BeställningId == beställningId);

                if (beställning == null)
                {
                    Console.WriteLine($"Fetch Beställningsdetaljer failed: Beställning with ID {beställningId} not found");
                    return NotFound(new { error = $"Beställning med ID {beställningId} finns inte." });
                }

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username) && beställning.Säljare != username)
                    {
                        Console.WriteLine($"Fetch Beställningsdetaljer failed: Säljare '{username}' is not authorized to view Beställning {beställningId}");
                        return Forbid("Du har inte behörighet att se denna beställnings detaljer.");
                    }
                }

                Console.WriteLine($"Fetched {beställning.Beställningsdetaljer.Count} Beställningsdetaljer for Beställning {beställningId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(beställning.Beställningsdetaljer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Beställningsdetaljer for Beställning {beställningId}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningsdetaljer. Kontakta support." });
            }
        }

        // GET: /api/beställningsdetaljer/produkt/{produktId}
        // Fetches all Beställningsdetaljer for a specific Produkt
        [HttpGet("produkt/{produktId}")]
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetBeställningsdetaljerByProdukt(int produktId)
        {
            try
            {
                var produkt = await _context.Produkter.FindAsync(produktId);
                if (produkt == null)
                {
                    Console.WriteLine($"Fetch Beställningsdetaljer failed: Produkt with ID {produktId} not found");
                    return NotFound(new { error = $"Produkt med ID {produktId} finns inte." });
                }

                var query = _context.Beställningsdetaljer
                    .Include(bd => bd.Produkt)
                    .Where(bd => bd.ProduktId == produktId)
                    .AsQueryable();

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    Console.WriteLine($"User is Säljare, filtering by username: {username}");
                    if (!string.IsNullOrEmpty(username))
                    {
                        query = query.Where(bd => _context.Beställningar
                            .Where(b => b.Säljare == username)
                            .Select(b => b.BeställningId)
                            .Contains(bd.BeställningId));
                    }
                }

                var beställningsdetaljer = await query.ToListAsync();

                Console.WriteLine($"Fetched {beställningsdetaljer.Count} Beställningsdetaljer for Produkt {produktId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(beställningsdetaljer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Beställningsdetaljer for Produkt {produktId}: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningsdetaljer för produkt. Kontakta support." });
            }
        }

        // POST: /api/beställningsdetaljer/{beställningId}
        // Adds a new Beställningsdetalj to an existing Beställning
        [HttpPost("{beställningId}")]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> AddBeställningsdetalj(int beställningId, [FromBody] Beställningsdetalj detalj)
        {
            if (detalj == null)
            {
                Console.WriteLine("Add Beställningsdetalj failed: Detalj data is null");
                return BadRequest(new { error = "Detaljdata saknas." });
            }

            if (detalj.Antal < 1)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Antal ({detalj.Antal}) must be at least 1");
                return BadRequest(new { error = "Antal måste vara minst 1." });
            }

            if (detalj.Styckpris < 0)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Styckpris ({detalj.Styckpris}) cannot be negative");
                return BadRequest(new { error = "Styckpris får inte vara negativt." });
            }

            if (detalj.Rabatt < 0)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Rabatt ({detalj.Rabatt}) cannot be negative");
                return BadRequest(new { error = "Rabatt får inte vara negativt." });
            }

            if (detalj.Rabatt > detalj.Styckpris)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Rabatt ({detalj.Rabatt}) exceeds Styckpris ({detalj.Styckpris}) for ProduktId {detalj.ProduktId}");
                return BadRequest(new { error = $"Rabatt ({detalj.Rabatt}) får inte överstiga styckpriset ({detalj.Styckpris}) för produkt med ID {detalj.ProduktId}." });
            }

            var beställning = await _context.Beställningar
                .Include(b => b.Beställningsdetaljer)
                .FirstOrDefaultAsync(b => b.BeställningId == beställningId);

            if (beställning == null)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Beställning with ID {beställningId} not found");
                return NotFound(new { error = $"Beställning med ID {beställningId} finns inte." });
            }

            var produkt = await _context.Produkter.FindAsync(detalj.ProduktId);
            if (produkt == null)
            {
                Console.WriteLine($"Add Beställningsdetalj failed: Produkt with ID {detalj.ProduktId} not found");
                return BadRequest(new { error = $"Produkt med ID {detalj.ProduktId} finns inte." });
            }

            try
            {
                detalj.BeställningId = beställningId;
                beställning.Beställningsdetaljer.Add(detalj);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Added Beställningsdetalj to Beställning {beställningId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Detalj tillagd till beställning {beställningId}." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding Beställningsdetalj: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid tillägg av detalj. Kontakta support." });
            }
        }

        // PUT: /api/beställningsdetaljer/{detaljId}
        // Updates an existing Beställningsdetalj
        [HttpPut("{detaljId}")]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> UpdateBeställningsdetalj(int detaljId, [FromBody] Beställningsdetalj updatedDetalj)
        {
            var detalj = await _context.Beställningsdetaljer
                .FirstOrDefaultAsync(d => d.BeställningsdetaljId == detaljId);

            if (detalj == null)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Detalj with ID {detaljId} not found");
                return NotFound(new { error = $"Detalj med ID {detaljId} finns inte." });
            }

            if (updatedDetalj.Antal < 1)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Antal ({updatedDetalj.Antal}) must be at least 1");
                return BadRequest(new { error = "Antal måste vara minst 1." });
            }

            if (updatedDetalj.Styckpris < 0)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Styckpris ({updatedDetalj.Styckpris}) cannot be negative");
                return BadRequest(new { error = "Styckpris får inte vara negativt." });
            }

            if (updatedDetalj.Rabatt < 0)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Rabatt ({updatedDetalj.Rabatt}) cannot be negative");
                return BadRequest(new { error = "Rabatt får inte vara negativt." });
            }

            var produkt = await _context.Produkter.FindAsync(updatedDetalj.ProduktId);
            if (produkt == null)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Produkt with ID {updatedDetalj.ProduktId} not found");
                return BadRequest(new { error = $"Produkt med ID {updatedDetalj.ProduktId} finns inte." });
            }

            if (updatedDetalj.Rabatt > updatedDetalj.Styckpris)
            {
                Console.WriteLine($"Update Beställningsdetalj failed: Rabatt ({updatedDetalj.Rabatt}) exceeds Styckpris ({updatedDetalj.Styckpris}) for ProduktId {updatedDetalj.ProduktId}");
                return BadRequest(new { error = $"Rabatt ({updatedDetalj.Rabatt}) får inte överstiga styckpriset ({updatedDetalj.Styckpris}) för produkt med ID {updatedDetalj.ProduktId}." });
            }

            try
            {
                detalj.ProduktId = updatedDetalj.ProduktId;
                detalj.Antal = updatedDetalj.Antal;
                detalj.Styckpris = updatedDetalj.Styckpris;
                detalj.Rabatt = updatedDetalj.Rabatt;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated Beställningsdetalj {detaljId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Detalj med ID {detaljId} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Beställningsdetalj: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av detalj. Kontakta support." });
            }
        }

        // DELETE: /api/beställningsdetaljer/{detaljId}
        // Deletes a Beställningsdetalj
        [HttpDelete("{detaljId}")]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> DeleteBeställningsdetalj(int detaljId)
        {
            var detalj = await _context.Beställningsdetaljer
                .FirstOrDefaultAsync(d => d.BeställningsdetaljId == detaljId);

            if (detalj == null)
            {
                Console.WriteLine($"Delete Beställningsdetalj failed: Detalj with ID {detaljId} not found");
                return NotFound(new { error = $"Detalj med ID {detaljId} finns inte." });
            }

            try
            {
                _context.Beställningsdetaljer.Remove(detalj);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted Beställningsdetalj {detaljId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Detalj med ID {detaljId} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting Beställningsdetalj: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av detalj. Kontakta support." });
            }
        }
    }
}