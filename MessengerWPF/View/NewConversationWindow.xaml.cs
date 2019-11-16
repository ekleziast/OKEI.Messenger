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
    /// Логика взаимодействия для NewConversationWindow.xaml
    /// </summary>
    public partial class NewConversationWindow : Window
    {
        public NewConversationWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(TitleTB.Text))
            {
                MessageBox.Show("Укажите название диалога", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                this.DialogResult = true;
            }
        }
    }
}
