using Microsoft.AspNetCore.Mvc;
using ReserveNow_API.Models.Classes;

namespace ReserveNow_API.Models.Interfaces
{
    public interface IRestaurantService
    {
        Task<List<Restaurant>> GetRestaurantsAsync();
    }
}
