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
    public class ProdukterController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdukterController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Säljare,Planerare")]
        public async Task<IActionResult> HämtaProdukter()
        {
            try
            {
                var produkter = await _context.Produkter.ToListAsync(); // Query filter in AppDbContext excludes IsDeleted = true
                Console.WriteLine($"Fetched {produkter.Count} produkter by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                Console.WriteLine("Produkter data: " + JsonSerializer.Serialize(produkter, new JsonSerializerOptions { WriteIndented = true }));
                return Ok(produkter);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching produkter: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av produkter. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> LäggTillProdukt([FromBody] Produkt produkt)
        {
            if (produkt == null)
            {
                Console.WriteLine("Add product failed: Product data is null");
                return BadRequest(new { error = "Produktdata saknas." });
            }

            if (string.IsNullOrWhiteSpace(produkt.Namn))
            {
                Console.WriteLine("Add product failed: Product name is empty");
                return BadRequest(new { error = "Produktnamn får inte vara tomt." });
            }

            if (produkt.Baspris <= 0)
            {
                Console.WriteLine("Add product failed: Base price must be positive");
                return BadRequest(new { error = "Baspris måste vara större än 0." });
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Add product failed: Invalid model state");
                return BadRequest(ModelState);
            }

            if (await _context.Produkter.AnyAsync(p => p.Namn == produkt.Namn && !p.IsDeleted))
            {
                Console.WriteLine($"Add product failed: Product '{produkt.Namn}' already exists");
                return Conflict(new { error = $"Produkten '{produkt.Namn}' finns redan." });
            }

            try
            {
                produkt.IsDeleted = false; // Ensure IsDeleted is false for new products
                _context.Produkter.Add(produkt);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Added produkt: {JsonSerializer.Serialize(produkt, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
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
        public async Task<IActionResult> UppdateraProdukt(int id, [FromBody] Produkt uppdateradProdukt)
        {
            if (id != uppdateradProdukt.ProduktId)
            {
                Console.WriteLine($"Update product failed: ID mismatch (URL: {id}, Body: {uppdateradProdukt.ProduktId})");
                return BadRequest(new { error = "ID i URL och kropp matchar inte." });
            }

            if (string.IsNullOrWhiteSpace(uppdateradProdukt.Namn))
            {
                Console.WriteLine("Update product failed: Product name is empty");
                return BadRequest(new { error = "Produktnamn får inte vara tomt." });
            }

            if (uppdateradProdukt.Baspris <= 0)
            {
                Console.WriteLine("Update product failed: Base price must be positive");
                return BadRequest(new { error = "Baspris måste vara större än 0." });
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Update product failed: Invalid model state");
                return BadRequest(ModelState);
            }

            var produkt = await _context.Produkter.FirstOrDefaultAsync(p => p.ProduktId == id);
            if (produkt == null)
            {
                Console.WriteLine($"Update product failed: Product with ID {id} not found");
                return NotFound(new { error = $"Produkt med ID {id} finns inte." });
            }

            if (await _context.Produkter.AnyAsync(p => p.Namn == uppdateradProdukt.Namn && p.ProduktId != id && !p.IsDeleted))
            {
                Console.WriteLine($"Update product failed: Product '{uppdateradProdukt.Namn}' already exists");
                return Conflict(new { error = $"Produkten '{uppdateradProdukt.Namn}' finns redan." });
            }

            try
            {
                produkt.Namn = uppdateradProdukt.Namn;
                produkt.Baspris = uppdateradProdukt.Baspris;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated produkt: {JsonSerializer.Serialize(produkt, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Produkt med ID {id} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating produkt: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av produkt. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> TaBortProdukt(int id)
        {
            try
            {
                var produkt = await _context.Produkter.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.ProduktId == id);
                if (produkt == null)
                {
                    Console.WriteLine($"Delete product failed: Product with ID {id} not found");
                    return NotFound(new { error = $"Produkt med ID {id} finns inte." });
                }

                // Mark the product as deleted instead of removing it
                produkt.IsDeleted = true;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Soft-deleted produkt with ID {id} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Produkt med ID {id} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error soft-deleting produkt: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av produkt. Kontakta support." });
            }
        }
    }
}