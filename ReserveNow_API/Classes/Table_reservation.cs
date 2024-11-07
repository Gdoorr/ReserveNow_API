namespace ReserveNow_API.Classes
{
    public class Table_reservation
    {
       public int ID {  get; set; }
       public string? Reservation { get; set; }
        
       public Administration? Administrations { get; set; }
    }
}
