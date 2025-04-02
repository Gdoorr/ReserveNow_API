using ReserveNow_API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReserveNow_API.Models.Interfaces;
using ReserveNow_API.Servises;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using static ReserveNow_API.AuthOptions;
using ReserveNow_API;
using System.Text;
using ReserveNow_API.Models.Classes;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
builder.Services.AddDbContext<ApplicationContext>();
builder.Services.AddControllers();
builder.Services.AddScoped<Clients>();
builder.Services.AddScoped<IAuthorization, Authorization>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                    var secretKey = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(secretKey),

                            ValidateIssuer = true,
                            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

                            ValidateAudience = true,
                            ValidAudience = builder.Configuration["JwtSettings:Audience"],

                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero // ”читывать разницу времени между серверами
                        };
                    });

// ƒобавление CORS (пример)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin() // или конкретный origin
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseMvcWithDefaultRoute(); // не требуетс€ в данном случае
app.UseCors("CorsPolicy"); // если нужен CORS
app.UseRouting();
app.UseAuthentication(); // !! јвторизаци€ после маршрутизации
app.UseAuthorization(); // !! и перед сопоставлением контроллеров

app.MapControllers(); // !! ƒобавлен сопоставитель контроллеров !!

app.MapGet("/", () => "Hello World!");

app.Run();