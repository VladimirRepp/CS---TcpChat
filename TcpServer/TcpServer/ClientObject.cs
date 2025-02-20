using System.Net.Sockets;

namespace TcpServerApp
{
    internal class ClientObject
    {
        protected internal string ID { get; } = Guid.NewGuid().ToString();
        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }

        private string? _userName;

        private TcpClient _client;
        private ServerObject _server;

        public ClientObject(TcpClient client, ServerObject server)
        {
            _client = client;
            _server = server;

            var stream = client.GetStream();
            Reader = new(stream);
            Writer = new(stream);
        }

        /// <summary>
        /// Используется паттерн Update для цикличного прослушивания сообщений от клиента
        /// </summary>
        /// <returns></returns>
        public async Task UpdateAsync()
        {
            try
            {
                _userName = await Reader.ReadLineAsync();
                string? message = $"{_userName} - вошел в чат!";

                await SendMessageAsync(_userName, message);

                // В бесконечном цикле получаем сообщения от клиента
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
                        await SendMessageAsync(_userName, message);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClientObject.UpdateAsync(Exception): {ex.Message}");
            }
            finally
            {
                _server.RemoveConnectionById(ID);
            }
        }

        /// <summary>
        /// Закрытие ресурсов клиента
        /// </summary>
        protected internal void Close()
        {
            Reader.Close();
            Writer.Close();
            _client.Close();
        }

        /// <summary>
        /// Широковещательная отправка: broadcast
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendMessageAsync(string from, string message)
        {
            await _server.BroadcastSendMessageAsync(from, message, ID);
            Console.WriteLine($"{from}: " + message);
        }

        /// <summary>
        /// Одиночная отправка: singlecast
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="recipientID"></param>
        /// <returns></returns>
        private async Task SendMessageAsync(string from, string message, string recipientID)
        {
            await _server.SinglecastSendMessageAsync(from, message, ID, recipientID);
            Console.WriteLine($"{from}: " + message);
        }

        /// <summary>
        /// Многоадресная рассылка: multicast
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        /// <param name="recipientsID"></param>
        /// <returns></returns>
        private async Task SendMessageAsync(string from, string message, List<string> recipientsID)
        {
            await _server.MulticastSendMessageAsync(from, message, ID, recipientsID);
            Console.WriteLine($"{from}: " + message);
        }
    }

    public enum ETypeSendMessage
    {
        Broadcast,
        Singlecast,
        Muslticast
    }
}
