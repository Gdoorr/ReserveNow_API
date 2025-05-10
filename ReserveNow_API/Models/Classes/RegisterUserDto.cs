namespace ReserveNow_API.Models.Classes
{
    public class RegisterUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public int CityId { get; set; }
    }
}
