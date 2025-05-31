using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;
using System.Net;
using System.Security.Claims;

using ResetPasswordRequest = ReserveNow_API.Models.Classes.ResetPasswordRequest;
using ForgotPasswordRequest = ReserveNow_API.Models.Classes.ForgotPasswordRequest;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ReserveNow_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class RegistrationController : ControllerBase
    {
        private readonly IAuthorization _auth;
        public ApplicationContext _app;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public RegistrationController(IAuthorization auth, ApplicationContext app, EmailService emailService, IConfiguration configuration)
        {
            _auth = auth;
            _app = app;
            _emailService = emailService;
            _configuration = configuration;
        }
        [HttpPost("data")]
        public IActionResult ClientData([FromBody] Client client)
        {
            var data = _app.Client
        .Include(c => c.City) // Явно загружаем связанный объект City
        .FirstOrDefault(c => c.Email == client.Email);

            if (client == null)
            {
                return NotFound("Client not found");
            }

            // Проекция данных в DTO
            var clientDto = new ClientDto
            {
                ID = data.ID,
                Name = data.Name,
                Email = data.Email,
                Phone = data.Phone,
                Password = data.Password,
                CreatedAt = data.CreatedAt,
                Role = data.Role,
                CityId = data.CityId,
                City = data.City?.Name // Теперь City не будет null, если он существует
            };

            return Ok(clientDto);
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Проверка уникальности email
            if (await _app.Client.AnyAsync(u => u.Email == userDto.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Создание нового пользователя
            var user = new Client
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Phone = userDto.Phone,
                Password = HashPassword(userDto.Password), // Хешируем пароль
                CityId = userDto.CityId,
                Role = "User"
            };

            _app.Client.Add(user);
            await _app.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        private string HashPassword(string password)
        {
            // Простой пример хеширования пароля (в реальном проекте используйте более надежные методы)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] Client client)
        {
            if (string.IsNullOrEmpty(client.Email) || string.IsNullOrEmpty(client.Password))
            {
                return BadRequest(new { Message = "Username and password are required." });
            }

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
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            // Генерация access_token
            var accessTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Email, client.Email),
                new Claim(ClaimTypes.Role, "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(30), // Время жизни access_token
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var accessToken = tokenHandler.CreateToken(accessTokenDescriptor);
            var accessTokenString = tokenHandler.WriteToken(accessToken);

            // Генерация refresh_token
            var refreshToken = Guid.NewGuid().ToString();

            // Сохранение refresh_token в базе данных или кэше (например, Redis)
            SaveRefreshToken(client.Email, refreshToken);

            return Ok(new
            {
                AccessToken = accessTokenString,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow
            });
        }
        [HttpGet("cities")]
        [AllowAnonymous]
        public IActionResult Cities()
        {
            var cities = _app.Cities.Select(c => new City
            {
                ID = c.ID,
                Name = c.Name
            }).ToList();

            return Ok(cities);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Проверка refresh_token (например, из базы данных)
            var username = ValidateRefreshToken(request.RefreshToken);
            if (username == null)
            {
                return Unauthorized();
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

            // Генерация нового access_token
            var newAccessTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var newAccessToken = tokenHandler.CreateToken(newAccessTokenDescriptor);
            var newAccessTokenString = tokenHandler.WriteToken(newAccessToken);

            return Ok(new
            {
                AccessToken = newAccessTokenString
            });
        }
        [HttpPost("validate")]
        [AllowAnonymous]
        public IActionResult ValidateRefreshToken([FromBody] RefreshTokenRequest request)
        {
            // Проверка refresh_token (например, из базы данных)
            var isValid = ValidateRefreshToken(request.RefreshToken);

            if (isValid==null)
            {
                return Ok(new { value = false });
            }

            return Ok(new { value = true });
        }
        private void SaveRefreshToken(string email, string refreshToken)
        {
            var existingToken = _app.RefreshTokens.FirstOrDefault(rt => rt.Email == email);

            if (existingToken != null)
            {
                // Если запись существует, обновляем её данные
                existingToken.Token = refreshToken;
                existingToken.ExpiresAt = DateTime.UtcNow.AddDays(7); // Обновляем время жизни токена
            }
            else
            {
                // Если записи нет, создаем новую
                var newToken = new RefreshTokenModel
                {
                    Email = email,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Время жизни refresh_token
                };

                _app.RefreshTokens.Add(newToken);
            }

            // Сохраняем изменения в базе данных
            _app.SaveChanges();
        }

        private string ValidateRefreshToken(string refreshToken)
        {
            var tokenEntity = _app.RefreshTokens
                .FirstOrDefault(rt => rt.Token == refreshToken && rt.ExpiresAt > DateTime.UtcNow);

            if (tokenEntity == null)
            {
                return null; // Токен недействителен или истек
            }

            return tokenEntity.Email;
        }
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    };

            var token = new JwtSecurityToken(
                issuer: "MyAuthServer",
                audience: "MyAuthClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Срок действия токена
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
