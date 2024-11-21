using ReserveNow_API.Servises;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
ApplicationContext add = new ApplicationContext();
app.MapGet("/", () => "Hello World!");

app.Run();
