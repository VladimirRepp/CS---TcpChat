using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TcpServer.Services;

namespace TcpServer
{
    /// <summary>
    /// Сущность сервера  
    /// </summary>
    internal class ServerObject
    {
        private readonly TcpListener _tcpListener;
        private readonly List<ClientObject> _clients;
        private ILogger _logger;

        internal ServerObject(IPEndPoint endPoint, ILogger logger)
        {
            _tcpListener = new TcpListener(endPoint);
            _logger = logger;

            _clients = new();
        }

        /// <summary>
        /// Ассинхронный запуск сервера <br/>
        /// UpdateListenAsync - инкапсулирован 
        /// </summary>
        /// <returns></returns>
        internal async Task StartupAsync()
        {
            try
            {
                _tcpListener.Start();
                _logger.Log("Сервер запущен. Ожидание подключений ...");

                await UpdateListenAsync();
            }
            catch (Exception ex)
            {
                _logger.Log($"ServerObject.StartupAsync: {ex.Message}", ELogType.Error);
            }           
        }

        /// <summary>
        /// Закрытие всех клиентов и Tcp сокет сервера 
        /// </summary>
        internal void Disconnect()
        {
            foreach (var c in _clients)
            {
                c.Close();
            }

            _tcpListener.Stop();
        }

        /// <summary>
        /// Удалить клиента по Id
        /// </summary>
        /// <param name="id"></param>
        internal void RemoveConnectionById(string id)
        {
            var foundClient = _clients.FirstOrDefault(x => x.ID == id);

            if (foundClient != null)
            {
                _clients.Remove(foundClient);
                foundClient.Close();
            }
        }

        /// <summary>
        /// Широковещательная рассылка - всем, кроме отправителя 
        /// </summary>
        /// <param name="from_userName"></param>
        /// <param name="message"></param>
        /// <param name="sender_id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal async Task BroadcastSendMessageAsync(string? from_userName, string message, string sender_id)
        {
            foreach(var client in _clients)
            {
                if (client.ID != sender_id)
                    await SendMessage(client, from_userName, message);
            }
        }

        /// <summary>
        /// Рассылка подписчиткам\группе
        /// </summary>
        /// <param name="from_userName"></param>
        /// <param name="message"></param>
        /// <param name="sender_id"></param>
        /// <param name="recipiendsID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal async Task MulticastSendMessageAsync(string? from_userName, string message, string sender_id, List<string> recipiendsID)
        {
            foreach (var client in _clients)
            {
                foreach (var recipiendId in recipiendsID){
                    if (client.ID == recipiendId)
                    {
                        await SendMessage(client, from_userName, message);
                    }
                }              
            }
        }

        /// <summary>
        /// Рассылка одному
        /// </summary>
        /// <param name="from_userName"></param>
        /// <param name="message"></param>
        /// <param name="sender_id"></param>
        /// <param name="recipiendID"></param>
        /// <returns></returns>
        internal async Task SinglecastSendMessageAsync(string? from_userName, string message, string sender_id, string recipiendID)
        {
            foreach (var client in _clients)
            {
                if (client.ID == recipiendID)
                {
                    await SendMessage(client, from_userName, message);
                    break;
                }
            }
        }

        /// <summary>
        /// Цикличная прослушка клиентов (паттерн Update)
        /// </summary>
        /// <returns></returns>
        private async Task UpdateListenAsync()
        {
            try
            {
                while (true)
                {
                    TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();

                    // TODO: придумать способ как определить нужного логера
                    ILogger loggerClient = null;
                    if (_logger is ConsoleLogger)
                        loggerClient = new ConsoleLogger();

                    ClientObject clientObject = new(tcpClient, this, loggerClient);
                    _clients.Add(clientObject);

                    Task.Run(() => clientObject.StartupAsync()); // без ожидания 
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"ServerObject.UpdateListenAsync: {ex.Message}", ELogType.Error);
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Отправка сообщения 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="from_userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendMessage(ClientObject client, string? from_userName, string message)
        {
            await client.Writer.WriteLineAsync($"[{from_userName}]: " + message);
            await client.Writer.FlushAsync();
        }
    }
}
