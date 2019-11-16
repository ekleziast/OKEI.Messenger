using ContextLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            Title_Console("ОКЭИ Сервер");
            /*
            using (Context db = new Context())
            {
                Guid personId = db.Persons.Where(o => o.Login == "admin").FirstOrDefault().ID;
                Message message = new Message {
                    PersonID = personId,
                    ConversationID = db.Conversations.Where(o => o.ID == db.Members.Where(k => k.PersonID == personId).FirstOrDefault().ConversationID).FirstOrDefault().ID,
                    DateTime = DateTime.Now, Text = "Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!Hello, World!"
                };
                db.Messages.Add(message);
                db.SaveChanges();
            }
            */
            Message_Console("Сервер запущен. Ожидание подключения...");
            try
            {
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
            } catch(Exception ex)
            {
                Error_Message(ex.Message);
            }
        }
        /// <summary>
        /// Слушатель входящих сообщений
        /// </summary>
        public static void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(PORT); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.Unicode.GetString(data);

                    // Logging
                    Message_Console(
                        $"({DateTime.Now.ToShortTimeString()}) {remoteIp.Address.ToString()}:{remoteIp.Port}");
                    Console.WriteLine(message + "\n");

                    // Processing
                    ProcessMessage(message, remoteIp);
                }
            }
            catch (Exception ex)
            {
                Error_Message(ex.Message);
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
        private static void ProcessMessage(string json, IPEndPoint ip)
        {
            DefaultJSON request;
            try
            {
                request = JsonConvert.DeserializeObject<DefaultJSON>(json);
            }
            catch (Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = "Не удалось распознать запрос.\nПодробная информация: " + ex.Message  }), ip);
                return;
            }

            switch (request.Code)
            {
                case (int) Codes.Registraion:
                    RegisterProcess(request.Content, ip);
                    break;
                    
                case (int) Codes.Authorization:
                    AuthProcess(request.Content, ip);
                    break;
                    
                case (int) Codes.NewMessage:
                    NewMessageProcess(request.Content, ip);
                    break;
                    
                case (int) Codes.LogOut:
                    LogOutProcess(request.Content, ip);
                    break;
                case (int) Codes.GetConversations:
                    GetConversationsProcess(request.Content, ip);
                    break;
                case (int) Codes.GetMessages:
                    GetMessagesProcess(request.Content, ip);
                    break;
                case (int)Codes.GetUsers:
                    GetUsersProcess(ip);
                    break;
                case (int)Codes.NewStatus:
                    SetStatusProcess(request.Content, ip);
                    break;
                case (int)Codes.NewConversation:
                    NewConversationProcess(request.Content, ip);
                    break;
                default:
                    SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = "Код ("+ request.Code +") не найден." }), ip);
                    break;
            }
        }

        /// <summary>
        /// Обработчик нового диалога
        /// Return: Conversation
        /// </summary>
        /// <param name="json">Content: Conversation, Members (list -> member)</param>
        private static void NewConversationProcess(string json, IPEndPoint ip)
        {
            string errorMessage;
            dynamic content = JsonConvert.DeserializeObject(json);
            Conversation conversation = JsonConvert.DeserializeObject<Conversation>(JsonConvert.SerializeObject(content.Conversation));
            List<Member> members = JsonConvert.DeserializeObject<List<Member>>(JsonConvert.SerializeObject(content.Members));
            List<Person> receivers = new List<Person>();
            members.ForEach(o => receivers.Add(new Person { ID = o.PersonID }));

            NewConversation(conversation, members);
            BroadcastMessage(
                new DefaultJSON
                {
                    Code = (int)Codes.NewConversation,
                    Content = JsonConvert.SerializeObject(conversation)
                }, receivers, out errorMessage
                );
        }
        
        private static void NewConversation(Conversation conversation, List<Member> members)
        {
            using(Context db = new Context())
            {
                db.Conversations.Add(conversation);
                db.Members.AddRange(members);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Обработчик нового статуса
        /// Return: Code: NewStatus, Content: Person
        /// </summary>
        /// <param name="json">Content: Person, Status</param>
        private static void SetStatusProcess(string json, IPEndPoint ip)
        {
            Person person;
            Status status;
            string errorMessage;

            try
            {
                dynamic content = JsonConvert.DeserializeObject(json);
                person = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(content.Person));
                status = JsonConvert.DeserializeObject<Status>(JsonConvert.SerializeObject(content.Status));
            }
            catch(Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = ex.Message }), ip);
                return;
            }
            try
            {
                SetStatus(person, status, ip);
            }
            catch(Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = ex.Message }), ip);
                return;
            }
            person.Status = status;
            BroadcastMessage(
                new DefaultJSON { 
                    Code = (int)Codes.NewStatus, 
                    Content = JsonConvert.SerializeObject(person) 
                }, 
                    person, Clients.Keys.ToList(), out errorMessage);
        }
        
        /// <summary>
        /// Устанавливает новый статус для пользователя
        /// </summary>
        /// <param name="person"></param>
        /// <param name="status"></param>
        private static void SetStatus(Person person, Status status, IPEndPoint ip)
        {
            using (Context db = new Context())
            {
                db.Persons.Where(o => o.ID == person.ID).FirstOrDefault().StatusID = db.Statuses.Where(o => o.Name == status.Name).FirstOrDefault().ID;
                db.SaveChanges();
            }
            UpdateInfo(person, ip);
        }

        /// <summary>
        /// Обработчик запроса на список пользователей
        /// </summary>
        private static void GetUsersProcess(IPEndPoint ip)
        {
            try
            {
                List<Person> people = GetPeople();
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.True, Content = JsonConvert.SerializeObject(people) }), ip);
            }
            catch(Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = ex.Message }), ip);
            }
        }
        /// <summary>
        /// Получение списка пользователей без логина и пароля
        /// </summary>
        private static List<Person> GetPeople()
        {
            List<Person> people = new List<Person>();
            using(Context db = new Context())
            {
                people = db.Persons.Include("Status").ToList();
            }
            people.ForEach(o => { o.Login = null; o.Password = null; o.Photo = null; });
            return people;
        }

        /// <summary>
        /// Обработчик получения списка сообщений
        /// </summary>
        /// <param name="json">Content: Person, Conversation</param>
        private static void GetMessagesProcess(string json, IPEndPoint ip)
        {
            Person person;
            Conversation conversation;
            string errorMessage;

            try
            {
                dynamic content = JsonConvert.DeserializeObject(json);
                person = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(content.Person));
                conversation = JsonConvert.DeserializeObject<Conversation>(JsonConvert.SerializeObject(content.Conversation));
            }
            catch
            {
                errorMessage = "Убедитесь, что все поля заполнены верно.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            if (!IsAuthorized(person, ip))
            {
                errorMessage = "Вы не авторизованы.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            List<Message> messages;
            try
            {
                messages = GetMessages(person, conversation);
            }
            catch (Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(
                    new DefaultJSON
                    {
                        Code = (int)Codes.False,
                        Content = JsonConvert.SerializeObject(ex.Message)
                    }), ip);
                return;
            }

            SendMessageToClient(JsonConvert.SerializeObject(
                new DefaultJSON
                {
                    Code = (int)Codes.True,
                    Content = JsonConvert.SerializeObject(messages)
                }), ip);
        }

        /// <summary>
        /// Метод для получения списка сообщений диалога
        /// </summary>
        private static List<Message> GetMessages(Person person, Conversation conversation)
        {
            List<Message> messages = new List<Message>();

            using (Context db = new Context())
            {
                messages = db.Messages.Include("Person").Where(o => o.ConversationID == conversation.ID).ToList();
                messages.ForEach(o => Console.WriteLine(o.ConversationID + " " + o.PersonID + ": " + " " + o.Text));
            }

            return messages;
        }

        /// <summary>
        /// Обработчик получения списка диалогов
        /// </summary>
        /// <param name="json">Content: Person</param>
        private static void GetConversationsProcess(string json, IPEndPoint ip)
        {
            Person p;
            string errorMessage;

            try
            {
                p = JsonConvert.DeserializeObject<Person>(json);
            }
            catch
            {
                errorMessage = "Не удалось создать пользователя. Убедитесь, что все поля заполнены верно.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            if(!IsAuthorized(p, ip))
            {
                errorMessage = "Вы не авторизованы.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            List<Conversation> conversations;
            try
            {
                conversations = GetConversations(p);
            }catch(Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(
                new DefaultJSON
                {
                    Code = (int)Codes.False,
                    Content = ex.Message
                }), ip);
                return;
            }
            SendMessageToClient(JsonConvert.SerializeObject(
                new DefaultJSON
                {
                    Code = (int)Codes.True,
                    Content = JsonConvert.SerializeObject(conversations)
                }), ip);
            
        }

        /// <summary>
        /// Метод для получения списка диалогов пользователя
        /// </summary>
        /// <param name="person">Пользователь</param>
        /// <returns></returns>
        private static List<Conversation> GetConversations(Person person)
        {
            List<Conversation> conversations = new List<Conversation>();
            using(Context db = new Context())
            {
                var personMember = db.Members.Where(o => o.PersonID == person.ID).ToList();
                db.Conversations.ToList().ForEach(o => {
                    personMember.ForEach(member =>
                    {
                        if(o.ID == member.ConversationID)
                        {
                            o.PhotoSource = null;
                            conversations.Add(o);
                            return;
                        }
                    });
                });
            }

            return conversations;
        }

        /// <summary>
        /// Обработчик выхода пользователя с системы 
        /// </summary>
        /// <param name="json">Content: Person</param>
        private static void LogOutProcess(string json, IPEndPoint ip)
        {
            Person sender;
            try
            {
                sender = JsonConvert.DeserializeObject<Person>(json);
            }catch(Exception ex)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = ex.Message }), ip);
                return;
            }

            Person person = Clients.ToList().Where(o => o.Value.Equals(ip) && o.Key.ID.Equals(sender.ID)).FirstOrDefault().Key;
            if (person != null)
            {
                string errorMessage;
                SetStatus(person, new Status { Name = "Не в сети" }, ip);
                Clients.Remove(person);
                person.Status = new Status { Name = "Не в сети" };
                BroadcastMessage(
                    new DefaultJSON
                    {
                        Code = (int)Codes.NewStatus,
                        Content = JsonConvert.SerializeObject(person)
                    },
                        person, Clients.Keys.ToList(), out errorMessage);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = "Вы не были авторизованы." }), ip);
            }
        }

        /// <summary>
        /// Обновляет информацию о подключенном клиенте
        /// </summary>
        private static void UpdateInfo(Person person, IPEndPoint ip)
        {
            Clients.Remove(Clients.Where(o => o.Key.ID == person.ID).FirstOrDefault().Key);
            Clients.Add(person, ip);
        }

        /// <summary>
        /// Обработчик нового сообщения в чате
        /// </summary>
        /// <param name="json">Content: Person, Message</param>
        private static void NewMessageProcess(string json, IPEndPoint ip)
        {
            string errorMessage;
            Message message;
            Person person;

            try
            {
                dynamic content = JsonConvert.DeserializeObject(json);
                person = JsonConvert.DeserializeObject<Message>(JsonConvert.SerializeObject(content.Person));
                message = JsonConvert.DeserializeObject<Message>(JsonConvert.SerializeObject(content.Message));
            } catch
            {
                errorMessage = "Не удалось распознать запрос.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            bool result = NewMessage(person, message, out errorMessage);
            if (result)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.True, Content = "Сообщение успешно отправлено." }), ip);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
            }
        }

        /// <summary>
        /// Отправляет сообщение (код 6) на сервер и оповещает об этом получателей
        /// </summary>
        private static bool NewMessage(Person person, Message message, out string errorMessage)
        {
            errorMessage = "";
            List<Person> _receivers = new List<Person>();
            try
            {
                using (Context db = new Context())
                {
                    db.Messages.Add(message);
                    db.SaveChanges();

                    db.Members.Include("Person").Where(o => o.PersonID != person.ID).ToList().ForEach(o => _receivers.Add(o.Person));
                }
            }catch(Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            bool result = BroadcastMessage(
                new DefaultJSON {
                    Code = 6,
                    Content = JsonConvert.SerializeObject(message)
                }, person, _receivers, out errorMessage);

            return result;
        }

        /// <summary>
        /// Отправляет сообщение всем получателям, кроме отправителя
        /// </summary>
        private static bool BroadcastMessage(DefaultJSON response, Person sender, List<Person> receivers, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                receivers.ForEach(receiver =>
                {
                    Clients.ToList().ForEach(client =>
                    {
                        if (receiver.ID == client.Key.ID)
                        {
                            if (receiver.ID != sender.ID)
                            {
                                SendMessageToClient(JsonConvert.SerializeObject(response), client.Value);
                            }
                        }
                    });
                });
            }catch(Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Отправляет сообщение всем получателям
        /// </summary>
        private static bool BroadcastMessage(DefaultJSON response, List<Person> receivers, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                receivers.ForEach(receiver =>
                {
                    Clients.ToList().ForEach(client =>
                    {
                        if (receiver.ID == client.Key.ID)
                        {
                            SendMessageToClient(JsonConvert.SerializeObject(response), client.Value);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Обработчик JSON регистрации
        /// </summary>
        /// <param name="json">Content: Person</param>
        private static void RegisterProcess(string json, IPEndPoint ip)
        {
            string errorMessage;
            bool result;
            Person p;

            try
            {
                p = JsonConvert.DeserializeObject<Person>(json);
            }catch
            {
                errorMessage = "Не удалось создать пользователя. Убедитесь, что все поля заполнены верно.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            // TODO: Добавление фотографии
            // p.Photo = new Photo { ID = p.ID, PhotoSource = json.Person.Photo.PhotoSource };
            result = Register(p, out errorMessage);
            if (result)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.True, Content = JsonConvert.SerializeObject(p) }), ip);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
            }
        }

        /// <summary>
        /// Обработчик JSON авторизации
        /// </summary>
        /// <param name="json">Content: Person</param>
        private static void AuthProcess(string json, IPEndPoint ip)
        {
            string errorMessage;
            bool result;
            Person person;

            try
            {
                person = JsonConvert.DeserializeObject<Person>(json);
            }
            catch
            {
                errorMessage = "Не удалось авторизоваться. Убедитесь, что все поля заполнены верно.";
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
                return;
            }

            Person validPerson;
            result = Auth(person, out validPerson, out errorMessage);
            if (result)
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.True, Content = JsonConvert.SerializeObject(validPerson) }), ip);
                ConnectClient(validPerson, ip);
            }
            else
            {
                SendMessageToClient(JsonConvert.SerializeObject(new DefaultJSON { Code = (int)Codes.False, Content = errorMessage }), ip);
            }
        }

        /// <summary>
        /// Подключает пользователя к серверу
        /// </summary>
        /// <param name="person">Аккаунт клиента</param>
        /// <param name="ip">IP адрес клиента</param>
        private static void ConnectClient(Person person, IPEndPoint ip)
        {
            string errorMessage;
            if (!IsAuthorized(person, ip)) { 
                Clients.Add(person, ip);

                SetStatus(person, new Status { Name = "В сети" }, ip);
                person.Status = new Status { Name = "В сети" };
                BroadcastMessage(
                    new DefaultJSON
                    {
                        Code = (int)Codes.NewStatus,
                        Content = JsonConvert.SerializeObject(person)
                    },
                        person, Clients.Keys.ToList(), out errorMessage);
            }
        }

        /// <summary>
        /// Проверяет, подключен ли пользователь к серверу
        /// </summary>
        /// <param name="person">Пользователь</param>
        private static bool IsAuthorized(Person person, IPEndPoint ip)
        {
            foreach(var o in Clients)
            {
                if(o.Key.ID == person.ID && o.Value.Equals(ip)) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Авторизует пользователя в системе
        /// </summary>
        /// <param name="person">Аккаунт пользователя</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
        /// <param name="p">Аккаунт, полученный с базы данных</param>
        /// <returns>Результат прохождения авторизации</returns>
        private static bool Auth(Person person, out Person p, out string errorMessage)
        {
            p = null;
            try
            {
                using (Context db = new Context())
                {
                    p = db.Persons.Where(o => o.Login == person.Login && o.Password == person.Password).FirstOrDefault();
                    if (p == null)
                    {
                        errorMessage = "Пользователь с этой парой логин-пароль не найден в системе.";
                        return false;
                    }
                    else
                    {
                        if (Clients.Where(o => o.Key.Login == person.Login).FirstOrDefault().Key != null)
                        {
                            errorMessage = "Пользователь уже авторизован в системе на другом устройстве.\nПожалуйста, выйдите с другого устройства и подождите.";
                            return false;
                        }
                    }
                }
                errorMessage = "";
                return true;
            }
            catch (Exception ex)
            {
                Error_Message(ex.Message);
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
                Error_Message(ex.Message);
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Отправляет сообщение всем подключенным клиентам, кроме отправителя
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
                Error_Message(ex.Message);
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
                Message_Console($"To {ip} : {message}");
            }
            catch (Exception ex)
            {
                Error_Message(ex.Message);
            }
            finally
            {
                sender.Close();
            }
        }
        

        /////////////////Краска, Титулка, Паузы////////////////////////////
        private static void Title_Console(string title)
        {
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = title;
        }

        private static void Pause()
        {
            Console.ReadKey(true);
        }


        private static void Message_Console(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Error_Message(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
        }
        ////////////////////////////////////////////////////////////////////
    }
}
