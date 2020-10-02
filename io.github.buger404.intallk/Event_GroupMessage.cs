using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buger404;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;

namespace io.github.buger404.intallk.code
{
    public class Event_GroupMessage : IGroupMessage
    {
        List<Exception> errlog = new List<Exception>();
        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            try
            {
                if (DT.swi(e.FromGroup.Id, "Command")) Command(e);
            }
            catch(Exception err)
            {
                e.FromGroup.SendGroupMessage("故障：" + err.Message + "\n利用指令`.elog " + errlog.Count + "`取得详细信息。");
                errlog.Add(err);
            }
            
        }

        public void Command(CQGroupMessageEventArgs e)
        {
            if (!e.Message.Text.StartsWith(".")) return;
            #region Command Text Convert
            string[] r = e.Message.Text.Split(new string[] { "[CQ:at,qq=" },StringSplitOptions.None);
            string re = "";
            if(r.Length > 1)
            {
                for (int i = 0; i < r[1].Length; i++) { if (r[1][i] == ']') { r[1] = r[1].Remove(i,1); break; } }
                re = r[0] + r[1];
            }
            else
            {
                re = e.Message.Text;
            }
            string[] p = re.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            #endregion

            if (p[0] == ".bark" && DT.pm(e.FromQQ.Id, 0)) e.FromGroup.SendGroupMessage("汪");
            if (p[0] == ".save" && DT.pm(e.FromQQ.Id, 32767))
            {
                DT.pms.Write(); DT.sw.Write(); DT.bm.Write();
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "成功");
            }
            if (p[0] == ".pms")
            {
                if (p.Length == 1) e.FromGroup.SendGroupMessage(DT.pmi(e.FromQQ.Id));
                if (p.Length == 2) e.FromGroup.SendGroupMessage(DT.pmi(long.Parse(p[1])));
                if (p.Length == 3)
                {
                    if (DT.pmc(e.FromQQ.Id, long.Parse(p[1])))
                    {
                        if(int.Parse(p[2]) >= DT.pmi(e.FromQQ.Id))
                        {
                            e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "不能给予目标比自身高的权限");
                        }
                        else
                        {
                            DT.pms["q" + p[1], "permission"] = int.Parse(p[2]);
                            e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "成功更新目标权限");
                        }
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "您的权限必须高于目标对象");
                    }
                }
            }
            if (p[0] == ".lic")
            {
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "私聊查收：“黑嘴功能规范”");
                e.FromQQ.SendPrivateMessage(File.ReadAllText("C:\\.intallk\\License.txt"));
            }
            if (p[0] == ".elog" && DT.pm(e.FromQQ.Id, 0))
            {
                Exception err = errlog[int.Parse(p[1])];
                e.FromQQ.SendPrivateMessage(err.Message);
                e.FromQQ.SendPrivateMessage(err.StackTrace);
                e.FromQQ.SendPrivateMessage(err.Source);
                string o = "";
                if(err.Data.Count > 0)
                {
                    foreach (DictionaryEntry de in err.Data)
                    {
                        o += de.Key.ToString() + ":" + de.Value + "\n";
                    }
                }
                e.FromQQ.SendPrivateMessage(o);
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "私聊查收");
            }
            if (p[0] == ".or")
            {
                string o = "";
                for (int i = 0; i < e.Message.Text.Length; i++) o += e.Message.Text[i] + " ";
                e.FromGroup.SendGroupMessage(o);
            }
            if (p[0] == ".fea")
            {
                e.FromGroup.SendGroupMessage(re);
            }
        }
    }
}
