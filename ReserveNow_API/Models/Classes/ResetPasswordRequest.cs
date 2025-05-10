namespace ReserveNow_API.Models.Classes
{
    public class ResetPasswordRequest
    {
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? NewPassword { get; set; }
    }
}
