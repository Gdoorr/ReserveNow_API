using ReserveNow_API.Models.Classes;
using System.Security.Claims;

namespace ReserveNow_API.Models.Interfaces
{
    public interface IAuthorization
    {
        public string GenerateToken(string username);
    }
}
