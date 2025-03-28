using Microsoft.AspNetCore.Mvc;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ButikerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ButikerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Säljare,Planerare")]
        public async Task<IActionResult> HämtaButiker()
        {
            try
            {
                var butiker = await _context.Butiker.ToListAsync();
                Console.WriteLine($"Fetched {butiker.Count} butiker by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                Console.WriteLine("Butiker data: " + JsonSerializer.Serialize(butiker, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"Number of butiker with non-null ButikNamn: {butiker.Count(b => !string.IsNullOrEmpty(b.ButikNamn))}");
                return Ok(butiker);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching butiker: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av butiker. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> LäggTillButik([FromBody] Butik butik)
        {
            if (butik == null)
            {
                Console.WriteLine("Add butik failed: Butik data is null");
                return BadRequest(new { error = "Butikdata saknas." });
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Add butik failed: Invalid model state");
                return BadRequest(ModelState);
            }

            if (await _context.Butiker.AnyAsync(b => b.ButikNamn == butik.ButikNamn))
            {
                Console.WriteLine($"Add butik failed: Butik '{butik.ButikNamn}' already exists");
                return Conflict(new { error = $"Butiken '{butik.ButikNamn}' finns redan." });
            }

            try
            {
                butik.Låst ??= false;
                _context.Butiker.Add(butik);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Added butik: {JsonSerializer.Serialize(butik, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return CreatedAtAction(nameof(HämtaButiker), new { id = butik.ButikId }, butik);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding butik: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid tillägg av butik. Kontakta support." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UppdateraButik(int id, [FromBody] Butik uppdateradButik)
        {
            if (id != uppdateradButik.ButikId)
            {
                Console.WriteLine($"Update butik failed: ID mismatch (URL: {id}, Body: {uppdateradButik.ButikId})");
                return BadRequest(new { error = "ID i URL och kropp matchar inte." });
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Update butik failed: Invalid model state");
                return BadRequest(ModelState);
            }

            var butik = await _context.Butiker.FirstOrDefaultAsync(b => b.ButikId == id);
            if (butik == null)
            {
                Console.WriteLine($"Update butik failed: Butik with ID {id} not found");
                return NotFound(new { error = $"Butik med ID {id} finns inte." });
            }

            if (await _context.Butiker.AnyAsync(b => b.ButikNamn == uppdateradButik.ButikNamn && b.ButikId != id))
            {
                Console.WriteLine($"Update butik failed: Butik '{uppdateradButik.ButikNamn}' already exists");
                return Conflict(new { error = $"Butiken '{uppdateradButik.ButikNamn}' finns redan." });
            }

            try
            {
                butik.ButikNummer = uppdateradButik.ButikNummer;
                butik.ButikNamn = uppdateradButik.ButikNamn;
                butik.Telefonnummer = uppdateradButik.Telefonnummer;
                butik.Besöksadress = uppdateradButik.Besöksadress;
                butik.Fakturaadress = uppdateradButik.Fakturaadress;
                butik.ButikschefNamn = uppdateradButik.ButikschefNamn;
                butik.ButikschefTelefon = uppdateradButik.ButikschefTelefon;
                butik.BrödansvarigNamn = uppdateradButik.BrödansvarigNamn;
                butik.BrödansvarigTelefon = uppdateradButik.BrödansvarigTelefon;
                butik.Låst = uppdateradButik.Låst;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated butik: {JsonSerializer.Serialize(butik, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Butik med ID {id} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating butik: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av butik. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> TaBortButik(int id)
        {
            try
            {
                var butik = await _context.Butiker.FirstOrDefaultAsync(b => b.ButikId == id);
                if (butik == null)
                {
                    Console.WriteLine($"Delete butik failed: Butik with ID {id} not found");
                    return NotFound(new { error = $"Butik med ID {id} finns inte." });
                }

                if (await _context.Beställningar.AnyAsync(b => b.ButikId == id))
                {
                    Console.WriteLine($"Delete butik failed: Butik '{butik.ButikNamn}' is linked to existing orders");
                    return BadRequest(new { error = $"Kan inte ta bort butiken '{butik.ButikNamn}' eftersom den är kopplad till befintliga beställningar." });
                }

                _context.Butiker.Remove(butik);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted butik with ID {id} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Butik med ID {id} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting butik: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av butik. Kontakta support." });
            }
        }

        [HttpPut("{id}/lås")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LåsButik(int id, [FromBody] bool låst)
        {
            try
            {
                var butik = await _context.Butiker.FirstOrDefaultAsync(b => b.ButikId == id);
                if (butik == null)
                {
                    Console.WriteLine($"Lock butik failed: Butik with ID {id} not found");
                    return NotFound(new { error = $"Butik med ID {id} finns inte." });
                }

                butik.Låst = låst;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated Låst status for butik ID {id} to {låst} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Butik '{butik.ButikNamn}' är nu {(låst ? "låst" : "olåst")}." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error locking/unlocking butik {id}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid låsning av butik. Kontakta support." });
            }
        }
    }
}