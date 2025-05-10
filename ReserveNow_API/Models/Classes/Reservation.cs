namespace ReserveNow_API.Models.Classes
{
    public class Reservation
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public int TableId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Guests { get; set; }
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Client Client { get; set; }
        public Restaurant Restaurant { get; set; }
        public Table Table { get; set; }
    }
}
