namespace TcpServer.Services
{
    internal interface ILogger
    {
        public void Log(string message, ELoggerType type = ELoggerType.Info);
    }

    public enum ELoggerType
    {
        Info, 
        Error,
        Warning
    }
}
