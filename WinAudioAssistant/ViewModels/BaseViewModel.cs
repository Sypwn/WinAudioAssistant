using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public Action CloseViewAction { get; set; } = () => { }; // Set in BaseView
        public Action FocusViewAction { get; set; } = () => { }; // Set in BaseView
        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand ApplyCommand { get; }

        public BaseViewModel()
        {
            OkCommand = new RelayCommand(OkAction);
            CancelCommand = new RelayCommand(CancelAction);
            ApplyCommand = new RelayCommand(ApplyAction);
        }

        public abstract bool Apply();
        public abstract bool Discard();
        public abstract bool ShouldClose();
        public abstract void Cleanup();

        private void OkAction(object? parameter)
        {
            if (Apply() && ShouldClose())
                CloseViewAction();
        }

        private void CancelAction(object? parameter)
        {
            if (Discard() && ShouldClose())
                CloseViewAction();
        }

        private void ApplyAction(object? parameter)
        {
            Apply();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
