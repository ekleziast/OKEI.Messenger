using ContextLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

namespace MessengerWPF.View
{
    /// <summary>
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        private MessengerClient Client = MessengerClient.GetInstant();
        public Conversation Conversation;
        public ObservableCollection<Message> Messages { get; set; }
        private Frame NavigationFrame;

        public ChatPage(Frame navigationFrame, Conversation conversation)
        {
            Conversation = conversation;
            NavigationFrame = navigationFrame;
            InitializeComponent();
            TitleConversation.Text = conversation.Title;

            ChatMessagesListView.ItemsSource = new ObservableCollection<Message>();
            ((INotifyCollectionChanged)ChatMessagesListView.ItemsSource).CollectionChanged += (s, e) =>
            {
                if (e.Action ==
                    System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    ChatMessagesListView.ScrollIntoView(ChatMessagesListView.Items[ChatMessagesListView.Items.Count - 1]);
                }
            };
        }

        private void ChatMessagesListView_Initialized(object sender, EventArgs e)
        {
            Messages = new ObservableCollection<Message>();
            Client.GetMessages(Conversation);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationFrame.GoBack();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendProcess();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                SendProcess();
            }else if(e.Key == Key.Escape)
            {
                NavigationFrame.GoBack();
            }
        }

        public void ScrollToEnd()
        {
            ChatMessagesListView.ScrollIntoView(ChatMessagesListView.Items[ChatMessagesListView.Items.Count - 1]);
        }

        private void SendProcess()
        {
            if (!String.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                Message message = new Message
                {
                    ID = Guid.NewGuid(),
                    ConversationID = Conversation.ID,
                    PersonID = Client.Person.ID,
                    DateTime = DateTime.Now,
                    Text = MessageTextBox.Text
                };
                Client.NewMessage(message);
                MessageTextBox.Clear();
            }
        }
    }
}
