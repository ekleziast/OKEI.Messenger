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
        
        public static void DisposeInstant()
        {
            _instant.LogOut();
            _instant = null;
        }

        /// <summary>
        /// Получение ответа с сервера
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private DefaultJSON GetResponse(string jsonString)
        {
            SendMessage(jsonString);
            dynamic jsonResponse;
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                byte[] data = Client.Receive(ref remoteIp); // получаем данные

                string response = Encoding.Unicode.GetString(data);
                jsonResponse = JsonConvert.DeserializeObject(response);
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
        public bool Authorize()
        {
            DefaultJSON jSON = new DefaultJSON { Code = (int) Codes.Authorization, Content = JsonConvert.SerializeObject(Person) };
            string jsonString = JsonConvert.SerializeObject(jSON);

            DefaultJSON response = GetResponse(jsonString);
            if(response.Code == (int)Codes.True)
            {
                dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
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
        public bool LogOut()
        {
            DefaultJSON jSON = new DefaultJSON { Code = (int) Codes.LogOut, Content = "" };
            string jsonString = JsonConvert.SerializeObject(jSON);

            DefaultJSON response = GetResponse(jsonString);

            if(response.Code == (int)Codes.False)
            {
                ErrorAlert(response.Content);
            }
            return response.Code == (int)Codes.True;
        }

        /// <summary>
        /// Регистрация на сервере
        /// </summary>
        /// <returns></returns>
        public bool Register()
        {
            DefaultJSON jSON = new DefaultJSON { Code = (int) Codes.Registraion, Content = JsonConvert.SerializeObject(Person) };
            string jsonString = JsonConvert.SerializeObject(jSON);

            DefaultJSON response = GetResponse(jsonString);
            return response.Code == (int)Codes.True;
        }

        /// <summary>
        /// Отправляет сообщение на сервер
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        private void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                Client.Send(data, data.Length, SERVERADDRESS, SERVERPORT); // отправка
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
            }
        }

        /// <summary>
        /// Прослушивание сообщений с сервера в бесконечном цикле
        /// </summary>
        private void ReceiveMessages()
        {
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = Client.Receive(ref remoteIp); // получаем данные
                    string response = Encoding.Unicode.GetString(data);
                }
            }
            catch (Exception ex)
            {
                ErrorAlert(ex.Message);
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
