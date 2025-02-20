using System.Net.Sockets;
using System.Reflection.PortableExecutable;

namespace TcpClientApp
{
    internal class Messager
    {
        private static string? _userName;
        private static TcpClient _client = new();

        private static string _host;
        private static int _port;

        private static StreamReader? _reader = null;
        private static StreamWriter? _writer = null;

        public string UserName
        {
            get => _userName;
            set => _userName = value;
        }

        public Messager(string userName, string host = "127.0.0.1", int port = 8888)
        {
            _userName = userName;
            _host = host;
            _port = port;

            _client.Connect(_host, _port);

            _reader = new StreamReader(_client.GetStream());
            _writer = new StreamWriter(_client.GetStream());

            if (_reader is null || _writer is null)
            {
                throw new Exception($"Messager.Messager: StreamReader or StreamWriter is NULL!");
            }
        }

        /// <summary>
        /// Асинхронная отправка сообщений 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }

        /// <summary>
        /// Асинхронное получение сообщений
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReceiveMessageAsync()
        {
            string? message = await _reader.ReadLineAsync();
            return message;
        }
    }
}
