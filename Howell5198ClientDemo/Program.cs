using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howell5198ClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            Howell5198ClientTest clienttest = new Howell5198ClientTest("127.0.0.1", 5198);//192.168.222.1
            clienttest.Run();
            Console.ReadLine();
        }
    }
}
