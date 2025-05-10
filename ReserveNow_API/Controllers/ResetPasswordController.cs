using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveNow_API.Models;
using ReserveNow_API.Servises;

namespace ReserveNow_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResetPasswordController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationContext _dbContext;
        private static Dictionary<string, string> _resetCodes = new();

        public ResetPasswordController(IEmailService emailService, ApplicationContext dbContext)
        {
            _emailService = emailService;
            _dbContext = dbContext;
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Генерация 6-значного кода
            var code = new Random().Next(100000, 999999).ToString();
            _resetCodes[request.Email] = code;

            // Отправка кода на email
            await _emailService.SendEmailAsync(request.Email, "Восстановление пароля", $"Ваш код: {code}");

            return Ok(new { message = "Код отправлен на ваш email." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!_resetCodes.TryGetValue(request.Email, out var storedCode) || storedCode != request.ResetCode)
            {
                return BadRequest(new { message = "Неверный код." });
            }
            var user = await _dbContext.Client.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не найден." });
            }

            // Хеширование нового пароля
            user.Password = HashPassword(request.NewPassword);

            // Сохранение изменений в базе данных
            _dbContext.Client.Update(user);
            await _dbContext.SaveChangesAsync();
            _resetCodes.Remove(request.Email);

            return Ok(new { message = "Пароль успешно обновлен." });
        }
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
