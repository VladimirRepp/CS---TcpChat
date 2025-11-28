using System.Net.Sockets;

namespace TcpClientApp
{
    internal class Program
    {
        private static Messager _messager;

        static async Task Main(string[] args)
        {
            try
            {
                await _messager.Startup();
                
                Console.WriteLine("Введите имя пользователя: ");
                string userName = Console.ReadLine();

                Console.Clear();

                Console.WriteLine($"Добро пожаловать {userName}!");

                _messager = new(userName);

                // TODO: new Thread?
                Task.Run(() => UpdateReceiveMessageAsync());

                await UpdateSendMessageAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program.Main(Exception): {ex.Message}");
            }
        }

        /// <summary>
        /// Ассинхронная отправка сообщений 
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateSendMessageAsync()
        {
            await _messager.SendMessageAsync($"{_messager.UserName}");

            Console.WriteLine(ConsoleHelper.MSG_ForEnterMessage);
            while (true) 
            { 
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("Введите сообщение для отправки!");
                    continue;
                }

                await _messager.SendMessageAsync(message);
            }
        }

        /// <summary>
        /// Ассинхронное получение сообщений
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateReceiveMessageAsync()
        {
            while (true)
            {
                try
                {
                    string? message = await _messager.ReceiveMessageAsync();

                    if (string.IsNullOrEmpty(message))
                        continue;

                    Print(message);
                }
                catch
                {
                    break;
                }
            }
        }

        private static void Print(string message)
        {
            if (OperatingSystem.IsWindows())
            {
                var position = Console.GetCursorPosition();
                int left = position.Left;
                int top = position.Top;

                Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
                Console.SetCursorPosition(0, top);
                Console.WriteLine(message);
                Console.SetCursorPosition(left, top + 1);
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}

