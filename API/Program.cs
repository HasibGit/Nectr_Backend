using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

});
builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

app.UseCors(x => x.AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithOrigins("http://localhost:4200", "http://localhost:4200"));

app.MapControllers();

app.Run();
