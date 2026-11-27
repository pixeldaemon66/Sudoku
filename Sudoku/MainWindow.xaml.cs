using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.RightsManagement;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Sudoku
{
    public partial class MainWindow : Window
    {
        private SudokuGen Gen; 
        public ObservableCollection<CellViewModel> Cells { get; set; }
        private readonly PlayerManager _playerManager;
        public Player _currentPlayer;
        public int empty_cells { get; set; }
        public int mistakes = 0;
        public int hints = 3;
        public int score;
        private bool is_finished = true;

        private Stopwatch _stopwatch = new();
        private DispatcherTimer _timer;
        private bool isTimerStarted;
        private bool isTimerVisible = true;
        private CellViewModel? _selected { get; set; }

        //--ЗАВАНТАЖЕННЯ ПОЛЯ--//
        private void Load()
        {
            Cells.Clear();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    int val = Gen.GameGrid[r, c];
                    bool isFixed = val != 0;
                    Cells.Add(new CellViewModel(r, c, val, isFixed));
                }
        }
        //--ВИБІР КЛІТИНКИ--//
        private void Select(CellViewModel? cell)
        {
            if (cell == null) return;
            _selected = cell;
            foreach (var c in Cells)
                c.IsSelected = c == _selected;
        }
        //--ПЕРЕВІРКА НА ВИГРАШ--//
        private void CheckWin()
        {
            if (Cells.Count != 81) return;
            bool complete = Cells.All(cell => cell.Value != 0 && cell.Value == Gen.SudokuGrid[cell.Row, cell.Col]);
            if (!complete) return;
            is_finished = true;
            _selected = null;
            StopTimer();
            score = CalculateScore();
            var result = new GameResults
            {
                Score = score,
                Difficulty = GetDifficultyName(empty_cells),
                Mistakes = mistakes,
                Duration = (long)_stopwatch.Elapsed.TotalSeconds
            };
            try
            {
                _playerManager.SaveResult(_currentPlayer, result);
                MessageBox.Show(
                    $"Вітаю, {_currentPlayer.Username}! Ви перемогли.\n\n" +
                    $"Складність: {result.Difficulty}\n" +
                    $"Час: {_stopwatch.Elapsed:hh\\:mm\\:ss}\n" +
                    $"Помилок: {mistakes}\n" +
                    $"Результат: {score} очок\n\n" +
                    "Результат збережено в профілі.",
                    "Перемога!", MessageBoxButton.OK, MessageBoxImage.Information);
                new Leaderboard(_playerManager) { Owner = this}.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //--ПІДРАХУНОК ОЧОК--//
        private int CalculateScore()
        {
            int baseScore = empty_cells switch
            {
                30 => 100,
                40 => 200,
                50 => 350,
                60 => 500,
                _ => 100
            };
            int timeBonus = Math.Max(0, 100 - (int)(_stopwatch.Elapsed.TotalSeconds / 30));
            return Math.Max(0, baseScore + timeBonus - mistakes * 10 - hints * 5);
        }
        //--ПОВЕРНЕННЯ РІВНЯ СКЛАДНОСТІ ДЛЯ ТАБЛИЦІ РЕЗУЛЬТАТІВ--//
        private static string GetDifficultyName(int emptyCells) => emptyCells switch
        {
            30 => "Легкий",
            40 => "Середній",
            50 => "Складний",
            60 => "Експертний",
            _ => "Легкий"
        };
        //--ПЕРЕВІРКА КЛІТИНОК--//
        private bool Validate(CellViewModel cell)
        {
            if (cell.Value != Gen.SudokuGrid[cell.Row, cell.Col] && cell.Value != 0)
                return false;
            return true;
        }

        public MainWindow() : this(new PlayerManager(), null!) { } // DEFAULT
        //--КОНСТРУКТОР ВІКНА--//
        public MainWindow(PlayerManager playerManager, Player currentPlayer)
        {
            InitializeComponent();
            DataContext = this;
            HideField();
            Cells = new ObservableCollection<CellViewModel>();
            _playerManager = playerManager;
            _currentPlayer = currentPlayer ?? playerManager.LastPlayer ?? new Player { Username = "Default" };
            Username_Txt.Text = _currentPlayer.Username;
            _timer = new() { Interval = TimeSpan.FromSeconds(0) };
            _timer.Tick += (_, _) =>
                Timer_Lbl.Content = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
        }
        //--СХОВАТИ ВСЕ--//
        private void HideField()
        {
            SudokuBoard.Visibility = Visibility.Hidden;
            Hint_Btn.Visibility = Visibility.Hidden;
            Hints.Visibility = Visibility.Hidden;
            Timer_Lbl.Visibility = Visibility.Hidden;
            HideTimer_Btn.Visibility = Visibility.Hidden;
            Reset_Btn.Visibility = Visibility.Hidden;
        }
        //--ПОКАЗАТИ ВСЕ--//
        private void ShowField()
        {
            SudokuBoard.Visibility = Visibility.Visible;
            Hint_Btn.Visibility = Visibility.Visible;
            Hints.Visibility = Visibility.Visible;
            Timer_Lbl.Visibility = Visibility.Visible;
            HideTimer_Btn.Visibility = Visibility.Visible;
            Reset_Btn.Visibility = Visibility.Visible;
        }
        //--КЕРУВАННЯ ТАЙМЕРОМ--//
        private void StartTimer()
        {
            _stopwatch.Reset();
            _stopwatch.Start();
            _timer.Start();
            isTimerStarted = true;
            Timer_Lbl.Content = "00:00:00";
        }
        private void StopTimer()
        {
            _stopwatch.Stop();
            _timer.Stop();
            isTimerStarted = false;
        }
        //--ОНОВИТИ ЗНАЧЕННЯ КІЛЬКОСТІ ПІДКАЗОК--//
        private void UpdateHints() => Hints.Text = (3 - hints).ToString();
        //--ВВЕДЕННЯ ЦИФРИ--//
        private void Cell_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (is_finished ||
                sender is not TextBox textBox ||
                textBox.DataContext is not CellViewModel cell ||
                cell.IsFixed || 
                !int.TryParse(e.Text, out int value) || 
                value < 1 || value > 9)
            {
                e.Handled = true;
                return; // значення не введеться
            }

            cell.Value = int.Parse(e.Text);
            cell.HasError = false;
            if (!Validate(cell))
            {
                cell.HasError = true;
                cell.IsSelected = false;
                mistakes++;
            }
            CheckWin();
        }
        //--ВИДАЛЕННЯ ЦИФРИ З КЛІТИНКИ--//
        private void Cell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (is_finished) return;
            if (sender is not TextBox textBox || textBox.DataContext is not CellViewModel cell) return;
            if (cell.IsFixed) return;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (cell.Value != 0)
                {
                    cell.Value = 0;
                    cell.HasError = false;
                }
            }
        }
        //--НАТИСНЕННЯ НА КЛІТИНКУ--//
        private void Cell_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (is_finished) return;
            if (sender is not TextBox textbox || textbox.DataContext is not CellViewModel cell) return;
            if (e.ClickCount == 1)
            {
                Select(cell);
            }
        }
        //--ВИКЛИК ВІКНА НОВА ГРА--//
        private void NewGame_Btn_Click(object sender, RoutedEventArgs e)
        {
            NewGame newgame = new() { Owner = this };
            bool? result = newgame.ShowDialog();
            if(result == true)
            {
                Board.Visibility = Visibility.Visible;
                empty_cells = newgame.empty_cells;
                Gen = new SudokuGen(empty_cells);
                Load();
                is_finished = false;
                mistakes = 0;
                score = 0;
                hints = 0;
                StartTimer();
                UpdateHints();
                ShowField();
            }
        }
        //--ВИХІД--//
        private void Quit_Btn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Ви дійсно хочете вийти з гри?", "Вийти?", MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
                Close();
        }
        //--СКИНУТИ ВВЕДЕНІ КЛІТИНИ--//
        private void Reset_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (is_finished) return;
            foreach (var cell in Cells)
                if (!cell.IsFixed)
                    cell.Value = 0;
        }
        //--ПІДКАЗКА--//
        private void Hint_Click(object sender, MouseButtonEventArgs e)
        {
            if(hints == 3)
            {
                MessageBox.Show("У вас закінчилися підказки", "Sudoku", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var target = _selected;
            if (target == null) return;
            target.Value = Gen.SudokuGrid[target.Row, target.Col];
            target.HasError = false;
            hints++;
            UpdateHints();
        }
        //--ЛОГІНКА--//
        private void Player_Btn_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Login(_playerManager) { Owner = this };
            if (!is_finished)
                if (MessageBox.Show("При переході на інший ігровий профіль, цю гру буде перервано без збереження результату. Продовжити?", "Sudoku", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    HideField();
                    StopTimer();
                    return;
                };
            if (loginWindow.ShowDialog() == true && loginWindow.SelectedPlayer != null)
            {
                _currentPlayer = loginWindow.SelectedPlayer;
                Username_Txt.Text = _currentPlayer.Username;
            }
        }
        //--СХОВАТИ-ПОКАЗАТИ ТАЙМЕР--//
        private void HideTimer_Btn_Click(object sender, RoutedEventArgs e)
        {
            Timer_Lbl.Visibility = isTimerVisible == true ? Visibility.Hidden : Visibility.Visible;
            HideTimer_Btn.Content = isTimerVisible == true ? "Показати таймер" : "Сховати таймер";
            isTimerVisible = !isTimerVisible;
        }
        //--ВИКЛИК КНОПКОЮ ТАБЛИЦІ РЕКОРДІВ--//
        private void Leaderboards_Btn_Click(object sender, RoutedEventArgs e)
        {
            new Leaderboard(_playerManager) { Owner = this }.ShowDialog();
        }
        //--ЗДАТИСЯ--//
        private void GiveUp_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Gen == null || is_finished) return;
            if (MessageBox.Show("Завершити гру без збереження результату?", "Здатися?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                is_finished = true;
                StopTimer();
                HideField();
            }
        }
        //--ЯК ГРАТИ--//
        private void Manual_Btn_Click(object sender, RoutedEventArgs e)
        {
            Manual manual = new() { Owner = this };
            manual.Show();
        }
    }
}