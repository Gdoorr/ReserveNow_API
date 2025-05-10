using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;

namespace ReserveNow_API.Servises
{
    public class RestaurantService: IRestaurantService
    {
        private readonly ApplicationContext _context;
        public RestaurantService(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<List<Restaurant>> GetRestaurantsAsync()
        {
            var restaurants = await _context.Restaurants
            .Include(r => r.City) // Явно загружаем связанный объект City
            .ToListAsync();

            return restaurants;
        }
    }
}
