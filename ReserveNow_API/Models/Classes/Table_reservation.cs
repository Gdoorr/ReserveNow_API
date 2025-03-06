namespace ReserveNow_API.Models.Classes
{
    public class Table_reservation
    {
        public int ID { get; set; }
        public string? Reservation { get; set; }

        public ICollection<Administration>? Administrations { get; set; }
    }
}
