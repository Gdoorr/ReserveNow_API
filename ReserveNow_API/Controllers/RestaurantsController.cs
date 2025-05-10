using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;
using System.Linq;

namespace ReserveNow_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantsController : Controller
    {
        public ApplicationContext _context;
        public IRestaurantService _rest;

        public RestaurantsController(ApplicationContext context,IRestaurantService rest)
        {
            _context = context;
            _rest = rest;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetRestaurants()
        {
            var restaurants = await _context.Restaurants.ToListAsync();
            return Ok(restaurants);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.Tables)
                .FirstOrDefaultAsync(r => r.ID == id);

            if (restaurant == null)
            {
                return NotFound("Restaurant not found");
            }

            return Ok(restaurant);
        }
        [HttpGet("{id}/tables")]
        public async Task<IActionResult> GetAvailableTables(int id, [FromQuery] DateTime date, [FromQuery] TimeSpan time)
        {
            var reservations = await _context.Reservations
                .Where(r => r.RestaurantId == id && r.Date == date && r.Time == time)
                .ToListAsync();

            var tables = await _context.Tables
                .Where(t => t.RestaurantId == id)
                .ToListAsync();

            var availableTables = tables
                .Select(t => new
                {
                    t.ID,
                    t.Capacity,
                    IsAvailable = !reservations.Any(r => r.TableId == t.ID)
                })
                .ToList();

            return Ok(availableTables);
        }
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveTable([FromBody] Reservation request)
        {
            var table = await _context.Tables.FindAsync(request.TableId);
            if (table == null || !table.IsAvailable)
            {
                return BadRequest("Table is not available");
            }

            var reservation = new Reservation
            {
                UserId = request.UserId,
                RestaurantId = request.RestaurantId,
                TableId = request.TableId,
                Date = request.Date,
                Time = request.Time,
                Guests = request.Guests
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok("Reservation created successfully");
        }
    }
}
