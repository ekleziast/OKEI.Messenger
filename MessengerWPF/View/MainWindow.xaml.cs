﻿using MessengerWPF.View;
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
        public static Thread ReceiveThread;
        private MessengerClient Client = MessengerClient.GetInstant();
        public static MainPage MainPage;
        double res = System.Windows.SystemParameters.PrimaryScreenWidth;
        private AuthWindow parentWindow;
        
        public MainWindow(AuthWindow parentWindow)
        {
            this.MinWidth = res / 2;
            InitializeComponent();
            this.parentWindow = parentWindow;

            // TODO: Threads
            StartReceiveMessages();

            MainFrame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            MainPage = new MainPage(MainFrame);
            MainFrame.Navigate(MainPage);

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
            ReceiveThread = new Thread(new ThreadStart(ReceiveMessages));
            ReceiveThread.Start();
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
        public void ProcessMessage(string response)
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
                case (int)Codes.NewMessage:
                    NewMessageProcess(json.Content);
                    break;
                case (int)Codes.GetConversations:
                    GetConversationsProcess(json.Content);
                    break;
                case (int)Codes.GetMessages:
                    GetMessagesProcess(json.Content);
                    break;
                case (int)Codes.GetUsers:
                    GetUsersProcess(json.Content);
                    break;
                case (int)Codes.NewMember:
                    NewMemberProcess(json.Content);
                    break;
                case (int)Codes.GetMembers:
                    GetMembersProcess(json.Content);
                    break;
                case (int)Codes.Registraion:
                    RegistrationProcess(json.Content);
                    break;
                default:
                    //ErrorAlert("Не удалось распознать код сообщения сервера: " + json.Code);
                    break;
            }
        }
        private void RegistrationProcess(string json)
        {
            Person person = JsonConvert.DeserializeObject<Person>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                MainPage.UsersList.Add(person);
                MainPage.UsersListView.ItemsSource = null;
                MainPage.UsersListView.ItemsSource = MainPage.UsersList;
            }));
        }
        private void NewMemberProcess(string json)
        {
            Member member = JsonConvert.DeserializeObject<Member>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (MainPage.ChatPage != null)
                {
                    if(MainPage.ChatPage.Conversation.ID == member.ConversationID)
                    {
                        MainPage.ChatPage.People.Add(member.Person);
                    }
                }
            }));
        }
        private void GetMembersProcess(string json)
        {
            List<Member> members = JsonConvert.DeserializeObject<List<Member>>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (members.Count > 0)
                {
                    if (MainPage.ChatPage != null)
                    {
                        MainPage.ChatPage.People.Clear();
                        foreach(var m in members)
                        {
                            if(m.ConversationID == MainPage.ChatPage.Conversation.ID)
                            {
                                MainPage.ChatPage.People.Add(m.Person);
                            }
                        }
                        MainPage.ChatPage.MembersListView.ItemsSource = null;
                        MainPage.ChatPage.MembersListView.ItemsSource = MainPage.ChatPage.People;
                    }
                }
            }));
        }
        private void GetMessagesProcess(string json)
        {
            List<Message> messages = JsonConvert.DeserializeObject<List<Message>>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                messages.ForEach(o => {
                    if (MainPage.ChatPage != null)
                    {
                        MainPage.ChatPage.Messages.Add(o);
                        MainPage.ChatPage.ChatMessagesListView.ItemsSource = null;
                        MainPage.ChatPage.ChatMessagesListView.ItemsSource = MainPage.ChatPage.Messages;
                        MainPage.ChatPage.ScrollToEnd();
                    }
                });
            }));
        }
        private void GetUsersProcess(string json)
        {
            List<Person> people = JsonConvert.DeserializeObject<List<Person>>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (MainPage != null)
                {
                    people.ForEach(o =>
                    {
                        if (o.ID != Client.Person.ID)
                        {
                            MainPage.UsersList.Add(o);
                        }
                        MainPage.UsersListView.ItemsSource = null;
                        MainPage.UsersListView.ItemsSource = MainPage.UsersList;
                    });
                }
            }));
        }

        private void GetConversationsProcess(string json)
        {
            List<Conversation> conversations = JsonConvert.DeserializeObject<List<Conversation>>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                conversations.ForEach( o => {
                    if (MainPage.Conversations != null)
                    {
                        MainPage.Conversations.Add(o);
                        MainPage.FilteredConversations.Add(o);
                        MainPage.FilterConversations("");
                    }
                });
            }));
        }
        private void NewMessageProcess(string json)
        {
            Message message = JsonConvert.DeserializeObject<Message>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (MainPage.ChatPage != null)
                {
                    if (MainPage.ChatPage.Conversation.ID == message.ConversationID)
                    {
                        MainPage.ChatPage.Messages.Add(message);
                        MainPage.ChatPage.ChatMessagesListView.ItemsSource = null;
                        MainPage.ChatPage.ChatMessagesListView.ItemsSource = MainPage.ChatPage.Messages;
                        MainPage.ChatPage.ScrollToEnd();
                    }
                }
            }));
        }

        private void NewConversationProcess(string json)
        {
            Conversation conversation = JsonConvert.DeserializeObject<Conversation>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (MainPage.Conversations != null)
                {
                    MainPage.Conversations.Add(conversation);
                    MainPage.FilterConversations("");
                }
            }));
        }

        private void NewStatusProcess(string json)
        {
            Person person = JsonConvert.DeserializeObject<Person>(json);
            Dispatcher.BeginInvoke(new Action(() => {
                if (MainPage == null)
                {
                    MainPage = new MainPage(MainFrame);
                }
                if (MainPage.UsersList != null)
                {
                    foreach (var p in MainPage.UsersList)
                    {
                        if (p.ID == person.ID)
                        {
                            if (p.Status == null)
                            {
                                p.Status = new Status { Name = person.Status.Name };
                            }
                            else
                            {
                                p.Status.Name = person.Status.Name;
                            }
                            break;
                        }
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
