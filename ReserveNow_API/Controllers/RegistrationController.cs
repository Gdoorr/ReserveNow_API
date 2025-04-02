using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;
using System.Net;
using System.Security.Claims;

namespace ReserveNow_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class RegistrationController : ControllerBase
    {
        private readonly IAuthorization _auth;
        public ApplicationContext _app;

        public RegistrationController(IAuthorization auth, ApplicationContext app)
        {
            _auth = auth;
            _app = app;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] Clients client)
        {
            //if (string.IsNullOrEmpty(client.Email) || string.IsNullOrEmpty(client.Password))
            //{
            //    return BadRequest(new { Message = "Username and password are required." });
            //}

            // Поиск пользователя в базе данных
            var user = _auth.FindUserByUsername(client.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            // Проверка пароля
            if (!_auth.VerifyPassword(user, client.Password))
            {
                return Unauthorized(new { Message = "Invalid username or password." });
            }

            // Генерация JWT-токена
            var token = _auth.GenerateToken(user.ID.ToString(), user.Role);

            return Ok(new { Token = token });
        }

        [HttpPost("refresh-token")]
        public IActionResult Refresh([FromBody] RefreshTokenModel model)
        {
            try
            {
                var newToken = _auth.RefreshToken(model.OldToken);
                return Ok(new { Token = newToken });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }
    }
}
