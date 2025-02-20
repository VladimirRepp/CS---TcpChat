namespace TcpServerApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ServerObject server = new();   
            await server.ListenAsync(); // запускам сервер - ждем новых клиентов
        }
    }
}
