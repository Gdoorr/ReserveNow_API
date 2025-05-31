namespace ReserveNow_API.Models.Classes
{
    public class AvailabilityRequest
    {
        public int TableId { get; set; } // ID столика
        public string? Date { get; set; } // Дата в формате "yyyy-MM-dd"
        public string? StartTime { get; set; } // Время начала в формате "hh:mm:ss"
        public string? EndTime { get; set; }
    }
}
