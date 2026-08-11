// [EN] Synchronous command implementation for MVVM
// [RU] Синхронная команда для MVVM
// [ZH] MVVM的同步命令实现
// [FA] پیاده‌سازی فرمان همزمان برای MVVM

using System;
using System.Windows.Input;

namespace Forestgram.Core.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}