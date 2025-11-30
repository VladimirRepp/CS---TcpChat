using System.Net;
using System.Net.Sockets;

namespace TcpClientApp
{
    /// <summary>
    /// Сущность сервера отправителя сообщений 
    /// </summary>
    internal class Messager
    {
        private static string _userName;
        private static TcpClient _client;

        private static string _host;
        private static int _port;
        private static IPEndPoint? _serverEndPoint = null;

        private static StreamReader? _reader = null;
        private static StreamWriter? _writer = null;

        private static Thread _updateReceiveThread;
        private static Thread _updateSendMessageThread;

        public string UserName
        {
            get => _userName; 
            set => _userName = value;
        }

        public Thread UpdateReceiveThread => _updateReceiveThread;
        public Thread UpdateSendMessageThread => _updateSendMessageThread;

        public Messager(string userName, string host, int port)
        {
            Console.WriteLine(_userName);

            _userName = userName;
            _host = host;
            _port = port;
        }

        public Messager(string userName, IPEndPoint serverEndPoint)
        {
            _userName = userName;
            _serverEndPoint = serverEndPoint;
        }

        /// <summary>
        /// Ассинхронный запуск 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Startup(Action<string> print)
        {
            if (_serverEndPoint == null)
                _serverEndPoint = new(IPAddress.Parse(_host), _port);

            _client = new();
            await _client.ConnectAsync(_serverEndPoint);
            Console.WriteLine("Подключение установлено!"); // todo: уйти от зависимости 

            _reader = new(_client.GetStream());
            _writer = new(_client.GetStream());

            if(_reader is null || _writer is null)
            {
                throw new Exception($"Messanger.Startup: поток чтения или записи пуст!");
            }

            _updateReceiveThread = new(() => UpdateReceiveMessageAsync(print));
            _updateSendMessageThread = new(UpdateSendMessageAsync);

            _updateReceiveThread.Start();
            _updateSendMessageThread.Start();

            _updateReceiveThread.Join();
            _updateSendMessageThread.Join();
        }

        /// <summary>
        /// Закрытие мессенджера
        /// </summary>
        public void Close()
        {
            _client.Close();
            _writer?.Close();
            _reader?.Close();
        }

        /// <summary>
        /// Цикличное получение сообщений - паттерн Update
        /// </summary>
        /// <param name="print"></param>
        private async void UpdateReceiveMessageAsync(Action<string> print)
        {
            while (true)
            {
                try
                {
                    string? message = await ReceiveMessageAsync();

                    if (string.IsNullOrEmpty(message))
                        continue;

                    print(message);
                }
                catch
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Цикличное отправление сообщений - паттерн Update
        /// </summary>
        private async void UpdateSendMessageAsync()
        {
            await SendMessageAsync(UserName);

            Console.WriteLine("Для отправки сообщения ведние сообщение и нажмите ВВОД...");

            while (true)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("Введите сообщение для отправки!");
                    continue;
                }

                await SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Отправка сообщений 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendMessageAsync(string message) 
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }

        /// <summary>
        /// Получение сообщений 
        /// </summary>
        /// <returns></returns>
        private async Task<string?> ReceiveMessageAsync()
        {
            string? message = await _reader.ReadLineAsync();
            return message;
        }
    }
}
