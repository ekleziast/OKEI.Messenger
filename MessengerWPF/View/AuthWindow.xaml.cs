using ContextLibrary;
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
using System.Windows.Shapes;

namespace MessengerWPF.View
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private MessengerClient client;

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void GithubURI_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ramil2321/MessengerOKEI");
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Person person = new Person { Login = LoginTB.Text, Password = PasswordTB.Password };
            client = MessengerClient.GetInstant(person);
            if (await client.Authorize())
            {
                this.Hide();
                MainWindow main = new MainWindow(this);
                main.Show();
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
        }

        public void ClearPassword()
        {
            this.PasswordTB.Clear();
        }
    }
}
