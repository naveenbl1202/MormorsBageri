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
        [Authorize(Roles = "Admin,Säljare,Planerare")] // Allow all roles to view
        public IActionResult HämtaProdukter()
        {
            var produkter = _context.Produkter.ToList();
            return Ok(produkter);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult LäggTillProdukt([FromBody] Produkt produkt)
        {
            _context.Produkter.Add(produkt);
            _context.SaveChanges();
            return CreatedAtAction(nameof(HämtaProdukter), new { id = produkt.ProduktId }, produkt);
        }
    }
}