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
        [Authorize(Roles = "Admin,Säljare,Planerare")] // Allow all roles to view
        public IActionResult HämtaButiker()
        {
            var butiker = _context.Butiker.ToList();
            return Ok(butiker);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult LäggTillButik([FromBody] Butik butik)
        {
            _context.Butiker.Add(butik);
            _context.SaveChanges();
            return CreatedAtAction(nameof(HämtaButiker), new { id = butik.ButikId }, butik);
        }
    }
}