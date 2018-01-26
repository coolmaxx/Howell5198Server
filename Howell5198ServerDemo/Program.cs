using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HW5198Service
{
    class Program
    {
        static void Main(string[] args)
        {



            //Howell5198ClientTest clienttest = new Howell5198ClientTest("192.168.19.51", 5198);
            //clienttest.Run();
            //Console.ReadLine();

            Howell5198ServerAppInstance server = new Howell5198ServerAppInstance(ServiceConfiguration.Instance.HW5198ServerPort);
            server.Start();
            //Console.WriteLine("输入'q'停止服务");
            //while (Console.ReadKey().KeyChar != 'q')
            //{
            //    Console.WriteLine();
            //    continue;
            //}
            Console.ReadLine();
            server.Stop();
            Console.ReadLine();
        }
        //static void howell5198client_StreamDataReceived(object sender, StreamDataReceivedEventArgs e)
        //{
        //    Console.WriteLine("DataReceived: ChannelNo:{0} Freametype:{1} FrameData:{2}", ((Howell5198Stream)sender).ChannelNo, e.FrameType, System.Text.Encoding.ASCII.GetString(e.Data));
            
        //}
    }
}
