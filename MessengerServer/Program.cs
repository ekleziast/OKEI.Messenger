using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    class Program
    {
        public static readonly int PORT = 8005;

        static Socket listeningSocket; // Сокет
        static List<IPEndPoint> Clients = new List<IPEndPoint>(); // Список клиентов
        static void Main(string[] args)
        {
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Task listeningTask = new Task(Listen); // Создание потока для получения сообщений
                listeningTask.Start();
                listeningTask.Wait(); // Не идем дальше пока поток не будет остановлен
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        private static void Listen()
        {
            try
            {
                IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), PORT);
                listeningSocket.Bind(localIP);

                Console.WriteLine("Сервер запущен! Ожидаем подключения...");

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];
                    EndPoint remoteIP = new IPEndPoint(IPAddress.Any, 0); // адрес удаленного подключения

                    do
                    {
                        bytes = listeningSocket.ReceiveFrom(data, ref remoteIP);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (listeningSocket.Available > 0);

                    IPEndPoint remoteFullIP = remoteIP as IPEndPoint; // данные о подключении
                    Console.WriteLine(
                        $"({DateTime.Now.ToShortTimeString()}) {remoteFullIP.Address.ToString()}:{remoteFullIP.Port} - " +
                        $"{builder.ToString()}"); // Logging

                    bool isClientInCollection = Clients.Any(o => 
                        o.Address.ToString() == remoteFullIP.Address.ToString()
                        && o.Port == remoteFullIP.Port
                    );
                    if (!isClientInCollection) Clients.Add(remoteFullIP); // Добавляем нового клиента в список

                    // TODO: Оповещение клиентов о получении нового сообщения
                    // Сейчас происходит отправка всем клиентов
                    BroadcastMessage(builder.ToString(), remoteFullIP);

                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// Отправляет сообщение всем подключенным клиентам
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="ip">IPEndPoint отправителя</param>
        private static void BroadcastMessage(string message, IPEndPoint ip)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            Clients.ForEach(o => {
            if ($"{o.Address.ToString()}:{o.Port}" != $"{ip.Address.ToString()}:{ip.Port}")
                {
                    listeningSocket.SendTo(data, o);
                }
            });
        }

        /// <summary>
        /// Закрывает соединение
        /// </summary>
        private static void Close()
        {
            if(listeningSocket != null)
            {
                listeningSocket.Shutdown(SocketShutdown.Both);
                listeningSocket.Close();
                listeningSocket = null;
            }
            Console.WriteLine("Сервер остановлен!");
        }
    }
}
