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
            _execute (parameter);
        }



        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }



    public class RelayCommandWithAdditionalFields : ICommand
    {
        private readonly Action<object, object, object> _execute;
        private readonly Func<object, object, object, bool> _canExecute;
        private readonly Object _hostInstance;
        private readonly Object _userData;



        public RelayCommandWithAdditionalFields (Action<object, object, object> execute)
            : this (execute, null)
        {
        }



        public RelayCommandWithAdditionalFields (Action<object, object, object> execute, Func<object, object, object, bool> canExecute, object hostInstance = null, object userData = null)
        {
            _execute = execute ?? throw new ArgumentNullException (nameof (execute));
            _canExecute = canExecute;
            _hostInstance = hostInstance;
            _userData = userData;
        }



        public bool CanExecute (object parameter)
        {
            return _canExecute == null || _canExecute (parameter, _hostInstance, _userData);
        }



        public void Execute (object parameter)
        {
            _execute (parameter, _hostInstance, _userData);
        }



        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}

