using MessengerWPF.View;
using System.Collections.Generic;
using System.Windows;
using ContextLibrary;

namespace MessengerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double res = System.Windows.SystemParameters.PrimaryScreenWidth;
        private List<Conversation> _conversations { get; set; }
        private AuthWindow parentWindow;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(AuthWindow parentWindow)
        {
            this.MinWidth = res / 2;
            InitializeComponent();
            this.parentWindow = parentWindow;
            this.Content = new MainPage();
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
    }
}
