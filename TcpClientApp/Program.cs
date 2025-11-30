using System.Diagnostics;
using System.Net;
using TcpClientApp;

class Programm
{
    static Messager _messanger;

    static async Task Main()
    {
        try
        {
            Console.WriteLine("Введите имя: ");
            string userName = Console.ReadLine();

            Console.Clear();

            Console.WriteLine("Подключение к серверру ...");

            IPEndPoint serverEndPoint = new(IPAddress.Parse("127.0.0.1"), 8888);
            _messanger = new(userName, serverEndPoint);

            await _messanger.Startup(Print);

            _messanger.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Main: " + ex.ToString());
        }
    }

    private static void Print(string message)
    {
        if (OperatingSystem.IsWindows())
        {
            var position = Console.GetCursorPosition();
            int left = position.Left;
            int top = position.Top;

            Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
            Console.SetCursorPosition(0, top);
            Console.WriteLine(message);
            Console.SetCursorPosition(left, top + 1);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}