namespace ReserveNow_API.Models.Classes
{
    public class Administration
    {
        public int ID { get; set; }
        public int OrganizationKey { get; set; }
        public int CityKey { get; set; }
        public string? Adress { get; set; }
        public int Table_reservationKey { get; set; }
        public int Tables { get; set; }

        public City? City { get; set; }
        public Organization? Organizations { get; set; }
        public Table_reservation? Table_reservations { get; set; }

    }
}
