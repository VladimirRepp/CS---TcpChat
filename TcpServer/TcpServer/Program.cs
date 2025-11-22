using TcpServer.Services;

namespace TcpServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            ServerObject server = new(logger);   
            await server.ListenAsync(); // запускам сервер - ждем новых клиентов
        }
    }
}

