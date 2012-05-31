using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ChemProV.Library.OSBLE
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = delegate { };
        private Func<object, bool> _canExecute;
        private Action<object> _executeAction;
        private bool _canExecuteCache;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            _executeAction = executeAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool temp = _canExecute(parameter);
            if (_canExecuteCache != temp)
            {
                _canExecuteCache = temp;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
            return _canExecuteCache;
        }

        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }
    }
}
