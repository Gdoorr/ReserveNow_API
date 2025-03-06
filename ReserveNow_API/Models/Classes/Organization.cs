namespace ReserveNow_API.Models.Classes
{
    public class Organization
    {
        public int ID { get; set; }
        public string? Org_name { get; set; }

        public ICollection<Administration>? Administrations { get; set; }
    }
}
