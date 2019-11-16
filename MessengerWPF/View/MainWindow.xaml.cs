using MessengerWPF.View;
using System.Collections.Generic;
using System.Windows;
using ContextLibrary;
using System.Windows.Navigation;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System;
using Newtonsoft.Json;

namespace MessengerWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessengerClient Client = MessengerClient.GetInstant();
        public static MainPage MainPage;
        double res = System.Windows.SystemParameters.PrimaryScreenWidth;
        private AuthWindow parentWindow;
        
        public MainWindow(AuthWindow parentWindow)
        {
            this.MinWidth = res / 2;
            InitializeComponent();
            this.parentWindow = parentWindow;

            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainPage = new MainPage(MainFrame);
            MainFrame.Navigate(MainPage);

            StartReceiveMessages();
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



        /// <summary>
        /// Стартует прослушивание сообщений в отдельном потоке
        /// </summary>
        public void StartReceiveMessages()
        {
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessages));
            receiveThread.Start();
        }


        /// <summary>
        /// Прослушивание сообщений с сервера в бесконечном цикле и отправление их на обработку
        /// </summary>
        private async void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    UdpReceiveResult result = await Client.Client.ReceiveAsync();
                    byte[] data = result.Buffer; // получаем данные
                    string response = Encoding.Unicode.GetString(data);
                    ProcessMessage(response);
                }
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
            }
        }

        /// <summary>
        /// Обработчик входящих сообщений
        /// </summary>
        public async void ProcessMessage(string response)
        {
            DefaultJSON json;
            try
            {
                json = JsonConvert.DeserializeObject<DefaultJSON>(response);
            }
            catch
            {
                ErrorAlert("Не удалось распознать сообщение сервера");
                return;
            }
            switch (json.Code)
            {
                case (int)Codes.NewStatus:
                    NewStatusProcess(json.Content);
                    break;
                case (int)Codes.NewConversation:
                    NewConversationProcess(json.Content);
                    break;
                default:
                    //ErrorAlert("Не удалось распознать код сообщения сервера: " + json.Code);
                    break;
            }
        }

        private void NewConversationProcess(string json)
        {
            Console.WriteLine(json);
            Conversation conversation = JsonConvert.DeserializeObject<Conversation>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                MainPage.Conversations.Add(conversation);
                MainPage.FilterConversations("");
                //MainPage.ChatsListView.ItemsSource = null;
                //MainPage.ChatsListView.ItemsSource = MainPage.Conversations;
                //MainPage.ChatsListView.SelectedItem = conversation;
            }));
        }

        private void NewStatusProcess(string json)
        {
            Person person = JsonConvert.DeserializeObject<Person>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                foreach(var p in MainPage.UsersList)
                {
                    if(p.ID == person.ID)
                    {
                        p.Status.Name = person.Status.Name;
                        break;
                    }
                }
                MainPage.UsersListView.ItemsSource = null;
                MainPage.UsersListView.ItemsSource = MainPage.UsersList;
                }));
        }

        /// <summary>
        /// Уведомление об ошибке
        /// </summary>
        private void ErrorAlert(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
