using System.Net.Sockets;
using TcpServer.Services;

namespace TcpServer
{
    /// <summary>
    /// Объект клиента на стороне сервера
    /// </summary>
    internal class ClientObject
    {
        protected internal string ID { get; } = Guid.NewGuid().ToString();
        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }

        private string? _userName;

        private readonly TcpClient _client;
        private readonly ServerObject _server;
        private readonly ILogger _logger;

        public ClientObject(TcpClient client, ServerObject server, ILogger logger)
        {
            _client = client;
            _server = server;
            _logger = logger;

            var stream = _client.GetStream();
            Writer = new(stream);
            Reader = new(stream);
        }

        /// <summary>
        /// Ассинхронный запуск клиента <br/>
        /// UpdateAsync - инкапсулирован
        /// </summary>
        /// <returns></returns>
        public async Task StartupAsync()
        {
            try
            {
                _userName = await Reader.ReadLineAsync();
                string message = $"{_userName} - вошел в чат!";

                await SendMessageAsync(_userName, message);

                await UpdateReceivedAsync();
            }
            catch (Exception ex)
            {
                _logger.Log("ClientObject.StartupAsync: " + ex.Message, ELogType.Error);
            }
        }
        
        /// <summary>
        /// Закрытие клиента
        /// </summary>
        public void Close()
        {
            Reader.Close();
            Writer.Close();
            _client.Close();
        }

        /// <summary>
        /// Цикличная просушка входящих сообщени (паттерн Update)
        /// </summary>
        /// <returns></returns>
        private async Task UpdateReceivedAsync()
        {
            try
            {
                string? message;

                // В данном бесконечном цикле 
                // обрабатываем сообщения от клиента 
                while (true)
                {
                    try
                    {
                        message = await Reader.ReadLineAsync();

                        if (message == null)
                            continue;

                        await SendMessageAsync(_userName, message);
                    }
                    catch
                    {
                        message = $"{_userName} - покинул чат!";
                        _server.RemoveConnectionById(ID);
                        await SendMessageAsync(_userName, message);
                        break;
                    }                   
                }
            }
            catch(Exception ex)
            {
                _logger.Log("ClientObject.UpdateAsync: " + ex.Message, ELogType.Error);
            }
            finally
            {
                _server.RemoveConnectionById(ID);
            }
        }

        /// <summary>
        /// Широковищательная отправка: broadcast
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SendMessageAsync(string? from, string message)
        {
            await _server.BroadcastSendMessageAsync(from, message, ID);
            _logger.Log($"{from}: {message}");
        }

        /// <summary>
        /// Одиночная отправка: singlecast
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SendMessageAsync(string? from, string message, string recipiendID)
        {
            await _server.SinglecastSendMessageAsync(from, message, ID, recipiendID);
            _logger.Log($"{from}: {message}");
        }

        /// <summary>
        /// Многоадресная рассылка: multicast
        /// </summary>
        /// <param name="from"> - от кого</param>
        /// <param name="message"> - текст сообщения</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SendMessageAsync(string? from, string message, List<string> recipiendsID)
        {
            await _server.MulticastSendMessageAsync(from, message, ID, recipiendsID);
            _logger.Log($"{from}: {message}");
        }
    }
}
