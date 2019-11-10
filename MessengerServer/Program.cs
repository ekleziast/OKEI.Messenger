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

namespace MessengerServer
{
    class Program
    {
        public static readonly int PORT = 8005;

        static Dictionary<Person, IPEndPoint> Clients = new Dictionary<Person, IPEndPoint>(); // Список клиентов
        static void Main(string[] args)
        {
            Console.WriteLine("Сервер запущен. Ожидание подключения...");
            try
            {
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(PORT); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.Unicode.GetString(data);
                    Console.WriteLine(
                        $"({DateTime.Now.ToShortTimeString()}) {remoteIp.Address.ToString()}:{remoteIp.Port} - " +
                        $"{message}"); // Logging

                    // Parsing
                    dynamic jsonMessage = JsonConvert.DeserializeObject(message);
                    ProcessMessage(jsonMessage, remoteIp);

                    
                    // Широковещательная рассылка сообщения
                    BroadcastMessage(message, remoteIp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private static void ProcessMessage(dynamic json, IPEndPoint ip)
        {
            string errorMessage;
            bool result = false;
            Person p;

            switch (Convert.ToInt32(json.Code))
            {
                // Регистрация
                case 4:
                    p = new Person
                    {
                        ID = json.Person.ID,
                        Login = json.Person.Login,
                        Password = json.Person.Password,
                        Name = json.Person.Name,
                        SurName = json.Person.SurName
                    };
                    // p.Photo = new Photo { ID = p.ID, PhotoSource = json.Person.Photo.PhotoSource };
                    result = Register(p, out errorMessage);
                    if (result)
                    {
                        SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 1, Content = JsonConvert.SerializeObject(p) }), ip);
                        
                        // Добавление клиента в список подключенных
                        bool isClientInCollection = Clients.Any(o =>
                        o.Value.Address.ToString() == ip.Address.ToString() &&
                        o.Value.Port == ip.Port);
                        if (!isClientInCollection) Clients.Add(p, ip);
                    }
                    else
                    {
                        SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 0, Content = errorMessage }), ip);
                    }
                    break;
                // Авторизация
                case 5:
                    p = new Person
                    {
                        Login = json.Person.Login,
                        Password = json.Person.Password
                    };
                    result = Register(p, out errorMessage);
                    if (result)
                    {
                        SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 1, Content = JsonConvert.SerializeObject(p) }), ip);

                        // Добавление клиента в список подключенных
                        bool isClientInCollection = Clients.Any(o =>
                        o.Value.Address.ToString() == ip.Address.ToString() &&
                        o.Value.Port == ip.Port);
                        if (!isClientInCollection) Clients.Add(p, ip);
                    }
                    else
                    {
                        SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 0, Content = errorMessage }), ip);
                    }
                    break;
            }
        }

        private static bool Register(Person person, out string errorMessage)
        {
            try
            {
                using(Context db = new Context())
                {
                    if(db.Persons.Any(o => o.Login == person.Login))
                    {
                        errorMessage = "Пользователь с таким логином уже зарегистрирован в системе.";
                        return false;
                    }
                    db.Persons.Add(person);
                    db.SaveChanges();
                }
                errorMessage = "";
                return true;
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Отправляет сообщение всем подключенным клиентам
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="ip">IPEndPoint отправителя</param>
        private static void BroadcastMessage(string message, IPEndPoint ip)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений

            byte[] data = Encoding.Unicode.GetBytes(message);
            try
            {
                Clients.ToList().ForEach(o => {
                    if ($"{o.Value.Address.ToString()}:{o.Value.Port}" != $"{ip.Address.ToString()}:{ip.Port}")
                    {
                        sender.Send(data, data.Length, o.Value);
                    }
                });
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        private static void SendMessageToClient(string message, IPEndPoint ip)
        {
            UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений

            byte[] data = Encoding.Unicode.GetBytes(message);
            try
            {
                sender.Send(data, data.Length, ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }

        private class DefaultResponse
        {
            public int Code { get; set; }
            public string Content { get; set; }
        }
    }
}
