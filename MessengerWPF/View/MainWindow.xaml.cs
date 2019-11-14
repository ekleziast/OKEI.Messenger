using MessengerWPF.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessengerWPF.Util;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessengerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AuthWindow parentWindow;
        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(AuthWindow parentWindow)
        {
            InitializeComponent();
            this.parentWindow = parentWindow;
            
            AvatarGradientBrush.GradientStops = new GradientStopCollection(DesignUtil.GenerateRandomGradient());
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // search method
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (parentWindow != null)
            {
                parentWindow.ClearPassword();
                parentWindow.Show();
            }
            MessengerClient.DisposeInstant();
        }

        private void OnlineStatusCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock tb = ((ComboBox)sender).SelectedItem as TextBlock;
            if(tb != null)
            {
                AvatarGradientBrush.GradientStops = new GradientStopCollection(DesignUtil.GenerateRandomGradient());
                Console.WriteLine(tb.Text);
            }
        }
    }
}
