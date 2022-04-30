using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirJump.Logging
{
    class AJLog
    {
        public static void Log(string content)
        {
            Console.WriteLine($"AirJump - {content} - {DateTime.Now}");
        }
    }
}
