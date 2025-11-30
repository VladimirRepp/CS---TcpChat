using System.Net;
using TcpServer.Services;

namespace TcpServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ConsoleLogger logger = new();
            IPEndPoint endPoint = new(IPAddress.Parse("127.0.0.1"), 8888);
            ServerObject serverObject = new(endPoint, logger);

            await serverObject.StartupAsync();
        }
    }
}
