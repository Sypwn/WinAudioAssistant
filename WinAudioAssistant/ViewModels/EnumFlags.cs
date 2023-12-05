using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WinAudioAssistant.ViewModels
{
    // https://stackoverflow.com/a/49559152
    public class EnumFlags<T> : INotifyPropertyChanged where T : Enum, IComparable, IFormattable, IConvertible
    {
        private T value;

        public EnumFlags(T t)
        {
            value = t;
        }

        public T Value
        {
            get { return value; }
            set
            {
                if (this.value.Equals(value)) return;
                this.value = value;
                OnPropertyChanged("Item[]");
            }
        }

        [IndexerName("Item")]
        public bool this[T key]
        {
            get
            {
                // .net *does* now allow us to specify that T is an enum, but we still can't cast T to int because of variable...size...something
                // to get around this, cast it to object then cast that to int.
                return ((int)(object)value & (int)(object)key) == (int)(object)key;
            }
            set
            {
                if (((int)(object)this.value & (int)(object)key) == (int)(object)key == value) return;

                this.value = (T)(object)((int)(object)this.value ^ (int)(object)key);

                OnPropertyChanged("Item[]");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string memberName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
        }
        #endregion
    }
}
