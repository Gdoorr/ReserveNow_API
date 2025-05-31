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
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]; 
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
builder.Services.AddDbContext<ApplicationContext>();
builder.Services.AddControllers();
    //.AddJsonOptions(options =>
    //{
    //    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    //    options.JsonSerializerOptions.WriteIndented = true; // Optional: For pretty-printed JSON
    //});
builder.Services.AddScoped<Client>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IAuthorization, Authorization>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddTransient<IEmailService, EmailService>();


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
                            ClockSkew = TimeSpan.Zero // ��������� ������� ������� ����� ���������
                        };
                    });
//builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
    });// ������������ ���� 5000 �� ���� �����������
});
// ���������� CORS (������)
builder.Services.AddCors(options =>
{
    //options.AddPolicy("CorsPolicy", builder =>
    //{
    //    builder.AllowAnyOrigin() // ��� ���������� origin
    //           .AllowAnyMethod()
    //           .AllowAnyHeader();
    //});
    //options.AddPolicy("AllowLocalhost", policy =>
    //{
    //    policy.WithOrigins("http://localhost:5000") // ������� ���� ������ �������
    //          .AllowAnyMethod()
    //          .AllowAnyHeader();
    //});
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
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

//app.UseMvcWithDefaultRoute(); // �� ��������� � ������ ������
// ���� ����� CORS
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication(); // !! ����������� ����� �������������
app.UseAuthorization(); // !! � ����� �������������� ������������

app.MapControllers(); // !! �������� ������������� ������������ !!

app.MapGet("/", () => "Hello World!");

app.Run();