namespace Mastermind.DTOs
{
  public class CreateGameRequest
  {
    public int? Seed { get; set; }
  }

  public class CreateGameResponse
  {
    public int GameId { get; set; }
    public string Status { get; set; } = "IN_PROGRESS";
    public int Attempts { get; set; } = 0;
  }

  public class MakeGameRequest
  {
    public List<string> Colors { get; set; } = new();
  }

  public class MakeGuessResponse
  {
    public int GameId { get; set; }
    public List<string> Guess { get; set; } = new();
    public int ExactMatches { get; set; }
    public int PositionMatches { get; set; }
    public int NoMatches { get; set; }
    public int AttemptNumber { get; set; }
    public string GameStatus { get; set; } = string.Empty;
    public bool IsGameOver { get; set; }
    public List<string>? SecretCode { get; set; }
  }

  public class GameStateResponse
  {
    public int GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public List<GuessHistoryItem> GuessHistory { get; set; } = new();
    public List<string>? SecretCode { get; set; }
  }

  public class GuessHistoryItem
  {
    public List<string> Guess { get; set; } = new();
    public int ExactMatches { get; set; }
    public int PositionMatches { get; set; }
    public int NoMatches { get; set; }
    public int AttemptNumber { get; set; }
  }
  
}