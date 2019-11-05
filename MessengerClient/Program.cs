using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessengerClient
{
    class Program
    {
        public static readonly int SERVERPORT = 8005;
        public static readonly string SERVERADDRESS = "127.0.0.1";
        public static readonly int LOCALPORT = 1800;

        public static UdpClient Client;
        static void Main(string[] args)
        {
            try
            {
                // FOR RELEASE
                // Client = new UdpClient(LOCALPORT);
                // FOR DEBUG
                Client = new UdpClient(new Random().Next(30000, 50000));

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                SendMessage();
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
        private static void SendMessage()
        {
            try
            {
                while (true)
                {
                    string message = Console.ReadLine(); // сообщение для отправки
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    Client.Send(data, data.Length, SERVERADDRESS, SERVERPORT); // отправка
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // эта хуйня не работает короче
        private static void ReceiveMessage()
        {
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (true)
                {
                    byte[] data = Client.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.Unicode.GetString(data);
                    Console.WriteLine("Собеседник: {0}", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
