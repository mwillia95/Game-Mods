using System.IO;

namespace DuplicantLifeExpectancy
{
    public static class Debugger
    {
        public static string file = @"C:/Users/Mitchell/Desktop/Mod Logs/error.txt";
        private static bool started = false;
        public static string debugFile = @"C:/Users/Mitchell/Desktop/Mod Logs/output_log.txt";
        private static bool debugStarted = false;

        public static void AddMessage(string message)
        {
            if (started)
            {
                AppendToFile(message);
                return;
            }
            started = true;
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(message);
            }
        }

        public static void AppendToFile(string message)
        {
            string initial = "";
            using (StreamReader sr = new StreamReader(file))
            {
                initial = sr.ReadToEnd();
            }
            using (StreamWriter sw = new StreamWriter(file))
            {
                sw.Write(initial + "\r\n" + message);
            }
        }

        public static void AddLog(string message)
        {
            if (debugStarted)
            {
                AppendToLog(message);
                return;
            }
            debugStarted = true;
            using (StreamWriter sw = new StreamWriter(debugFile))
            {
                sw.Write(message);
            }
        }

        public static void AppendToLog(string message)
        {
            string initial = "";
            using (StreamReader sr = new StreamReader(debugFile))
            {
                initial = sr.ReadToEnd();
            }
            using (StreamWriter sw = new StreamWriter(debugFile))
            {
                sw.Write(initial + "\r\n" + message);
            }
        }
    }
}
