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

            if (fileDialog.ShowDialog() != null)
            {
                byte[] img = File.ReadAllBytes(fileDialog.FileName);
                SelectedPhoto.Source = ByteToImageConverter.ByteToImage(img);
                SelectedPhoto.Visibility = Visibility.Visible;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
