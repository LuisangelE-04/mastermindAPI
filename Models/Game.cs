using Microsoft.EntityFrameworkCore;

namespace Mastermind.Models
{
  public class Game
  {
    public int Id { get; set; }
    public string SecretCode { get; set; } = string.Empty;
    public List<GameGuess> Guesses { get; set; } = new();
    public int Attempts { get; set; } = 0;
    public GameStatus Status { get; set; } = GameStatus.IN_PROGRESS;
  }

  public class GameGuess
  {
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game GameEntity { get; set; } = null!;
    public string GuessCode { get; set; } = string.Empty;
    public int ExactMatches { get; set; }
    public int PositionMatches { get; set; }
    public int NoMatches { get; set; }
    public int AttemptNumber { get; set; }
  }

  public enum GameStatus
  {
    IN_PROGRESS,
    WON,
    LOST
  }

  public enum Colors
  {
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Orange,
    Pink,
    Brown,
    Silver,
    Black
  }

  public enum Match
  {
    EXACT,
    POSITION,
    NONE
  }

  public class MastermindDb : DbContext
  {
    public MastermindDb(DbContextOptions options) : base(options) { }
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<GameGuess> GameGuesses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<GameGuess>()
        .HasOne(g => g.GameEntity)
        .WithMany(game => game.Guesses)
        .HasForeignKey(g => g.GameId);
    }
  }
}