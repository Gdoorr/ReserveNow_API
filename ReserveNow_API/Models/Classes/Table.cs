using System.Text.Json.Serialization;
namespace ReserveNow_API.Models.Classes
{
    public class Table
    {
        public int ID { get; set; }
        public int RestaurantId { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; } = true;
        [JsonIgnore]
        public Restaurant Restaurant { get; set; }
        public ICollection<Reservation> Reservations { get; set; }
    }
}
