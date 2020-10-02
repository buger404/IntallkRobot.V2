using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buger404;

namespace Test
{
    [Serializable]
    public class Program
    {
        static void Main(string[] args)
        {
            DataCenter d = new DataCenter("test");
            if(d["word"] == null)
            {
                Console.WriteLine("写入");
                d["word"] = "我最爱冰棍了！";
                d["number"] = 12345;
            }
            Console.WriteLine(d["word"]);
            Console.WriteLine(d["number"]);
            Console.ReadLine();
        }
    }
}
