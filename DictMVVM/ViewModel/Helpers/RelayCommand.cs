﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DictMVVM.ViewModel.Helpers
{
    internal class RelayCommand : ICommand
    {
        //private readonly Action _execute;
        //private readonly Func<bool> _canExecute;

        //public RelayCommand(Action execute)
        //    : this(execute, null)
        //{
        //}

        //public RelayCommand(Action execute, Func<bool> canExecute)
        //{
        //    _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        //    _canExecute = canExecute;
        //}

        //public event EventHandler CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        //public bool CanExecute(object parameter)
        //{
        //    return _canExecute?.Invoke() ?? true;
        //}

        //public void Execute(object parameter)
        //{
        //    _execute();
        //}

        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}