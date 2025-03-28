using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MormorsBageri.Data;
using MormorsBageri.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using MormorsBageri.Services;
using MormorsBageri.Enums;
using System.Security.Claims;

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
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Beställningar
                    .Include(b => b.Butik)
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .AsQueryable();

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                    {
                        query = query.Where(b => b.Säljare == username);
                    }
                }

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                var orders = await query
                    .OrderByDescending(b => b.Beställningsdatum)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new
                {
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Data = orders
                };

                Console.WriteLine($"Fetched {orders.Count} orders for page {page} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningar. Kontakta support." });
            }
        }

        [HttpGet("upcoming-deliveries")]
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetUpcomingDeliveries()
        {
            try
            {
                var now = DateTime.UtcNow;
                var threeDaysFromNow = now.AddDays(3);

                var query = _context.Beställningar
                    .Include(b => b.Butik)
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .Where(b => b.PreliminärtLeveransdatum >= now && b.PreliminärtLeveransdatum <= threeDaysFromNow)
                    .AsQueryable();

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                    {
                        query = query.Where(b => b.Säljare == username);
                    }
                }

                var upcomingDeliveries = await query
                    .OrderBy(b => b.PreliminärtLeveransdatum)
                    .ToListAsync();

                Console.WriteLine($"Fetched {upcomingDeliveries.Count} upcoming deliveries by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(upcomingDeliveries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching upcoming deliveries: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av kommande leveranser. Kontakta support." });
            }
        }

        [HttpGet("dashboard-statistics")]
        [Authorize(Roles = "Admin,Planerare")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            try
            {
                // Total Orders
                var totalOrders = await _context.Beställningar.CountAsync();

                // Total Revenue
                var totalRevenue = await _context.Beställningsdetaljer
                    .SumAsync(bd => bd.Antal * (bd.Styckpris - bd.Rabatt));

                // Most Ordered Products
                var mostOrderedProducts = await _context.Beställningsdetaljer
                    .GroupBy(bd => bd.ProduktId)
                    .Select(g => new
                    {
                        ProduktId = g.Key,
                        ProduktNamn = g.First().Produkt.Namn,
                        TotalAntal = g.Sum(bd => bd.Antal)
                    })
                    .OrderByDescending(x => x.TotalAntal)
                    .Take(5)
                    .ToListAsync();

                // Orders by Store
                var ordersByStore = await _context.Beställningar
                    .GroupBy(b => b.ButikId)
                    .Select(g => new
                    {
                        ButikId = g.Key,
                        ButikNamn = g.First().Butik.ButikNamn,
                        TotalOrders = g.Count()
                    })
                    .OrderByDescending(x => x.TotalOrders)
                    .ToListAsync();

                var statistics = new
                {
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    MostOrderedProducts = mostOrderedProducts,
                    OrdersByStore = ordersByStore
                };

                Console.WriteLine($"Fetched dashboard statistics: {JsonSerializer.Serialize(statistics, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching dashboard statistics: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av statistik. Kontakta support." });
            }
        }

        [HttpGet("{butikId}")]
        [Authorize(Roles = "Admin,Planerare,Säljare")]
        public async Task<IActionResult> GetOrdersByStore(int butikId)
        {
            try
            {
                var query = _context.Beställningar
                    .Include(b => b.Butik)
                    .Include(b => b.Beställningsdetaljer)
                    .ThenInclude(bd => bd.Produkt)
                    .Where(b => b.ButikId == butikId)
                    .AsQueryable();

                if (User.IsInRole("Säljare"))
                {
                    var username = User.FindFirst(ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrEmpty(username))
                    {
                        query = query.Where(b => b.Säljare == username);
                    }
                }

                var orders = await query
                    .OrderByDescending(b => b.Beställningsdatum)
                    .ToListAsync();

                Console.WriteLine($"Fetched {orders.Count} orders for store {butikId} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching orders for store {butikId}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid hämtning av beställningar. Kontakta support." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Säljare")]
        public async Task<IActionResult> CreateOrder([FromBody] Beställning beställning)
        {
            if (beställning == null || beställning.Beställningsdetaljer == null || !beställning.Beställningsdetaljer.Any())
            {
                Console.WriteLine("Create order failed: Beställningsdata saknas eller är ogiltig");
                return BadRequest(new { error = "Beställningsdata saknas eller är ogiltig." });
            }

            try
            {
                beställning.Beställningsdatum = DateTime.UtcNow;
                _context.Beställningar.Add(beställning);
                await _context.SaveChangesAsync();

                var planerare = await _context.Användare
                    .Where(a => a.Roll == Roller.Planerare)
                    .ToListAsync();

                var admin = await _context.Användare
                    .Where(a => a.Roll == Roller.Admin)
                    .ToListAsync();

                foreach (var p in planerare)
                {
                    if (!string.IsNullOrEmpty(p.Email))
                    {
                        _emailService.SendEmail(
                            p.Email,
                            $"Ny beställning #{beställning.BeställningId} skapad",
                            $"En ny beställning har skapats av {beställning.Säljare}. Vänligen granska och bekräfta.\nOrder ID: {beställning.BeställningId}"
                        );
                    }
                }

                foreach (var a in admin)
                {
                    if (!string.IsNullOrEmpty(a.Email))
                    {
                        _emailService.SendEmail(
                            a.Email,
                            $"Ny beställning #{beställning.BeställningId} skapad",
                            $"En ny beställning har skapats av {beställning.Säljare}. Vänligen granska och bekräfta.\nOrder ID: {beställning.BeställningId}"
                        );
                    }
                }

                Console.WriteLine($"Created order: {JsonSerializer.Serialize(beställning, new JsonSerializerOptions { WriteIndented = true })} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(beställning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating order: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid skapande av beställning. Kontakta support." });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "PlanerareOnly")] // Restrict to Planerare only using policy
        public async Task<IActionResult> UpdateOrderDeliveryDate(int id, [FromBody] BeställningsUpdateDTO updateDTO)
        {
            try
            {
                var beställning = await _context.Beställningar.FindAsync(id);
                if (beställning == null)
                {
                    Console.WriteLine($"Update order delivery date failed: Beställning med ID {id} finns inte");
                    return NotFound(new { error = $"Beställning med ID {id} finns inte." });
                }

                beställning.PreliminärtLeveransdatum = updateDTO.PreliminärtLeveransdatum;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated delivery date for order {id} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Leveransdatum för beställning {id} har uppdaterats." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating delivery date for order {id}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid uppdatering av leveransdatum. Kontakta support." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrPlanerare")] // Restrict to Admin and Planerare using policy
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var beställning = await _context.Beställningar.FindAsync(id);
                if (beställning == null)
                {
                    Console.WriteLine($"Delete order failed: Beställning med ID {id} finns inte");
                    return NotFound(new { error = $"Beställning med ID {id} finns inte." });
                }

                _context.Beställningar.Remove(beställning);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted order with ID {id} by user '{User.Identity.Name ?? "Unknown"}' (Role: {User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown"})");
                return Ok(new { message = $"Beställning med ID {id} har tagits bort." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting order {id}: {ex.Message}");
                return StatusCode(500, new { error = "Databasfel vid borttagning av beställning. Kontakta support." });
            }
        }
    }

    public class BeställningsUpdateDTO
    {
        public DateTime PreliminärtLeveransdatum { get; set; }
    }
}