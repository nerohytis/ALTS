using InterfaceAltSecurity;
using SmartCardDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AltsDemoGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SmartCardMonitor scm;
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event called when connect button is pushed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Connect(object sender, RoutedEventArgs e)
        {
            var dm = this.DataContext as ViewModel;
            scm = new SmartCardMonitor();
            scm.Connect(okCallBack);
            dm.AltsStatus = Status.Locked;
            dm.Log("Connected");
        }

        /// <summary>
        /// Event called when disconnect button is pushed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_DisConnect(object sender, RoutedEventArgs e)
        {
            var dm = this.DataContext as ViewModel;
            scm.Disconnect();
            dm.AltsStatus = Status.NotSet;

        }

        /// <summary>
        /// Event called when TrustDevice button is pushed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Learn(object sender, RoutedEventArgs e)
        {
            var dm = this.DataContext as ViewModel;
            dm.AltsStatus = Status.Training;
            scm.TrustDevice(trustCallBack);

        }


        /// <summary>
        /// Callback called by the InterfaceAltSecurity interface implementation
        /// </summary>
        /// <param name="result">The authentication info provided to the callback</param>
        private async void okCallBack(AltsInfo result)
        {
            await this.Dispatcher.Invoke(async () =>
            {
                var dm = this.DataContext as ViewModel;
                dm.Log($"okCallBack");
                if (result.Authenticated)
                {
                    
                    dm.AltsStatus = Status.Unlocked;
                    dm.Log($"Unlock Success");

                    await Task.Delay(5000);
                    dm.Log($"Resetting lock");
                    dm.AltsStatus = Status.Locked;

                }
                else {
                    dm.Log($"Unlock Failed");
                }
            });

        }

        /// <summary>
        ///  Callback called by the InterfaceAltSecurity interface implementation
        /// </summary>
        /// <param name="result">The authentication info provided to the callback</param>
        private void trustCallBack(AltsInfo result)
        {
            this.Dispatcher.Invoke(() =>
            {
                var dm = this.DataContext as ViewModel;

                dm.Log($"Trust CallBack ");
                if (result.Authenticated)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        dm.AltsStatus = Status.Unlocked;
                        Task.Delay(1000);
                        dm.AltsStatus = Status.Locked;
                        Task.Delay(1000);
                    }
                }
                dm.Log($"Trust CallBack result => {result.Authenticated}");

                dm.AltsStatus = Status.Locked;
            });
            
        }
    }
}
