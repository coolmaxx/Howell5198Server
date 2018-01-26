using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace HW5198Service
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new HW5198Service() 
            };
            ServiceBase.Run(ServicesToRun);
        }

        public static void PrintVersion()
        {
            ServiceEnvironment.Instance.Logger.Info(String.Format("Service Start Succeed.Version {0}.", ServiceEnvironment.Instance.FileVersion));
        }
    }
}
