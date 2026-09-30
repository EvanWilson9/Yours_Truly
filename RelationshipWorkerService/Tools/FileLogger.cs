using System;
using System.Collections.Generic;
using System.Text;

namespace RelationshipWorkerService.Tools
{
    public class FileLogger
    {

        private readonly string resLogPath = "C:\\Users\\evanw\\Services\\Test Logs\\RES Log.txt";
        public void writeToLog(string message)
        {
            File.AppendAllText(resLogPath, $"{message}\n");
        }
    }
}
