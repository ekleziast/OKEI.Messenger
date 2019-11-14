using MessengerWPF.View;
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

            List<GradientStop> gradients = GenerateRandomGradient();
            AvatarGradientBrush.GradientStops = new GradientStopCollection(gradients);
        }

        private static List<GradientStop> GenerateRandomGradient()
        {
            Random rnd = new Random();
            List<GradientStop> gradients = new List<GradientStop>();

            for(int i = 0; i < rnd.Next(3, 10); i++)
            {
                gradients.Add(new GradientStop { Color = Color.FromRgb(Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255)), Convert.ToByte(rnd.Next(0, 255))) });
            }

            return gradients;
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
                GenerateRandomGradient();
                Console.WriteLine(tb.Text);
            }
        }
    }
}
