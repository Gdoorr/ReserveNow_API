using ReserveNow_API.Models.Classes;
using System.Security.Claims;

namespace ReserveNow_API.Models.Interfaces
{
    public interface IAuthorization
    {
        string GenerateToken(string userId, string role);
        string RefreshToken(string oldToken);
        Clients FindUserByUsername(string email);
        bool VerifyPassword(Clients user, string password);
    }
}
