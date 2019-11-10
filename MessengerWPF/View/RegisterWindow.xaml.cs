using ContextLibrary;
using Leadtools.Codecs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private bool isPhotoSelected = false;
        private byte[] selectedPhoto;
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void GithubURI_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ramil2321/MessengerOKEI");
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files(*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

            if (fileDialog.ShowDialog() == true)
            {
                selectedPhoto = File.ReadAllBytes(fileDialog.FileName);
                SelectedPhoto.Source = ByteToImageConverter.ByteToImage(selectedPhoto);
                SelectedPhoto.Visibility = Visibility.Visible;
                isPhotoSelected = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckValues()) { 
                ErrorAlert();
                return;
            }
            Person person = new Person { 
                ID = Guid.NewGuid(),
                Login = LoginTB.Text, 
                Password = PasswordTB.Password, 
                Name = NameTB.Text, 
                SurName = SurNameTB.Text
            };

            //person.Photo = new Photo { ID = person.ID, PhotoSource = selectedPhoto };

            AuthWindow.Client = new MessengerClient(person);
            bool result = AuthWindow.Client.Register();
            if (result)
            {
                MessageBox.Show("Вы успешно зарегистрировались!\nИспользуйте свои данные для входа.", "Добро пожаловать!", MessageBoxButton.OK, MessageBoxImage.None);
                this.Close();
            }
        }

        private void ErrorAlert()
        {
            MessageBox.Show("Пожалуйста, заполните все поля и загрузите фотографию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void ErrorAlert(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool CheckValues()
        {
            if (String.IsNullOrWhiteSpace(LoginTB.Text)) { return false; }
            if (String.IsNullOrWhiteSpace(PasswordTB.Password)) { return false; }
            if (String.IsNullOrWhiteSpace(NameTB.Text)) { return false; }
            if (String.IsNullOrWhiteSpace(SurNameTB.Text)) { return false; }
            if (!isPhotoSelected) { return false; }
            return true;
        }
    }
}
