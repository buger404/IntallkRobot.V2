using Buger404;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.buger404.intallk.code
{
    public class DT
    {
        public static DataCenter bm = new DataCenter("intallk_general");
        public static DataCenter pms = new DataCenter("intallk_permission");
        public static DataCenter sw = new DataCenter("intallk_switch");
        public static bool swi(long group, string fun)
        {
            object p = sw["g" + group, fun];
            if (p == null) return true;
            return ((int)p == 1);
        }
        public static bool pm(long qq, int require)
        {
            return (pmi(qq) >= require);
        }
        public static int pmi(long qq)
        {
            object p = pms["q" + qq, "permission"];
            if (p == null) return 0;
            return ((int)p);
        }
        public static bool pmc(long qq1, long qq2)
        {
            return (pmi(qq1) > pmi(qq2));
        }
        public static void log(string part,string content)
        {

        }
    }
}
