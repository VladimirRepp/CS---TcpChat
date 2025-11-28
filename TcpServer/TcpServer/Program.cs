using TcpServer.Services;

namespace TcpServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            IPEndPoint endPoint = new(IPAddress.Any, 8888);
            ServerObject server = new(endPoint, logger);   

            // TODO: в потоке или в таске?
            await server.StartupAsync();         // запускам сервер
            await server.UpdateListenAsync();    // делаем прослушку
        }
    }
}

