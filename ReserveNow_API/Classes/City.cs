namespace ReserveNow_API.Classes
{
    public class City
    {
        public int ID { get; set; }
        public string? City_name { get; set; }

        public Clients? Clients { get; set; }
        public Administration? Administration { get; set; }
    }
}
