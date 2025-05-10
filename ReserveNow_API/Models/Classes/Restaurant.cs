using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ReserveNow_API.Models.Classes
{
    public class Restaurant
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Capacity { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public int CityId { get; set; }
        public City City { get; set; }

        public ICollection<Table> Tables { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Menu> Menus { get; set; }
    }
}
