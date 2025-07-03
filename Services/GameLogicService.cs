using Mastermind.Models;

namespace Mastermind.Services
{
  public class GameLogicService
  {
    private static readonly List<Colors> AllColors = new List<Colors>
    {
      Colors.Red,
      Colors.Blue,
      Colors.Green,
      Colors.Yellow,
      Colors.Purple,
      Colors.Orange,
      Colors.Pink,
      Colors.Brown,
      Colors.Silver,
      Colors.Black
    };

    private const int MAX_COLORS = 6, MAX_ATTEMPTS = 20;

    public List<Colors> GenerateSecretCode(int? seed = null)
    {
      Random random = seed.HasValue ? new Random(seed.Value) : new Random();

      return AllColors.OrderBy(color => random.Next()).Take(MAX_COLORS).ToList();
    }

    public string ColorsToString(IEnumerable<Colors> colors)
    {
      return string.Join(",", colors.Select(c => c.ToString()));
    }

    public List<Colors> StringToColors(string colorString)
    {
      return colorString.Split(',').Select(c => Enum.Parse<Colors>(c)).ToList();
    }

    public Dictionary<Match, int> EvaluateGuess(IEnumerable<Colors> secretCode, IEnumerable<Colors> guess)
    {
      var matches = Enumerable.Range(0, MAX_COLORS).Select(positionIndex => MatchForPosition(positionIndex, secretCode, guess));

      var results = matches.GroupBy(match => match).ToDictionary(group => group.Key, group => group.Count());

      foreach (Match match in Enum.GetValues(typeof(Match)))
      {
        if (!results.ContainsKey(match)) results[match] = 0;
      }

      return results;
    }

    private Match MatchForPosition(int positionIndex, IEnumerable<Colors> secretCode, IEnumerable<Colors> guess)
    {
      var candidateColor = guess.ElementAt(positionIndex);

      if (candidateColor == secretCode.ElementAt(positionIndex))
      {
        return Match.EXACT;
      }

      if (guess.Take(positionIndex).Contains(candidateColor))
      {
        return Match.NONE;
      }

      var index = secretCode.ToList().IndexOf(candidateColor);

      if (index > -1 && secretCode.ElementAt(index) != guess.ElementAt(index))
      {
        return Match.POSITION;
      }

      return Match.NONE;
    }

    public GameStatus DetermineGameStatus(Dictionary<Match, int> guessResult, int attempts)
    {
      if (guessResult[Match.EXACT] == MAX_COLORS) return GameStatus.WON;

      if (attempts >= MAX_ATTEMPTS) return GameStatus.LOST;

      return GameStatus.IN_PROGRESS;
    }

    public bool IsValidGuess(IEnumerable<Colors> guess)
    {
      return guess.Count() == MAX_COLORS && guess.All(c => AllColors.Contains(c));
    }
    
  }
}