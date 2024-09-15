using System.Windows.Input;

namespace DigitalAudioExperiment.Infrastructure
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private Action _execute;
        private Func<bool?> _canExecute;

        public RelayCommand(Action execute, Func<bool?> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            //CanExecuteChanged += RaiseCanExecuteChanged;
        }

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? (bool)(parameter as bool?);

        public void Execute(object? parameter)
            => _execute.Invoke();
    }
}
