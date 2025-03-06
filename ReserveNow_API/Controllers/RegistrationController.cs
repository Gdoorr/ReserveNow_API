using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReserveNow_API.Models;
using ReserveNow_API.Models.Classes;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;

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
        public async Task<ActionResult> Login([FromBody] Clients client)
        {
            var person = _app.Client.FirstOrDefault(x=>x.Email==client.Email&&x.Password==client.Password);
            if (person!=null)
            {
                var token = _auth.GenerateToken(client.Email);
                return Ok(new { Token = token });
            }

            return Unauthorized(new { Message = "Неверные учётные данные." });
        }
    }
}
