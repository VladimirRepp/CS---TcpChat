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
            await server.StartupAsync();         // запускам сервер
            await server.UpdateListenAsync();    // делаем прослушку
        }
    }
}
