namespace ReserveNow_API.Models.Classes
{
    public class Clients
    {
        public int ID { get; set; }
        public string? Client_name { get; set; }
        public string? Phone_number { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int CityKey { get; set; }

        public City? City { get; set; }
    }
}
