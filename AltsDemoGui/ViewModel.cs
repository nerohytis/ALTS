using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AltsDemoGui
{
    class ViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// The status of the security
        /// </summary>
        public Status AltsStatus { get => _altsStatus; set {
                _altsStatus = value;
                NotifyPropertyChanged(nameof(StatusColor));
            }
        }

        /// <summary>
        /// The color to display depending on the current AltsStatus
        /// </summary>
        public Brush StatusColor
        {
            get
            {
                switch (AltsStatus)
                {
                    case Status.NotSet:
                        return new SolidColorBrush(Colors.LightGray);
                    case Status.Locked:
                        return new SolidColorBrush(Colors.Red);
                    case Status.Unlocked:
                        return new SolidColorBrush(Colors.Green);
                    case Status.Training:
                        return new SolidColorBrush(Colors.Yellow);
                    default:
                        return new SolidColorBrush(Colors.LightGray);
                }
            }
        }

        /// <summary>
        /// Logger text to display to the user
        /// </summary>
        public string LogText
        {
            get
            {
                return _msg.ToString();
            }
        }

        private StringBuilder _msg = new StringBuilder();
        private Status _altsStatus;

        public void Log(string msg)
        {
            _msg.AppendLine(msg);
            NotifyPropertyChanged(nameof(LogText));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Availiable ALTS mechanism status. Keeps user interface consistent with the overall system status
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Mechanism is not initialized
        /// </summary>
        NotSet,
        /// <summary>
        /// Mechanism is locked
        /// </summary>
        Locked,
        /// <summary>
        /// Mechanism is unlocked
        /// </summary>
        Unlocked,
        /// <summary>
        /// Training in progress
        /// </summary>
        Training
    }
}
