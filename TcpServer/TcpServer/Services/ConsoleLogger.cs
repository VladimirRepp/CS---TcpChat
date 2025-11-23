namespace TcpServer.Services
{
    internal class ConsoleLogger : ILogger
    {
        public void Log(string message, ELogType type = ELogType.Info)
        {
            Console.WriteLine($"[{type}]: {message}");
        }
    }
}

