using System;

namespace Backend.API.Logging
{
    public static class AppLogger
    {
        public static void LogException(Exception exception, string context = "")
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{timestamp}] [ERROR] {context}");
            Console.WriteLine(exception.ToString());
            Console.ResetColor();
        }

        public static void LogWarning(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{timestamp}] [WARN] {message}");
            Console.ResetColor();
        }

        public static void LogInfo(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{timestamp}] [INFO] {message}");
            Console.ResetColor();
        }
    }
}
