using Buger404;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.buger404.intallk.code
{
    public class DT
    {
        public static DataCenter bm = new DataCenter("intallk_general",0);
        public static DataCenter pms = new DataCenter("intallk_permission",0);
        public static DataCenter sw = new DataCenter("intallk_switch",0);
        public static DataCenter re = new DataCenter("intallk_repeater",0);

        public static bool swi(long group, string fun)
        {
            return ((int)sw["g" + group, fun] == 1);
        }
        public static bool pm(long qq, int require)
        {
            return (pmi(qq) >= require);
        }
        public static int pmi(long qq)
        {
            return ((int)pms["q" + qq, "permission"]);
        }
        public static bool pmc(long qq1, long qq2)
        {
            return (pmi(qq1) > pmi(qq2));
        }
        public static void log(string part,string content)
        {
            if (!Directory.Exists(@"C:\.dcenter\log")) Directory.CreateDirectory(@"C:\.dcenter\log");
            if (!Directory.Exists(@"C:\.dcenter\log\" + part)) Directory.CreateDirectory(@"C:\.dcenter\log\" + part);
            File.AppendAllText(@"C:\.dcenter\log\" + part + "\\" + part + " " + DateTime.Now.ToString("yyyy-MM-dd hh") + ".txt", content + "\r\n");
        }
    }
}
