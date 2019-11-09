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

namespace MessengerWPF
{
    public class MessengerClient
    {
        public static readonly int SERVERPORT = 8005;
        public static readonly string SERVERADDRESS = "127.0.0.1";
        public static readonly int LOCALPORT = 1800;

        public UdpClient Client;
        public Person Person;
        public MessengerClient(Person person)
        {
            this.Person = person;
            // FOR RELEASE
            // Client = new UdpClient(LOCALPORT);
            // FOR DEBUG
            Client = new UdpClient(new Random().Next(30000, 50000));
        }

        public bool Authorize()
        {
            return true;
        }

        public bool Register()
        {
            RegisterJSON jSON = new RegisterJSON { Person = Person };
            string jsonString = JsonConvert.SerializeObject(jSON);

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
                Console.WriteLine(ex.Message);
                return false;
            }
            return Convert.ToInt32(jsonResponse.Code) == 1;

        }
        /// <summary>
        /// Отправляет сообщение на сервер
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                Client.Send(data, data.Length, SERVERADDRESS, SERVERPORT); // отправка
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ReceiveMessage()
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
                Console.WriteLine(ex.Message);
            }
        }

        public class RegisterJSON
        {
            public int Code = 4;
            public Person Person { get; set; }
        }
    }
}
