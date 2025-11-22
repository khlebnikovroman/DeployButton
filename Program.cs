using Microsoft.AspNetCore.StaticFiles;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Enable CORS
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files from wwwroot

app.UseRouting();

app.MapControllers();

// Ensure music directory exists
var musicDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music");
if (!Directory.Exists(musicDirectory))
{
    Directory.CreateDirectory(musicDirectory);
}

app.Run();