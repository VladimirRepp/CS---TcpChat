using TcpServer.Services;

namespace TcpServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            ServerObject server = new(logger);   

            // TODO: в потоке или в таске?
            await server.(); // запускам сервер - ждем новых клиентов
            await server.UpdateListenAsync(); // запускам сервер - ждем новых клиентов
        }
    }
}



