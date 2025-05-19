namespace ReserveNow_API.Models.Classes
{
    public class RestaurantModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Capacity { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public string City { get; set; }
        public List<TableDto> Tables { get; set; }
    }
}
