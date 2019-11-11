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

        /// <summary>
        /// Метод обработки входящих сообщений
        /// </summary>
        /// <param name="json">JSON входящего сообщения</param>
        /// <param name="ip">IP адрес отправителя сообщения</param>
        private static void ProcessMessage(dynamic json, IPEndPoint ip)
        {
            int _code;
            bool isValidQuery = Int32.TryParse(json.Code, out _code);
            if (!isValidQuery)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 0, Content = "Неверный код запроса." }), ip);
                return;
            }

            switch (_code)
            {
                // Регистрация
                case 4:
                    RegisterProcess(json, ip);
                    break;

                // Авторизация
                case 5:
                    AuthProcess(json, ip);
                    break;

                // Новое сообщение
                case 6:
                    NewMessageProcess(json, ip);
                    break;

                // Новый диалог
                case 7:
                    break;
            }
        }
        /// <summary>
        /// Обработчик нового сообщения в чате
        /// </summary>
        private static void NewMessageProcess(dynamic json, IPEndPoint ip)
        {

        }

        /// <summary>
        /// Обработчик JSON регистрации
        /// </summary>
        private static void RegisterProcess(dynamic json, IPEndPoint ip)
        {
            string errorMessage;
            bool result;

            Person p = new Person
            {
                ID = json.Person.ID,
                Login = json.Person.Login,
                Password = json.Person.Password,
                Name = json.Person.Name,
                SurName = json.Person.SurName
            };
            // TODO: Добавление фотографии
            // p.Photo = new Photo { ID = p.ID, PhotoSource = json.Person.Photo.PhotoSource };
            result = Register(p, out errorMessage);
            if (result)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 1, Content = JsonConvert.SerializeObject(p) }), ip);
                ConnectClient(p, ip);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 0, Content = errorMessage }), ip);
            }
        }

        /// <summary>
        /// Обработчик JSON авторизации
        /// </summary>
        private static void AuthProcess(dynamic json, IPEndPoint ip)
        {
            string errorMessage;
            bool result;

            Person p = new Person
            {
                Login = json.Person.Login,
                Password = json.Person.Password
            };
            result = Auth(p, out errorMessage);
            if (result)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 1, Content = JsonConvert.SerializeObject(p) }), ip);
                ConnectClient(p, ip);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultResponse { Code = 0, Content = errorMessage }), ip);
            }
        }

        /// <summary>
        /// Подключает пользователя к серверу
        /// </summary>
        /// <param name="person">Аккаунт клиента</param>
        /// <param name="ip">IP адрес клиента</param>
        private static void ConnectClient(Person person, IPEndPoint ip)
        {
            if (!IsPersonLogged(person)) { Clients.Add(person, ip); }
        }

        /// <summary>
        /// Проверяет, подключен ли пользователь к серверу
        /// </summary>
        /// <param name="person">Пользователь</param>
        private static bool IsPersonLogged(Person person)
        {
            foreach(var o in Clients.Keys)
            {
                if(o.ID == person.ID) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Авторизует пользователя в системе
        /// </summary>
        /// <param name="person">Аккаунт пользователя</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <returns>Результат прохождения авторизации</returns>
        private static bool Auth(Person person, out string errorMessage)
        {
            try
            {
                using (Context db = new Context())
                {
                    Person p = db.Persons.Where(o => o.Login == person.Login && o.Password == person.Password).FirstOrDefault();
                    if (p == null)
                    {
                        errorMessage = "Пользователь с этой парой логин-пароль не найден в системе.";
                        return false;
                    }
                }
                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Регистрирует пользователя в системе
        /// </summary>
        /// <param name="person">Аккаунт пользователя</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <returns>Результат прохождения регистрации</returns>
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

        /// <summary>
        /// Отправляет сообщение конкретному клиенту
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="ip">Адрес клиента-получателя</param>
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
