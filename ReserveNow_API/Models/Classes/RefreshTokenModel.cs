namespace ReserveNow_API.Models.Classes
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
