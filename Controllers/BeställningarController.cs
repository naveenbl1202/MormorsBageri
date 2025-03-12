using Microsoft.AspNetCore.Mvc;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MormorsBageri.Enums;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BeställningarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BeställningarController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Säljare,Planerare")] // Allow Admin too
        public IActionResult HämtaBeställningar()
        {
            var beställningar = _context.Beställningar
                .Include(b => b.Beställningsdetaljer)
                .ToList();
            return Ok(beställningar);
        }

        [HttpGet("{butikId}")]
        [Authorize(Roles = "Admin,Säljare,Planerare")] // Allow Admin too
        public IActionResult HämtaBeställningarFörButik(int butikId)
        {
            var beställningar = _context.Beställningar
                .Where(b => b.ButikId == butikId)
                .Include(b => b.Beställningsdetaljer)
                .ToList();
            return Ok(beställningar);
        }

        [HttpPost]
        [Authorize(Roles = "Säljare")]
        public IActionResult SkapaBeställning([FromBody] Beställning beställning)
        {
            beställning.Beställningsdatum = DateTime.Now;
            beställning.PreliminärtLeveransdatum = DateTime.Now.AddDays(2);
            _context.Beställningar.Add(beställning);
            _context.SaveChanges();
            return CreatedAtAction(nameof(HämtaBeställningar), new { id = beställning.BeställningId }, beställning);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Planerare")]
        public IActionResult UppdateraBeställning(int id, [FromBody] Beställning uppdateradBeställning)
        {
            var beställning = _context.Beställningar.FirstOrDefault(b => b.BeställningId == id);
            if (beställning == null) return NotFound();

            beställning.Beställare = uppdateradBeställning.Beställare;
            beställning.PreliminärtLeveransdatum = uppdateradBeställning.PreliminärtLeveransdatum;
            beställning.Beställningsdetaljer = uppdateradBeställning.Beställningsdetaljer;
            _context.SaveChanges();
            return Ok(beställning);
        }
    }
}