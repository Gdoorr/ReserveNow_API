namespace ReserveNow_API.Models.Classes
{
    public class ReservationRequest
    {
        public int ID { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } // Название ресторана
        public string RestaurantCity { get; set; }
        public int TableId { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Guests { get; set; }
    }
}
