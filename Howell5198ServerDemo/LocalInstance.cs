using Howell5198.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HW5198Service
{
    class LocalInstance
    {
        public static SystemTimeInfo GetSystemTime()
        {
            return new SystemTimeInfo(DateTime.Now);
        }
    }
}
