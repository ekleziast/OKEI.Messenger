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
        private Conversation _conversation;
        public List<Message> Messages { get; set; }
        private Frame NavigationFrame;

        public ChatPage(Frame navigationFrame, Conversation conversation)
        {
            _conversation = conversation;
            InitializeComponent();
            TitleConversation.Text = conversation.Title;
            NavigationFrame = navigationFrame;
        }

        private async void ChatMessagesListView_Initialized(object sender, EventArgs e)
        {
            ChatMessagesListView.ItemsSource = await Client.GetMessages(_conversation);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationFrame.GoBack();
        }
    }
}
