namespace ReserveNow_API.Models.Classes
{
    public class City
    {
        public int ID { get; set; }
        public string? City_name { get; set; }

        public ICollection<Clients>? Clients { get; set; }
        public ICollection<Administration>? Administration { get; set; }
    }
}
