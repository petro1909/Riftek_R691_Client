using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Riftek_R691_Client
{
    class Program
    {
        private static Dictionary<string, byte[]> commands = new Dictionary<string, byte[]>()
        {
            { "sensor_on", new byte[4]{ 0x2, 0x1, 0x13, 0x1 } },
            { "sensor_off", new byte[4]{ 0x2, 0x1, 0x6, 0x0 } },
            { "start_track", new byte[4]{ 0x2, 0x1, 0x6, 0x1 } },
            { "end_track", new byte[4]{ 0x2, 0x1, 0x6, 0x0 } },
            { "set_joint_id", new byte[4]{ 0x2, 0x1, 0x10, 0x0 } },
            { "request_joint_data", new byte[8]{ 0x1, 0x6, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD } },
            { "request_status", new byte[3]{ 0x1, 0x1, 0x6 } },
            { "request_joint_id", new byte[3]{ 0x1, 0x1, 0x10} },
            { "close socket", new byte[]{} }
        };

        private static IPAddress ScannerIp = IPAddress.Parse("127.0.0.1");
        private static int Port = 5020;

        private static byte[]? GetR691Command(int commandNumber)
        {
            switch (commandNumber)
            {
                case 1:
                    return commands.GetValueOrDefault("sensor_on");
                case 2:
                    return commands.GetValueOrDefault("sensor_off");
                case 3:
                    return commands.GetValueOrDefault("start_track");
                case 4:
                    return commands.GetValueOrDefault("end_track");
                case 5:
                    return commands.GetValueOrDefault("set_joint_id");
                case 6:
                    return commands.GetValueOrDefault("request_joint_data");
                case 7:
                    return commands.GetValueOrDefault("request_status");
                case 8:
                    return commands.GetValueOrDefault("request_joint_id");
                default:
                    return new byte[] { };
            }
        }
        private static int GetCommandNumber()
        {
            Console.WriteLine("Ввведите номер команды от робота: ");
            string? inputCommand = Console.ReadLine();
            int commandNumber = 0;
            bool parsingResult = int.TryParse(inputCommand, out commandNumber);

            if (!parsingResult || commandNumber < 1 || commandNumber > 9)
            {
                throw new Exception("Wrong command number");
            }
            return commandNumber;
        }
        public static void Main(string[] args)
        {
            //адреса для запуска сокета
            IPEndPoint scannerPoint = new IPEndPoint(ScannerIp, Port);
            Socket scannerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // подключение к серверу
                scannerSocket.Connect(scannerPoint);

                Console.WriteLine("Робот подключен к сканеру");
                Console.WriteLine("Список команд:");
                for (int i = 0; i < commands.Count; i++)
                {
                    Console.WriteLine($"{i + 1} - {commands.ElementAt(i).Key}");
                }

                while (true)
                {

                    int commandNumber = GetCommandNumber();
                    if (commandNumber == 9)
                    {
                        Console.WriteLine("Разрыв соединения");
                        scannerSocket.Shutdown(SocketShutdown.Both);
                        scannerSocket.Close();
                        return;
                    }
                    byte[] commandMessage = GetR691Command(commandNumber);


                    //Отправляем данные на сканер
                    scannerSocket.Send(commandMessage);
                    Console.Write("Робот отправил на сканер: ");
                    for (int i = 0; i < commandMessage.Length; i++)
                    {
                        Console.Write($"{commandMessage[i]} ");
                    }
                    Console.WriteLine();

                    //Получаем данные со сканера
                    byte[] scannerResponse = new byte[254];
                    int scanerResponseLength = scannerSocket.Receive(scannerResponse);
                    Console.Write("Робот получил от сканера: ");
                    for (int i = 0; i < scanerResponseLength; i++)
                    {
                        Console.Write($"{scannerResponse[i]} ");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}


