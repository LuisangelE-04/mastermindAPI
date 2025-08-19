using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mastermind.Models;
using Mastermind.Services;
using Mastermind.DTOs;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "AllowLocalHost";

var connectionString = builder.Configuration.GetConnectionString("MastermindGames") ?? "Data Source=MastermindGames.db";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSqlite<MastermindDb>(connectionString);
builder.Services.AddScoped<GameLogicService>();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Master Game API",
    Description = "A web API for playing the classic Mastermind code-breaking game",
    Version = "v1"
  });
});
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                      policy.WithOrigins("http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
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

app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello World!");


app.MapPost("/games", async (MastermindDb db, GameLogicService gameLogic, CreateGameRequest request) =>
{
  var secretCode = gameLogic.GenerateSecretCode(request.Seed);
  var secretCodeString = gameLogic.ColorsToString(secretCode);

  var game = new Game
  {
    SecretCode = secretCodeString,
    Status = GameStatus.IN_PROGRESS,
    Attempts = 0,
  };

  await db.Games.AddAsync(game);
  await db.SaveChangesAsync();

  return Results.Created($"/games/{game.Id}", new CreateGameResponse
  {
    GameId = game.Id,
    Status = game.Status.ToString(),
    Attempts = game.Attempts,
  });
})
.WithName("CreateGame")
.WithSummary("Create a new Mastermind game")
.WithDescription("Creates a new game with a randomly generated secret code");


app.MapGet("/games", async (MastermindDb db) =>
{
  var games = await db.Games.Select(g => new
  {
    g.Id,
    g.Status,
    g.Attempts,
    SecretCode = g.Status != GameStatus.IN_PROGRESS ? g.SecretCode : null
  })
  .ToListAsync();

  return Results.Ok(games);
})
.WithName("GetAllGames")
.WithSummary("Get all games")
.WithDescription("Retrieves all games (secret codes only shown for completed games)");


app.MapGet("/games/{id}", async (MastermindDb db, GameLogicService gameLogix, int id) =>
{
  var game = await db.Games.Include(g => g.Guesses).FirstOrDefaultAsync(g => g.Id == id);

  if (game is null) return Results.NotFound();

  var response = new GameStateResponse
  {
    GameId = game.Id,
    Status = game.Status.ToString(),
    Attempts = game.Attempts,
    GuessHistory = game.Guesses.OrderBy(g => g.AttemptNumber).Select(g => new GuessHistoryItem
    {
      Guess = g.GuessCode.Split(',').ToList(),
      ExactMatches = g.ExactMatches,
      PositionMatches = g.PositionMatches,
      NoMatches = g.NoMatches,
      AttemptNumber = g.AttemptNumber,
    }).ToList()
  };

  if (game.Status != GameStatus.IN_PROGRESS) response.SecretCode = game.SecretCode.Split(',').ToList();

  return Results.Ok(response);
})
.WithName("GetGameState")
.WithSummary("Get game state")
.WithDescription("Retrieves the current state of a game including guess history");


app.MapPost("/games/{id}/guess", async (MastermindDb db, GameLogicService gameLogic, int id, MakeGameRequest request) =>
{
  var game = await db.Games.Include(g => g.Guesses).FirstOrDefaultAsync(g => g.Id == id);

  if (game is null) return Results.NotFound();

  if (game.Status != GameStatus.IN_PROGRESS) return Results.BadRequest("Game is already finished");

  try
  {
    var guessColors = request.Colors.Select(c => Enum.Parse<Colors>(c, true)).ToList();

    if (!gameLogic.IsValidGuess(guessColors)) return Results.BadRequest("Invalid guess. Must provide exactly 6 valid colors");

    var secretColors = gameLogic.StringToColors(game.SecretCode);
    var guessResult = gameLogic.EvaluateGuess(secretColors, guessColors);

    game.Attempts++;
    game.Status = gameLogic.DetermineGameStatus(guessResult, game.Attempts);

    var gameGuess = new GameGuess
    {
      GameId = game.Id,
      GuessCode = gameLogic.ColorsToString(guessColors),
      ExactMatches = guessResult[Match.EXACT],
      PositionMatches = guessResult[Match.POSITION],
      NoMatches = guessResult[Match.NONE],
      AttemptNumber = game.Attempts
    };

    await db.GameGuesses.AddAsync(gameGuess);
    await db.SaveChangesAsync();

    var response = new MakeGuessResponse
    {
      GameId = game.Id,
      Guess = request.Colors,
      ExactMatches = guessResult[Match.EXACT],
      PositionMatches = guessResult[Match.POSITION],
      NoMatches = guessResult[Match.NONE],
      AttemptNumber = game.Attempts,
      GameStatus = game.Status.ToString(),
      IsGameOver = game.Status != GameStatus.IN_PROGRESS,
    };

    if (game.Status != GameStatus.IN_PROGRESS) response.SecretCode = game.SecretCode.Split(',').ToList();

    return Results.Ok(response);
  }
  catch (ArgumentException)
  {
    return Results.BadRequest("Invalid Color names provided");
  }
})
.WithName("MakeGuess")
.WithSummary("Make a guess")
.WithDescription("Submit a guess for the secret code");


app.MapDelete("/games/{id}", async (MastermindDb db, int id) =>
{
  var game = await db.Games.FindAsync(id);
  if (game is null) return Results.NotFound();

  db.Games.Remove(game);
  await db.SaveChangesAsync();
  return Results.Ok();
})
.WithName("DeleteGames")
.WithSummary("Delete a game")
.WithDescription("Deletes a game and all its associated guesses");


app.MapGet("/colors", () =>
{
  var colors = Enum.GetValues<Colors>().Select(c => c.ToString()).ToList();

  return Results.Ok(colors);
})
.WithName("GetAvailableColors")
.WithSummary("Get available colors")
.WithDescription("Returns all available colors that can be used in the game");


app.Run();
