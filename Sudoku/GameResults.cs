using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class GameResults
    {
        public int Score { get; set; }
        public string Difficulty { get; set; } = "Легкий";
        public int Mistakes { get; set; }
        public long Duration { get; set; }
    }
}
