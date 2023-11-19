using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.ViewModels
{
    public class DevicePriorityViewModel : INotifyPropertyChanged
    {
        private bool _separateCommsPriority;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool SeparateCommsPriority
        {
            get => _separateCommsPriority;
            set
            {
                _separateCommsPriority = value;
                OnPropertyChanged("SeparateCommsPriority");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
