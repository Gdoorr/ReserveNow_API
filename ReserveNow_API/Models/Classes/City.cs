namespace ReserveNow_API.Models.Classes
{
    public class City
    {
        public int ID { get; set; }
        public string? Name { get; set; }

        // Связь один-ко-многим: один город - много ресторанов
        public ICollection<Restaurant> Restaurants { get; set; }
        public ICollection<Client> Clients { get; set; }
    }
}
