using Microsoft.AspNetCore.Mvc;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult HämtaButiker()
        {
            try
            {
                var butiker = _context.Butiker.ToList();
                Console.WriteLine($"Fetched {butiker.Count} butiker");
                return Ok(butiker);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching butiker: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av butiker. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Säljare")] // Updated: Added Säljare
        public IActionResult LäggTillButik([FromBody] Butik butik)
        {
            if (butik == null)
            {
                return BadRequest(new { error = "Butikdata saknas." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Butiker.Any(b => b.ButikNamn == butik.ButikNamn))
            {
                return Conflict(new { error = $"Butiken '{butik.ButikNamn}' finns redan." });
            }

            try
            {
                _context.Butiker.Add(butik);
                _context.SaveChanges();
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
        public IActionResult UppdateraButik(int id, [FromBody] Butik uppdateradButik)
        {
            if (id != uppdateradButik.ButikId)
            {
                return BadRequest(new { error = "ID i URL och kropp matchar inte." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var butik = _context.Butiker.FirstOrDefault(b => b.ButikId == id);
            if (butik == null)
            {
                return NotFound(new { error = $"Butik med ID {id} finns inte." });
            }

            if (_context.Butiker.Any(b => b.ButikNamn == uppdateradButik.ButikNamn && b.ButikId != id))
            {
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

                _context.SaveChanges();
                return Ok(new { message = $"Butik med ID {id} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating butik: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av butik. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult TaBortButik(int id)
        {
            try
            {
                var butik = _context.Butiker.FirstOrDefault(b => b.ButikId == id);
                if (butik == null)
                {
                    return NotFound(new { error = $"Butik med ID {id} finns inte." });
                }

                if (_context.Beställningar.Any(b => b.ButikId == id))
                {
                    return BadRequest(new { error = $"Kan inte ta bort butiken '{butik.ButikNamn}' eftersom den är kopplad till befintliga beställningar." });
                }

                _context.Butiker.Remove(butik);
                _context.SaveChanges();
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
        public IActionResult LåsButik(int id, [FromBody] bool låst)
        {
            try
            {
                var butik = _context.Butiker.FirstOrDefault(b => b.ButikId == id);
                if (butik == null)
                {
                    return NotFound(new { error = $"Butik med ID {id} finns inte." });
                }

                butik.Låst = låst;
                _context.SaveChanges();
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