namespace TcpServer.Services
{
    internal interface ILogger
    {
        public void Log(string message, ELogType type = ELogType.Info);
    }

    public enum ELogType
    {
        Info, 
        Error,
        Warning
    }
}

