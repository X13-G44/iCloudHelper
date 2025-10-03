using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;



namespace AutoUnzip.viewmodel
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;



        public RelayCommand (Action<object> execute)
            : this (execute, null)
        {
        }



        public RelayCommand (Action<object> execute, Predicate<object> canExecute, object hostInstance = null)
        {
            _execute = execute ?? throw new ArgumentNullException (nameof (execute));
            _canExecute = canExecute;
        }



        public bool CanExecute (object parameter)
        {
            return _canExecute == null || _canExecute (parameter);
        }



        public void Execute (object parameter)
        {
            _execute?.Invoke (parameter);
        }



        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }



    public class RelayCommand<T1, T2> : ICommand
    {
        private readonly Action<object, T1, T2> _execute;
        private readonly Func<object, T1, T2, bool> _canExecute;
        private readonly T1 _viewModel;
        private readonly T2 _userData;



        public RelayCommand (Action<object, T1, T2> execute,
                                                 T1 viewModel,
                                                 T2 userData)
            : this (execute, null, viewModel, userData)
        {
        }



        public RelayCommand (Action<object, T1, T2> execute, 
                                                 Func<object, T1, T2, bool> canExecute, 
                                                 T1 viewModel, 
                                                 T2 userData)
        {
            _execute = execute ?? throw new ArgumentNullException (nameof (execute));
            _canExecute = canExecute;
            _viewModel = viewModel;
            _userData = userData;
        }



        public bool CanExecute (object parameter)
        {
            return _canExecute == null || _canExecute (parameter, _viewModel, _userData);
        }



        public void Execute (object parameter)
        {
            _execute?.Invoke (parameter, _viewModel, _userData);
        }



        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

