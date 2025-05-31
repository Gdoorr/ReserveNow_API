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
        private readonly string _baseUrl;
        public RestaurantsController(ApplicationContext context,IRestaurantService rest, IConfiguration configuration)
        {
            _context = context;
            _rest = rest;
            _baseUrl = configuration["ServerSettings:BaseUrl"];
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
                ImageUrl = string.IsNullOrEmpty(r.ImageUrl) ? null : $"{_baseUrl}{r.ImageUrl}",
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
        [HttpPut("{id}")]
        
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationRequest updatedReservation)
        {
            var existingReservation = await _context.Reservations.FindAsync(id);
            if (existingReservation == null)
            {
                return NotFound("Reservation not found.");
            }

            // Обновляем данные брони
            if (!DateTime.TryParseExact(updatedReservation.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return BadRequest("Invalid date format. Expected 'yyyy-MM-dd'.");
            }

            if (!TimeSpan.TryParseExact(updatedReservation.StartTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var parsedStartTime))
            {
                return BadRequest("Invalid start time format. Expected 'hh:mm:ss'.");
            }

            if (!TimeSpan.TryParseExact(updatedReservation.EndTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var parsedEndTime))
            {
                return BadRequest("Invalid end time format. Expected 'hh:mm:ss'.");
            }

            // Обновляем данные брони
            existingReservation.TableId = updatedReservation.TableId;
            existingReservation.Date = parsedDate;
            existingReservation.StartTime = parsedStartTime;
            existingReservation.EndTime = parsedEndTime;
            existingReservation.Guests = updatedReservation.Guests;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse { Message = "Reservation updated successfully." });
        }
        //[HttpGet("Reservetion/{id}")]
        //public async Task<IActionResult> GetRestaurantReservetion(int UserId,string RestaurantName,string RestaurantCity)
        //{
        //    var restaurant = await _context.Restaurants
        //        .Include(r => r.Tables)
        //        .FirstOrDefaultAsync(r => r.ID == id);

        //    if (restaurant == null)
        //    {
        //        return NotFound("Restaurant not found");
        //    }
        //    var restaurantDto = new RestaurantModel
        //    {
        //        ID = restaurant.ID,
        //        Name = restaurant.Name,
        //        Address = restaurant.Address,
        //        Phone = restaurant.Phone,
        //        Description = restaurant.Description,
        //        OpeningTime = restaurant.OpeningTime,
        //        ClosingTime = restaurant.ClosingTime,
        //        Tables = restaurant.Tables.Select(t => new TableDto
        //        {
        //            ID = t.ID,
        //            Capacity = t.Capacity,
        //            IsAvailable = t.IsAvailable
        //        }).ToList()
        //    };
        //    return Ok(restaurant);
        //}
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
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckTableAvailability(
    int restaurantId,
    int tableId,
    [FromQuery] DateTime date,
    [FromQuery] TimeSpan startTime,
    [FromQuery] TimeSpan endTime,
    [FromQuery] int guests,
    [FromQuery] int? currentReservationId = null) // ID текущей брони
        {
            var existingReservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.RestaurantId == restaurantId &&
                    r.TableId == tableId &&
                    r.Date == date &&
                    (
                        (r.StartTime < endTime && r.EndTime > startTime) ||
                        (r.StartTime < endTime && r.EndTime >= endTime)
                    ) &&
                    r.ID != currentReservationId // Исключаем текущую бронь
                );

            if (existingReservation != null)
            {
                return Ok(false); // Стол занят
            }

            return Ok(true); // Стол свободен
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
            var canCreate = await CanCreateReservation(request.UserId, request.RestaurantId, parsedDate);
            if (!canCreate)
            {
                return Conflict("You have reached the maximum number of reservations for this restaurant on this date.");
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
        [HttpGet("history/{userId}")]
        
        public async Task<IActionResult> GetReservationsHistory(int userId)
        {
            try
            {
                // Текущая дата и время в UTC
                var nowUtc = DateTime.UtcNow;

                // Получаем все брони пользователя с присоединением данных ресторана
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
                            Date = reservation.Date.ToString("yyyy-MM-dd"),
                            StartTime = reservation.StartTime.ToString(@"hh\:mm\:ss"),
                            EndTime = reservation.EndTime.ToString(@"hh\:mm\:ss"),
                            Guests = reservation.Guests
                        })
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("user/{userId}")]
        
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            try
            {
                // Текущая дата и время в UTC
                var nowUtc = DateTime.UtcNow;

                // Фильтруем брони в базе данных
                var activeReservations = await _context.Reservations
                    .Where(r => r.UserId == userId &&
                                r.Date >= nowUtc.Date && // Бронь не старше текущей даты
                                (r.Date > nowUtc.Date || r.EndTime > nowUtc.TimeOfDay)) // Бронь еще не истекла
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
                            Date = reservation.Date.ToString("yyyy-MM-dd"),
                            StartTime = reservation.StartTime.ToString(@"hh\:mm\:ss"),
                            EndTime = reservation.EndTime.ToString(@"hh\:mm\:ss"),
                            Guests = reservation.Guests
                        })
                    .ToListAsync();

                // Удаляем брони старше полугода
                var sixMonthsAgo = nowUtc.AddMonths(-6);
                var expiredReservations = await _context.Reservations
                    .Where(r => r.UserId == userId &&
                                r.Date < sixMonthsAgo) // Брони старше полугода
                    .ToListAsync();

                if (expiredReservations.Any())
                {
                    _context.Reservations.RemoveRange(expiredReservations);
                    await _context.SaveChangesAsync();
                }

                return Ok(activeReservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
        private string HashPassword(string password)
        {
            // Простой пример хеширования пароля (в реальном проекте используйте более надежные методы)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        [HttpPut("users/{id}")]
        
        public async Task<IActionResult> UpdateClient(int id, [FromBody] Client updatedClient)
        {
            try
            {
                // Ищем пользователя в базе данных
                var client = await _context.Client.FindAsync(id);
                if (client == null)
                {
                    return NotFound("User not found.");
                }

                // Обновляем данные пользователя
                client.Name = updatedClient.Name;
                client.Email = updatedClient.Email;
                client.Phone = updatedClient.Phone;
                client.Password = HashPassword(updatedClient.Password); // Хэшируйте пароль перед сохранением
                client.CityId = updatedClient.CityId;

                // Сохраняем изменения
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpPut("update")]
        
        public async Task<IActionResult> UpdateReservation([FromBody] Reservation updatedReservation)
        {
            var existingReservation = await _context.Reservations.FindAsync(updatedReservation.ID);
            if (existingReservation == null)
            {
                return NotFound("Reservation not found.");
            }

            existingReservation.Guests = updatedReservation.Guests;
            existingReservation.Date = updatedReservation.Date;
            existingReservation.StartTime = updatedReservation.StartTime;
            existingReservation.EndTime = updatedReservation.EndTime;

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse { Message = "Reservation updated successfully." });
        }

        [HttpDelete("delete/{id}")]
        
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound("Reservation not found.");
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return Ok(new ApiResponse { Message = "Reservation deleted successfully." });
        }
        [HttpGet("restaurants/{restaurantId}/working-hours")]
        public async Task<IActionResult> GetRestaurantWorkingHours(int restaurantId)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
            {
                return NotFound("Restaurant not found.");
            }

            var workingHours = new
            {
                OpenTime = restaurant.OpeningTime.ToString(@"hh\:mm"),
                CloseTime = restaurant.ClosingTime.ToString(@"hh\:mm")
            };

            return Ok(workingHours);
        }
        private async Task<bool> CanCreateReservation(int userId, int restaurantId, DateTime date)
        {
            // Проверяем количество активных броней пользователя в ресторане за день
            var activeReservations = await _context.Reservations
                .Where(r => r.UserId == userId &&
                            r.RestaurantId == restaurantId &&
                            r.Date == date &&
                            r.Status != "Cancelled") // Исключаем отмененные брони
                .ToListAsync();

            if (activeReservations.Count >= 3) // Лимит: 3 брони на день
            {
                return false;
            }

            return true;
        }
    }
}
