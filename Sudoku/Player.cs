using System.Text.Json.Serialization;

namespace Sudoku
{
    public class Player
    {
        public Guid ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<GameResults> Results { get; set; } = new();

        [JsonIgnore]
        public int TotalGames => Results.Count();
        [JsonIgnore]
        public int BestScore => Results.Count == 0 ? 0 : Results.Max(result =>  result.Score);
    }
}
