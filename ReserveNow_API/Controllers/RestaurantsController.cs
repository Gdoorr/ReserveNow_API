using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;
using System.Globalization;
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
            var restaurants = await _context.Restaurants
        .Include(r => r.City) // Явно загружаем связанный объект City
        .ToListAsync();

            // Проекция данных в DTO
            var restaurantDtos = restaurants.Select(r => new RestaurantModel
            {
                ID = r.ID,
                Name = r.Name,
                Address = r.Address,
                Phone = r.Phone,
                Description = r.Description,
                ImageUrl = r.ImageUrl,
                Capacity = r.Capacity,
                OpeningTime = r.OpeningTime,
                ClosingTime = r.ClosingTime,
                City = r.City?.Name // Добавляем название города
            }).ToList();

            return Ok(restaurantDtos);
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
            var restaurantDto = new RestaurantModel
            {
                ID = restaurant.ID,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                Description = restaurant.Description,
                OpeningTime = restaurant.OpeningTime,
                ClosingTime = restaurant.ClosingTime,
                Tables = restaurant.Tables.Select(t => new TableDto
                {
                    ID = t.ID,
                    Capacity = t.Capacity,
                    IsAvailable = t.IsAvailable
                }).ToList()
            };
            return Ok(restaurant);
        }
        [HttpGet("{id}/available-tables")]
        public async Task<IActionResult> GetAvailableTables(
        int id,
        [FromQuery] DateTime date,
        [FromQuery] TimeSpan startTime,
        [FromQuery] TimeSpan endTime,
        [FromQuery] int guests)
        {
            var utcDate = date.ToUniversalTime();
            // Получаем все брони для ресторана на указанную дату и время
            var reservations = await _context.Reservations
                .Where(r => r.RestaurantId == id && r.Date == utcDate &&
                            ((r.StartTime <= startTime && r.EndTime > startTime) ||
                             (r.StartTime < endTime && r.EndTime >= endTime)))
                .ToListAsync();

            // Получаем все столы ресторана
            var tables = await _context.Tables
                .Where(t => t.RestaurantId == id && t.Capacity >= guests)
                .ToListAsync();

            // Исключаем занятые столы
            var availableTables = tables
        .Where(t => !reservations.Any(r => r.TableId == t.ID)) // Исключаем забронированные столики
        .OrderBy(t => t.Capacity) // Сортируем по вместимости (от меньшего к большему)
        .ToList();

            return Ok(availableTables);
        }
        private bool IsTimeOverlapping(TimeSpan startTime1, TimeSpan endTime1, TimeSpan startTime2, TimeSpan endTime2)
        {
            return (startTime1 < endTime2 && startTime2 < endTime1);
        }
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveTable([FromBody] ReservationRequest request)
        {
            if (request == null || request.TableId <= 0 || request.Guests <= 0)
            {
                return BadRequest("Invalid reservation request.");
            }

            // Проверяем, существует ли столик и достаточно ли его вместимости
            var table = await _context.Tables.FindAsync(request.TableId);
            if (table == null || table.Capacity < request.Guests)
            {
                return BadRequest("No suitable table available.");
            }

            // Проверяем формат даты и преобразуем в UTC
            if (!DateTime.TryParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsedDate))
            {
                return BadRequest("Invalid date format. Expected 'yyyy-MM-dd'.");
            }

            // Проверяем формат времени
            if (!TimeSpan.TryParseExact(request.StartTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var startTime) ||
                !TimeSpan.TryParseExact(request.EndTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var endTime))
            {
                return BadRequest("Invalid time format. Expected 'hh:mm:ss'.");
            }

            // Проверяем, существует ли уже бронь для этого пользователя, ресторана и столика
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.UserId == request.UserId &&
                    r.RestaurantId == request.RestaurantId &&
                    r.TableId == request.TableId &&
                    r.Date == parsedDate &&
                    (
                        (r.StartTime < endTime && r.EndTime > startTime) || // Пересечение по времени
                        (r.StartTime < endTime && r.EndTime >= endTime)     // Пересечение по времени
                    )
                );

            if (existingReservation != null)
            {
                return Conflict("You already have a reservation for this table at the specified time.");
            }

            // Создаем объект бронирования
            var reservation = new Reservation
            {
                UserId = request.UserId,
                RestaurantId = request.RestaurantId,
                TableId = request.TableId,
                Date = parsedDate, // Используем преобразованную дату в UTC
                StartTime = startTime,
                EndTime = endTime,
                Guests = request.Guests
            };

            // Сохраняем бронирование в базу данных
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Возвращаем JSON-ответ
            return Ok(new ApiResponse { Message = "Reservation created successfully" });
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            try
            {
                // Текущая дата и время в UTC
                var nowUtc = DateTime.UtcNow;

                // Получаем все брони пользователя
                var reservations = await _context.Reservations
            .Where(r => r.UserId == userId)
            .Join(
                _context.Restaurants,
                reservation => reservation.RestaurantId,
                restaurant => restaurant.ID,
                (reservation, restaurant) => new ReservationRequest
                {
                    ID = reservation.ID,
                    RestaurantId = reservation.RestaurantId,
                    RestaurantName = restaurant.Name,
                    RestaurantCity = restaurant.City.Name,
                    TableId = reservation.TableId,
                    Date = reservation.Date.ToString("yyyy-MM-dd"), // Преобразуем в строку
                    StartTime = reservation.StartTime.ToString(@"hh\:mm\:ss"), // Преобразуем в строку
                    EndTime = reservation.EndTime.ToString(@"hh\:mm\:ss"),
                    Guests = reservation.Guests
                })
            .ToListAsync();

                // Фильтруем истекшие брони
                var now = DateTime.UtcNow;
                var activeReservations = new List<ReservationRequest>();
                foreach (var r in reservations)
                {
                    if (DateTime.TryParse(r.Date, out var parsedDate) &&
                        TimeSpan.TryParse(r.EndTime, out var parsedEndTime))
                    {
                        if (parsedDate > now.Date ||
                            (parsedDate == now.Date && parsedEndTime > now.TimeOfDay))
                        {
                            activeReservations.Add(r);
                        }
                    }
                }

                // Удаляем истекшие брони из базы данных
                var expiredReservations = reservations.Except(activeReservations).ToList();
                if (expiredReservations.Any())
                {
                    var expiredReservationIds = expiredReservations.Select(r => r.ID);
                    var reservationsToDelete = await _context.Reservations
                        .Where(r => expiredReservationIds.Contains(r.ID))
                        .ToListAsync();
                    _context.Reservations.RemoveRange(reservationsToDelete);
                    await _context.SaveChangesAsync();
                }

                return Ok(activeReservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
