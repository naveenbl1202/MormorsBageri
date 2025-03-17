using Microsoft.AspNetCore.Mvc;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdukterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdukterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Säljare,Planerare")]
        public IActionResult HämtaProdukter()
        {
            try
            {
                var produkter = _context.Produkter.ToList();
                return Ok(produkter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching produkter: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av produkter. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult LäggTillProdukt([FromBody] Produkt produkt)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Produkter.Any(p => p.Namn == produkt.Namn))
            {
                return Conflict(new { error = $"Produkten '{produkt.Namn}' finns redan." });
            }

            try
            {
                _context.Produkter.Add(produkt);
                _context.SaveChanges();
                return CreatedAtAction(nameof(HämtaProdukter), new { id = produkt.ProduktId }, produkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding produkt: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid tillägg av produkt. Kontakta support." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult UppdateraProdukt(int id, [FromBody] Produkt uppdateradProdukt)
        {
            if (id != uppdateradProdukt.ProduktId)
            {
                return BadRequest(new { error = "ID i URL och kropp matchar inte." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produkt = _context.Produkter.FirstOrDefault(p => p.ProduktId == id);
            if (produkt == null)
            {
                return NotFound(new { error = $"Produkt med ID {id} finns inte." });
            }

            if (_context.Produkter.Any(p => p.Namn == uppdateradProdukt.Namn && p.ProduktId != id))
            {
                return Conflict(new { error = $"Produkten '{uppdateradProdukt.Namn}' finns redan." });
            }

            try
            {
                produkt.Namn = uppdateradProdukt.Namn;
                produkt.Baspris = uppdateradProdukt.Baspris;
                _context.SaveChanges();
                return Ok(new { message = $"Produkt med ID {id} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating produkt: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av produkt. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult TaBortProdukt(int id)
        {
            try
            {
                var produkt = _context.Produkter.FirstOrDefault(p => p.ProduktId == id);
                if (produkt == null)
                {
                    return NotFound(new { error = $"Produkt med ID {id} finns inte." });
                }

                if (_context.Beställningsdetaljer.Any(bd => bd.ProduktId == id))
                {
                    return BadRequest(new { error = $"Kan inte ta bort produkten '{produkt.Namn}' eftersom den är kopplad till befintliga beställningar." });
                }

                _context.Produkter.Remove(produkt);
                _context.SaveChanges();
                return Ok(new { message = $"Produkt med ID {id} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting produkt: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av produkt. Kontakta support." });
            }
        }
    }
}