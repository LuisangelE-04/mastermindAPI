using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mastermind.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MastermindGames") ?? "Data Source=MastermindGames.db";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlite<MastermindDb>(connectionString);
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Master Game API",
    Description = "A web API for playing the classic Mastermind code-breaking game",
    Version = "v1"
  });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mastermind Game API v1");
  });
}

app.MapGet("/", () => "Hello World!");

app.Run();
