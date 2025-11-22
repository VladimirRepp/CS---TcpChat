using System.Net;
using System.Net.Sockets;
using TcpServer.Services;

namespace TcpServerApp
{
    internal class ServerObject
    {
        private TcpListener _tcpListener;
        private List<ClientObject> _clients;
        private ILogger _logger;

        protected internal ServerObject(IPEndPoint endPoint, ILogger logger)
        {
            //_tcpListener = new(IPAddress.Any, 8888);
            _tcpListener = new(endPoint);
            _logger = logger;
            
            _clients = new List<ClientObject>();
        }

        /// <summary>
        /// Прослущивание входящих подключений
        /// </summary>
        /// <returns></returns>
        protected internal async Task ListenAsync()
        {
            try
            {
                _tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений ...");

                while (true)
                {
                    TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    // TODO: авто определить тип и создать нужные 
                    ILogger logger = new ConsoleLogger();
                    
                    ClientObject clientObject = new(tcpClient, this, logger);
                    _clients.Add(clientObject);

                    await clientObject.StartupAsync();
                    await clientObject.UpdateAsync;
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"ServerObject.ListenAsync: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Удаление клиента по ID
        /// </summary>
        /// <param name="id"></param>
        protected internal void RemoveConnectionById(string id)
        {
            ClientObject? foundClient = _clients.FirstOrDefault(c => c.ID == id);

            if(foundClient != null)
                _clients.Remove(foundClient);

            foundClient?.Close();
        }

        /// <summary>
        /// Отключение всех подключенных клиентов
        /// </summary>
        protected internal void Disconnect()
        {
            foreach(var client in _clients)
            {
                client.Close();
            }

            _tcpListener.Stop();
        }

        /// <summary>
        /// Широковещательная отправка - трансляция сообщения подключеным клиентам, кроме отправителя.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="senderID"></param>
        /// <returns></returns>
        protected internal async Task BroadcastSendMessageAsync(string from, string message, string senderID)
        {
            foreach(var client in _clients)
            {
                if(client.ID != senderID)
                {
                    await client.Writer.WriteLineAsync($"{from}: " + message);
                    await client.Writer.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Одиночная отправка - трансляция сообщения указанному получателю.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="senderID"></param>
        /// <param name="recipientID"></param>
        /// <returns></returns>
        protected internal async Task SinglecastSendMessageAsync(string from, string message, string senderID, string recipientID)
        {
            foreach (var client in _clients)
            {
                if (client.ID == recipientID)
                {
                    await client.Writer.WriteLineAsync($"{from}: " + message);
                    await client.Writer.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Многоадресная рассылка - трансляция сообщения выбранным получателям.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="senderID"></param>
        /// <param name="recipientsID"></param>
        /// <returns></returns>
        protected internal async Task MulticastSendMessageAsync(string from, string message, string senderID, List<string> recipientsID)
        {
            foreach (var client in _clients)
            {
                if (recipientsID.FirstOrDefault(x => x == client.ID) != null)
                {
                    await client.Writer.WriteLineAsync($"{from}: " + message);
                    await client.Writer.FlushAsync();
                }
            }
        }
    }
}

