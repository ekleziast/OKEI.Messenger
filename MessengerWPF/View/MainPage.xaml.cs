using ContextLibrary;
using MessengerWPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace MessengerWPF.View
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        public static ChatPage ChatPage;
        public static ObservableCollection<Person> UsersList { get; set; }
        public static ObservableCollection<Conversation> Conversations { get; set; }

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
            Conversations = new ObservableCollection<Conversation>();
            (await Client.GetConversations()).ForEach(o => Conversations.Add(o));
        }
        
        private void ChatsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conversation selectedConversation = ((ListView)sender).SelectedItem as Conversation;
            if (selectedConversation != null)
            {
                ChatPage chatPage  = new ChatPage(NavigationFrame, selectedConversation);
                NavigationFrame.Navigate(chatPage);
            }
            ((ListView)sender).UnselectAll();
        }

        private async void UsersListView_Initialized(object sender, EventArgs e)
        {
            UsersList = new ObservableCollection<Person>();
            (await Client.GetPeople()).ForEach(o => UsersList.Add(o));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Person selectedPerson = ((ListView)sender).SelectedItem as Person;
            if(selectedPerson != null)
            {
                NewConversationWindow newConversationWindow = new NewConversationWindow();
                if(newConversationWindow.ShowDialog() == true)
                {
                    Conversation newConversation = new Conversation { ID = Guid.NewGuid(), Title = newConversationWindow.TitleTB.Text };
                    List<Member> members = new List<Member> {
                        new Member { ConversationID = newConversation.ID, PersonID = selectedPerson.ID },
                        new Member { ConversationID = newConversation.ID, PersonID = Client.Person.ID },
                    };
                    Client.NewConversation(newConversation, members);
                }
            }
            UsersListView.UnselectAll();
        }
    }
}
