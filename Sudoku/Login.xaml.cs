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
    public partial class Login : Window
    {
        private readonly PlayerManager? _playerManager;
        public Player? SelectedPlayer { get; private set; }
        public Login(PlayerManager playerManager)
        {
            InitializeComponent();
            _playerManager = playerManager;
            PlayersCombo.ItemsSource = _playerManager.Players.OrderBy(player => player.Username).ToList();
            if (_playerManager.LastPlayer != null)
            {
                PlayersCombo.SelectedItem = _playerManager.LastPlayer;
            }
            PlayersCombo.SelectedIndex = 0;
        }
        //--УВІЙТИ--//
        private void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? name = PlayersCombo.Text;
                if (!string.IsNullOrWhiteSpace(name))
                    SelectedPlayer = _playerManager?.GetPlayer(name);
                else if (PlayersCombo.SelectedItem is Player selected)
                {
                    SelectedPlayer = selected;
                    _playerManager?.SetActive(selected);
                }
                DialogResult = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //--НОВИЙ ГРАВЕЦЬ--//
        private void NewPlayer_Btn_Click(object sender, RoutedEventArgs e)
        {
            string? name = Username_Txt.Text;
            if (name == "") return;
            bool nameVerifier = _playerManager!.Players.Any(player => string.Equals(player.Username, name, StringComparison.OrdinalIgnoreCase));
            if (nameVerifier) { MessageBox.Show("Будь оригінальніше з іменем"); return; }
            SelectedPlayer = _playerManager?.CreatePlayer(name);
            DialogResult = true;
        }

        private void Delete_Btn_Click(object sender, RoutedEventArgs e)
        {
            string? name = PlayersCombo.Text;
            try
            {
                if (MessageBox.Show("Ви дійсно хочете назавжди видатили цей профіль?", "Видалити профіль?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _playerManager?.DeletePlayer(name);
                    PlayersCombo.ItemsSource = null;
                    PlayersCombo.ItemsSource = _playerManager?.Players.OrderBy(player => player.Username).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
