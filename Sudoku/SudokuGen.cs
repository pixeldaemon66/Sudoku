using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class SudokuGen
    {
        private static readonly Random Rng = new Random();
        public int[,] SudokuGrid { get; private set; }
        public int[,] GameGrid { get; private set; }

        public SudokuGen(int empty_cells)
        {
            SudokuGrid = new int[9, 9];
            GameGrid = new int[9, 9];
            GenerateNewGame(empty_cells);
        }

        public void GenerateNewGame(int empty_cells)
        {
            Array.Clear(SudokuGrid, 0, SudokuGrid.Length);
            Fill(SudokuGrid);

            Array.Copy(SudokuGrid, GameGrid, SudokuGrid.Length);
            RemoveElements(GameGrid, empty_cells);
        }

        public bool isSafe(int[,] grid, int row, int col, int n)
        {
            for (int i = 0; i < 9; i++)
                if (grid[row, i] == n || grid[i, col] == n) return false;

            int start_row = row - row % 3;
            int start_col = col - col % 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (grid[start_row + i, start_col + j] == n) return false;

            return true;
        }

        public bool Fill(int[,] grid)
        {
            for(int row = 0; row < 9; row++)
                for(int col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        foreach(int n in Enumerable.Range(1, 9).OrderBy(_ => Rng.Next()).ToList())
                            if(isSafe(grid, row, col, n))
                            {
                                grid[row, col] = n;
                                if (Fill(grid)) return true;
                                grid[row, col] = 0;
                            }
                        return false;
                    }
                }

            return true;
        }
        public void RemoveElements(int[,] grid, int empty_cells)
        {
            int removed = 0;
            while(removed < empty_cells)
            {
                int cell = Rng.Next(81);
                int row = cell / 9;
                int col = cell % 9;
                if (grid[row, col] != 0)
                {
                    grid[row, col] = 0;
                    removed++;
                }
            }
        }
    }
}
