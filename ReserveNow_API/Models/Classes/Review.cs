namespace ReserveNow_API.Models.Classes
{
    public class Review
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Client Client { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
