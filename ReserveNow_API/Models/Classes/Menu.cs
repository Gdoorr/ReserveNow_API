namespace ReserveNow_API.Models.Classes
{
    public class Menu
    {
        public int ID { get; set; }
        public int RestaurantId { get; set; }
        public string? Category { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public Restaurant Restaurant { get; set; }
    }
}
