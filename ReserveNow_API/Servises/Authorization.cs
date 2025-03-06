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

namespace ReserveNow_API.Servises
{
    public class Authorization : IAuthorization
    {
        public ApplicationContext db;
        private readonly IConfiguration _config;
 
        public Authorization(ApplicationContext db, IConfiguration config) 
        {
            this.db = db;
            _config = config;
        }
        public string GenerateToken(string username)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, username)
        };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Срок действия токена
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }   
}
