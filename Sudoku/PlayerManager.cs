using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace Sudoku
{
    public class PlayerManager
    {
        private sealed class PlayerDatabase
        {
            public PlayerDatabase() { }
            public Guid LastPlayerID { get; set; }
            public List<Player> Players { get; set; } = new();

        }

        private readonly string path;
        private readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private PlayerDatabase Database = new();
        public IReadOnlyList<Player> Players => Database.Players;
        public Player? LastPlayer => Database.LastPlayerID is Guid _ID ?
            Database.Players.FirstOrDefault(player => player.ID == _ID) : null;
        public PlayerManager()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sudoku");
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, "players.json");
            Load();
        }

        public Player GetPlayer(string username)
        {
            string normalizedName = username.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Enter name.", nameof(username));
            Player? existing = Database.Players.FirstOrDefault(player =>
                string.Equals(player.Username, normalizedName, StringComparison.OrdinalIgnoreCase));
            Player player = existing ?? new Player { Username = normalizedName };
            SetActive(player);
            return player;
        }
        public Player CreatePlayer(string username)
        {
            string normalizedName = username.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Enter name.", nameof(username));
            Player player = new Player { Username = normalizedName };
            Database.Players.Add(player);
            player.ID = Guid.NewGuid();
            SetActive(player);
            return (player);
        }
        public void DeletePlayer(string username)
        {
            string normalizedName = username.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentException("Enter name.", nameof(username));
            Player? player = Database.Players.FirstOrDefault(p => p.Username == normalizedName);
            if (player == null) return;
            Database.Players.Remove(player);
            Save();
        }
        public void SetActive(Player player)
        {
            if (!Database.Players.Any(item => item.ID == player.ID))
                throw new InvalidOperationException("No player");
            Database.LastPlayerID = player.ID;
            Save();
        }
        
        private void Load()
        {
            if (!File.Exists(path))
            {
                Database = new PlayerDatabase();
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string json = reader.ReadToEnd();
                    Database = JsonSerializer.Deserialize<PlayerDatabase>(json, jsonOptions) 
                        ?? new PlayerDatabase();
                    foreach (Player p in Database.Players)
                        p.Results ??= new List<GameResults>();
                }
            }
            catch (JsonException)
            {
                string backup = path + ".corrupt-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                File.Move(path, backup);
                Database = new PlayerDatabase();
            }
        }

        private void Save()
        {
            string json = JsonSerializer.Serialize(Database, jsonOptions);
            string tmp = path + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, path, true);
        }
        public void SaveResult(Player player, GameResults gameResults)
        {
            Player? storedPlayer = Database.Players.FirstOrDefault(item => item.ID == player.ID) ?? throw new InvalidOperationException("Not found");
            storedPlayer.Results.Add(gameResults);
            Database.LastPlayerID = storedPlayer.ID;
            Save();
        }
        public List<Player> GetLeaderBoard() => Database.Players
            .Where(player => player.Results.Count > 0)
            .OrderByDescending(player => player.BestScore)
            .ThenBy(player => player.Username)
            .ToList();
    }
}
