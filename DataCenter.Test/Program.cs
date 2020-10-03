using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buger404;
using DataArrange.Storages;

namespace Test
{
    [Serializable]
    public class Program
    {
        public struct NameReplace
        {
            public string name;
            public string rname;
        }
        static void Main(string[] args)
        {
            DataCenter d = new DataCenter("intallk_repeater",0);
            Storage w = new Storage("wordcollections");
            int c = int.Parse(w.getkey("repeat", "count"));
            int co = 0;
            List<NameReplace> nr = new List<NameReplace>();
            string wo = "",on = "",rn = "";
            Console.WriteLine("开始转移语录库...");
            foreach(string cs in File.ReadAllText(@"D:\\word-replace.txt").Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] ts = cs.Split(new string[] { "|||" }, StringSplitOptions.None);
                Console.WriteLine("读取：" + ts[0] + "->" + ts[1]);
                nr.Add(new NameReplace { name = ts[0], rname = ts[1] });
            }
            for (int i = 0; i < c; i++)
            {
                wo = w.getkey("repeat", "item" + i);
                on = w.getkey("owner" + i, "name");
                int ni = nr.FindIndex(m => m.name == on);
                if(ni == -1)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(wo);
                    Console.Write("为‘" + on + "’替换一个QQ：");
                    rn = Console.ReadLine();
                    nr.Add(new NameReplace { name = on, rname = rn });
                    File.AppendAllText(@"D:\\word-replace.txt", on + "|||" + rn + "\r\n");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    rn = nr[ni].rname;
                }
                if(rn != "")
                {
                    d["q" + rn, "count"] = (int)d["q" + rn, "count"] + 1;
                    d["q" + rn, d["q" + rn, "count"].ToString()] = wo;
                    co++;
                    Console.WriteLine("转录了语录" + i + "：" + wo + "-> q" + rn + "\\" + d["q" + rn, "count"].ToString());
                }
                else
                {
                    Console.WriteLine("忽略了语录" + i + "：" + wo);
                }
            }
            d["count"] = co;
            d.Write();
            Console.WriteLine("完成，共转录" + co + "条语录。");
            Console.ReadLine();
        }
    }
}
