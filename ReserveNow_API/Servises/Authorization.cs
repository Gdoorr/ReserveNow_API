using System.Collections.Generic;
using System;
using System.Linq;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ReserveNow_API.Models.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ReserveNow_API.Servises
{
    public class Authorization : IAuthorization
    {
        public ApplicationContext db;
        private readonly IConfiguration _config;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenLifetimeDays;

        public Authorization(ApplicationContext db, IConfiguration config)
        {
            this.db = db;
            _config = config;
            _secretKey = _config["JwtSettings:SecretKey"];
            _issuer = _config["JwtSettings:Issuer"];
            _audience = _config["JwtSettings:Audience"];
            _tokenLifetimeDays = int.Parse(_config["JwtSettings:TokenLifetimeDays"]);
        }
        public string GenerateToken(string userId, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenLifetimeDays),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string RefreshToken(string oldToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Не проверяем срок действия, так как токен может быть просрочен
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey))
            };

            try
            {
                // Проверяем подпись старого токена
                var principal = tokenHandler.ValidateToken(oldToken, validationParameters, out var validatedToken);

                // Извлекаем информацию из старого токена
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (userId == null || role == null)
                {
                    throw new SecurityTokenException("Invalid token claims.");
                }

                // Генерируем новый токен
                return GenerateToken(userId, role);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid or expired token.", ex);
            }
        }
        public Clients FindUserByUsername(string email)
        {
            return db.Client.FirstOrDefault(u => u.Email == email);
        }

        public bool VerifyPassword(Clients user, string password)
        { 
            if (user == null) return false;

            // Предполагается, что пароли хранятся в захешированном виде
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }   
}
