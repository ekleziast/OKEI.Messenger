using ContextLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MessengerWPF
{
    public class MessengerClient
    {
        public static readonly int SERVERPORT = 8005;
        public static readonly string SERVERADDRESS = "127.0.0.1";
        public static readonly int LOCALPORT = 1800;

        private static MessengerClient _instant;

        public UdpClient Client;
        public Person Person;

        private MessengerClient() { }

        public static MessengerClient GetInstant(Person person)
        {
            if(_instant == null)
            {
                _instant = new MessengerClient();
            }
            
            _instant.Person = person;
            // FOR RELEASE
            // Client = new UdpClient(LOCALPORT);
            // FOR DEBUG
            _instant.Client = new UdpClient(new Random().Next(30000, 50000));

            return _instant;
        }

        public static MessengerClient GetInstant()
        {
            return _instant;
        }
        
        public static async void DisposeInstant()
        {
            await _instant.LogOut();
            _instant = null;
        }

        public async void SetStatus(Status status)
        {
            if (status.Name == "Невидимка")
            {
                status.Name = "Не в сети";
            }
            DefaultJSON jSON = new DefaultJSON { 
                Code = (int)Codes.NewStatus, 
                Content = await Task.Factory.StartNew(
                    () => JsonConvert.SerializeObject(new { Person = Person, Status = status })) 
            };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            try
            {
                SendMessageAsync(jsonString);
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
            }
        }

        /// <summary>
        /// Получение списка пользователей
        /// </summary>
        public async Task<List<Person>> GetPeople()
        {
            List<Person> people = new List<Person>();

            DefaultJSON jSON = new DefaultJSON { Code = (int)Codes.GetUsers, Content = "" };
            string jsonString = await Task.Factory.StartNew(()=> JsonConvert.SerializeObject(jSON));

            try
            {
                DefaultJSON response = await GetResponse(jsonString);
                people = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Person>>(response.Content));
            }catch(Exception ex)
            {
                ErrorAlert(ex.Message);
            }

            people.Remove(people.Where(o => o.ID == Person.ID).FirstOrDefault());

            return people;
        }

        /// <summary>
        /// Получение списка сообщений диалога
        /// </summary>
        /// <param name="conversation">Диалог</param>
        /// <returns>Список сообщений</returns>
        public async Task<List<Message>> GetMessages(Conversation conversation)
        {
            List<Message> messages = new List<Message>();

            DefaultJSON jSON = new DefaultJSON { 
                Code = (int)Codes.GetMessages, 
                Content = await Task.Factory.StartNew(
                    () => JsonConvert.SerializeObject(new { Person = Person, Conversation = conversation })) 
            };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            try
            {
                DefaultJSON response = await GetResponse(jsonString);
                messages = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Message>>(response.Content));
            }catch(Exception ex)
            {
                ErrorAlert(ex.Message);
            }

            return messages;
        }

        /// <summary>
        /// Получение списка диалогов
        /// </summary>
        /// <returns>Список диалогов</returns>
        public async Task<List<Conversation>> GetConversations()
        {
            var conversations = new List<Conversation>();

            DefaultJSON jSON = new DefaultJSON { 
                Code = (int)Codes.GetConversations, 
                Content = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Person))
            };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            DefaultJSON response = await GetResponse(jsonString);
            conversations = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<Conversation>>(response.Content));

            return conversations;
        }

        /// <summary>
        /// Получение ответа с сервера
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private async Task<DefaultJSON> GetResponse(string jsonString)
        {
            SendMessageAsync(jsonString);
            dynamic jsonResponse;
            try
            {
                UdpReceiveResult result = await Client.ReceiveAsync();
                byte[] data = result.Buffer;

                string response = Encoding.Unicode.GetString(data);
                jsonResponse = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(response));
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
                return new DefaultJSON { Code = (int) Codes.False, Content = ex.Message };
            }
            if ((int)(jsonResponse.Code) == (int) Codes.False)
            {
                ErrorAlert((string)jsonResponse.Content);
            }
            return new DefaultJSON { Code = (int)jsonResponse.Code, Content = (string)jsonResponse.Content };
        }

        /// <summary>
        /// Авторизация на сервере
        /// </summary>
        public async Task<bool> Authorize()
        {
            DefaultJSON jSON = new DefaultJSON { 
                Code = (int) Codes.Authorization, 
                Content = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Person)) };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            DefaultJSON response = await GetResponse(jsonString);
            if(response.Code == (int)Codes.True)
            {
                dynamic jsonResponse = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject(response.Content));
                Person = new Person {
                    ID = jsonResponse.ID,
                    Login = jsonResponse.Login,
                    Password = jsonResponse.Password,
                    Name = jsonResponse.Name,
                    SurName = jsonResponse.SurName
                };
                // TODO: Photo
            }

            return response.Code == (int)Codes.True;
        }

        /// <summary>
        /// Выход с сервера
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LogOut()
        {
            DefaultJSON jSON = new DefaultJSON { 
                Code = (int)Codes.LogOut, 
                Content = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Person))
            };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            DefaultJSON response = await GetResponse(jsonString);
            
            return response.Code == (int)Codes.True;
        }

        /// <summary>
        /// Регистрация на сервере
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Register()
        {
            DefaultJSON jSON = new DefaultJSON { 
                Code = (int) Codes.Registraion, 
                Content = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(Person))
            };
            string jsonString = await Task.Factory.StartNew(() => JsonConvert.SerializeObject(jSON));

            DefaultJSON response = await GetResponse(jsonString);
            return response.Code == (int)Codes.True;
        }

        /// <summary>
        /// Отправляет сообщение на сервер
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        private void SendMessageAsync(string message)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                Client.SendAsync(data, data.Length, SERVERADDRESS, SERVERPORT); // отправка
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
            }
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
                    UdpReceiveResult result = await Client.ReceiveAsync();
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
        private async void ProcessMessage(string response)
        {
            DefaultJSON json;
            try
            {
                json = await Task.Factory.StartNew(() =>  JsonConvert.DeserializeObject<DefaultJSON>(response));
            }catch
            {
                ErrorAlert("Не удалось распознать сообщение сервера");
                return;
            }
            switch (json.Code)
            {
                case (int)Codes.NewStatus:
                    break;
                default:
                    ErrorAlert("Не удалось распознать код сообщения сервера");
                    break;
            }
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
