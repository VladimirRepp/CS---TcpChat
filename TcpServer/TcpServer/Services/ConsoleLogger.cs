namespace TcpServer.Services
{
    internal class ConsoleLogger : ILogger
    {
        public void Log(string message, ELoggerType type = ELoggerType.Info)
        {
            Console.WriteLine($"[{type}]: {message}");
        }
    }
}
