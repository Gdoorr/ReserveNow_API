namespace ReserveNow_API.Classes
{
    public class Organization
    {
        public int ID { get; set; }
        public string? Org_name{ get; set; }

        public Administration? Administrations { get; set; }
    }
}
