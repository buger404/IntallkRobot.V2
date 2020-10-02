using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Buger404
{
    public class DataCenter : IDisposable
    {
        [Serializable]
        public struct DataItem
        {
            public object var;
            public string group;
            public string name;
            public void Update(object obj)
            {
                var = obj;
            }
        }
        public object nullchar = null;
        public List<DataItem> di = new List<DataItem>();
        string dname = "default";
        DateTime SaveTime;
        static DataCenter()
        {
            Console.WriteLine("DataCenter Installed.");
            if (!Directory.Exists(@"C:\.dcenter")) Directory.CreateDirectory(@"C:\.dcenter");
            if (!Directory.Exists(@"C:\.dcenter\backup")) Directory.CreateDirectory(@"C:\.dcenter\backup");
        }
        public DataCenter(string name,object whennull = null)
        {
            nullchar = whennull;
            if (name.Contains('\\') || name.Contains('?') || name.Contains('/')
                || name.Contains('*') || name.Contains('|') || name.Contains('"')
                || name.Contains('<') || name.Contains('>') || name.Contains(':')
                ) throw new Exception("存档名中存在特殊字符。");
            dname = name;
            Console.WriteLine("DataCenter: " + name);
            Read();
            if (!Directory.Exists(@"C:\.dcenter\backup\" + name)) Directory.CreateDirectory(@"C:\.dcenter\backup\" + name);
            Write("C:\\.dcenter\\backup\\" + name + "\\" + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".dct");
            SaveTime = DateTime.Now;
        }
        public object this[string key]
        {
            get
            {
                return this["default",key];
            }
            set
            {
                this["default", key] = value;
            }
        }
        public object this[int index]
        {
            get
            {
                return this["", "", index];
            }
            set
            {
                this["", "", index] = value;
            }
        }
        public object this[string group,string key,int ind = -1]
        {
            get
            {
                int index = ind;
                if (index == -1) index = di.FindIndex(m => m.name == key && m.group == group);
                if (index == -1) return nullchar;
                return di[index].var;
            }
            set
            {
                if (key.Contains('\\')) throw new Exception("符号'\\'不被允许作为键名。");
                if (key.Contains('\\')) throw new Exception("符号'\\'不被允许作为组名。");
                int index = ind;
                if (index == -1) index = di.FindIndex(m => m.name == key && m.group == group);
                if (index == -1)
                {
                    Console.WriteLine("DataCenter: [new]" + group + "\\" + key);
                    di.Add(new DataItem { name = key, var = value,group = group });
                    return;
                }
                di[index].Update(value);
                if ((DateTime.Now - SaveTime).TotalSeconds >= 600)
                {
                    Console.WriteLine("DataCenter: Auto saved.");
                    Write("C:\\.dcenter\\backup\\" + dname + "\\" + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss") + ".dct");
                    Write();
                    SaveTime = DateTime.Now;
                }
            }
        }
        public void Write(string des = "")
        {
            string r = "";
            foreach(DataItem d in di)
            {
                MemoryStream m = new MemoryStream();
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(m, d.var);
                string base64 = Convert.ToBase64String(m.ToArray());
                r += $"{d.group}\\{d.name}：{base64}\n";
                m.Dispose();
            }
            string path = des;
            if (path == "") path = @"C:\.dcenter\" + dname + ".dct";
            Console.WriteLine("DataCenter: Saved(" + path + ")");
            File.WriteAllText(path, r);
        }
        public void Read()
        {
            if(!File.Exists("C:\\.dcenter\\" + dname + ".dct")) return;
            string[] r = File.ReadAllText(@"C:\.dcenter\" + dname + ".dct").Split('\n');
            di.Clear();
            try
            {
                foreach (string t in r)
                {
                    string[] tt = t.Split('：');
                    if (tt.Length == 2)
                    {
                        MemoryStream m = new MemoryStream(Convert.FromBase64String(tt[1]));
                        BinaryFormatter b = new BinaryFormatter();
                        object obj = b.Deserialize(m);
                        tt = tt[0].Split('\\');
                        di.Add(new DataItem { name = tt[1], var = obj, group = tt[0] });
                        m.Dispose();
                    }
                }
            }
            catch
            {
                throw new Exception("读取.dct时失败，可能元数据损坏。");
            }

        }

        public void Dispose()
        {
            Write();
        }

        ~DataCenter()
        {
            Dispose();
        }
    }
}
