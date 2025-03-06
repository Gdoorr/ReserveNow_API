using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace ReserveNow_API
{
        public class AuthOptions
        {
            public const string ISSUER = "MyAuthServer"; // издатель токена
            public const string AUDIENCE = "MyAuthClient"; // потребитель токена
            const string KEY = "Reserve";   // ключ для шифрации
            public const int LIFETIME = 5; // время жизни токена - 1 минута
            public static SymmetricSecurityKey GetSymmetricSecurityKey()
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
            }
        }
}
