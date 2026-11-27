using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sudoku
{
    public partial class Leaderboard : Window
    {
        public sealed class LeaderboardRow
        {
            public int Rank { get; set; }
            public string Username { get; set; } = string.Empty;
            public int BestScore { get; set; }
            public int TotalGames { get; set; }
        }
        public Leaderboard(PlayerManager playerManager)
        {
            InitializeComponent();
            LeaderboardGrid.ItemsSource = playerManager.GetLeaderBoard()
                .Select((player, index) => new LeaderboardRow
                {
                    Rank = index + 1,
                    Username = player.Username,
                    BestScore = player.BestScore,
                    TotalGames = player.TotalGames
                }).ToList();
        }
    }
}
