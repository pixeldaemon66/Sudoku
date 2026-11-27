using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sudoku
{
    public class CellViewModel : INotifyPropertyChanged
    {
        private int _value;
        private bool _isFixed, _isSelected, _hasError;
        public int Row { get; }
        public int Col { get; }
        public CellViewModel(int row, int col, int val, bool isFixed)
        {
            Row = row;
            Col = col;
            _value = val;
            _isFixed = isFixed;
        }
        public bool IsFixed => _isFixed;
        public bool HasError{
            get => _hasError;
            set => SetField(ref _hasError, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }
        public Thickness BorderThickness => new(
            Col % 3 == 0 ? 1 : 0.5,
            Row % 3 == 0 ? 1 : 0.5,
            Col == 8 ? 1 : 0.5,
            Row == 8 ? 1 : 0.5
        );

        public int Value
        {
            get => _value;
            set
            {
                if (IsFixed || value < 0 || value > 9) return;
                if (SetField(ref _value, value))
                {
                    _value = value;
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(Text));
                }
            }
        }
        public string Text => _value == 0 ? "" : _value.ToString();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}
