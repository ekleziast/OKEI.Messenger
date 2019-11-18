using ContextLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для NewMemberWindow.xaml
    /// </summary>
    public partial class NewMemberWindow : Window
    {
        public Person SelectedPerson;
        public ObservableCollection<Person> People { get; set; }
        public NewMemberWindow()
        {
            InitializeComponent();
            People = new ObservableCollection<Person>();
            if(MainPage.UsersList != null)
            {
                MainPage.UsersList.ToList().ForEach(o => {
                    bool res = false;
                    foreach(var p in MainPage.ChatPage.People)
                    {
                        if(p.ID == o.ID)
                        {
                            res = true;
                            break;
                        }
                    }
                    if (!res)
                    {
                        People.Add(o);
                    }
                });
            }

            PeopleListView.ItemsSource = People;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if((PeopleListView.SelectedItem as Person) == null)
            {
                MessageBox.Show("Вы не выбрали пользователя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                SelectedPerson = PeopleListView.SelectedItem as Person;
                this.DialogResult = true;
            }
        }
    }
}
