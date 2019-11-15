using ContextLibrary;
using MessengerWPF.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace MessengerWPF.View
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        double res = System.Windows.SystemParameters.PrimaryScreenWidth;
        private MessengerClient Client = MessengerClient.GetInstant();
        public string Nickname
        {
            get => Client.Person.Name + " " + Client.Person.SurName;
            set
            {
                OnPropertyChanged("Nickname");
            }
        }
        private List<Conversation> _conversations { get; set; }
        Frame NavigationFrame;

        public MainPage(Frame navigationFrame)
        {
            InitializeComponent();

            DataContext = new { ThisContext = this };

            AvatarGradientBrush.GradientStops = new GradientStopCollection(DesignUtil.GenerateRandomGradient());
            NavigationFrame = navigationFrame;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // search method
        }

        private void OnlineStatusCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBlock tb = ((ComboBox)sender).SelectedItem as TextBlock;
            if (tb != null)
            {
                AvatarGradientBrush.GradientStops = new GradientStopCollection(DesignUtil.GenerateRandomGradient());
                Client.SetStatus(new Status { Name = tb.Text });
            }
        }

        private async void ChatsListView_Initialized(object sender, EventArgs e)
        {
            ChatsListView.ItemsSource = await Client.GetConversations();
        }
        
        private void ChatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conversation selectedConversation = ((ListView)sender).SelectedItem as Conversation;
            if (selectedConversation != null)
            {
                ChatPage chatPage = new ChatPage(NavigationFrame, selectedConversation);
                NavigationFrame.Navigate(chatPage);
            }
            ((ListView)sender).UnselectAll();
        }

        private async void OnlineUsersListView_Initialized(object sender, EventArgs e)
        {
            OnlineUsersListView.ItemsSource = await Client.GetPeople();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
