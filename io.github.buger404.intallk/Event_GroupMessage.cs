using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buger404;
using Native.Sdk.Cqp;
using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;

namespace io.github.buger404.intallk.code
{
    public class Event_GroupMessage : IGroupMessage
    {
        List<Exception> errlog = new List<Exception>();
        public Random ran = new Random(new Guid().ToString().GetHashCode());
        public struct Msg
        {
            public long qq;
            public long group;
            public string content;
            public int repeat;
            public string repeaters;
            public int tick;
            public void Tick() { tick++; }
        }
        List<Msg> mq = new List<Msg>();
        public struct Request
        {
            public long groupid;
            public string mark;
            public string signs;
            public void sign(string qq)
            {
                signs += qq + ";";
            }
        }
        List<Request> req = new List<Request>();

        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            try
            {
                Switcher(e);
                Requester(e);
                if (DT.swi(e.FromGroup.Id, "RepeatRecord")) RepeatRecord(e);
                if (DT.swi(e.FromGroup.Id, "Command")) Command(e);
            }
            catch(Exception err)
            {
                #region Error Loging
                e.FromGroup.SendGroupMessage("故障：" + err.Message + "\n利用指令`.elog " + errlog.Count + "`取得详细信息。");
                errlog.Add(err);
                DT.log("intallk-faults",err.Message);
                DT.log("intallk-faults", err.StackTrace);
                DT.log("intallk-faults", err.Source);
                string o = "";
                if (err.Data.Count > 0)
                {
                    foreach (DictionaryEntry de in err.Data)
                    {
                        o += de.Key.ToString() + ":" + de.Value + "\n";
                    }
                }
                DT.log("intallk-faults", o);
                #endregion
            }

        }

        public void request(CQGroupMessageEventArgs e, string mark,string description)
        {
            int fi = req.FindIndex(m => m.groupid == e.FromGroup.Id && m.mark == mark);
            if (fi != -1)
            {
                e.FromGroup.SendGroupMessage("该请求已被申请，使用'.ac " + fi + "'同意申请。");
                return;
            }
            req.Add(new Request { groupid = e.FromGroup.Id, mark = mark, signs = "" });
            e.FromGroup.SendGroupMessage("黑嘴想要" + mark + "\n" + description + "\n\n如果同意，请三名管理员/群主发送'.ac " + (req.Count - 1) + "'同意申请。");
        }

        public void requestOK(CQGroupMessageEventArgs e, string mark)
        {
            if(mark == "记录复读发言")
            {
                DT.sw["g" + e.FromGroup.Id.ToString(), "RepeatRecord"] = 1;
                DT.sw.Write();
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "已开启功能：", "RepeatRecord");
            }
        }

        public void RepeatRecord(CQGroupMessageEventArgs e)
        {
            mq.Add(new Msg { qq = e.FromQQ.Id, group = e.FromGroup.Id, content = e.Message.Text, repeaters = "", repeat = 0 });
            int index = mq.FindIndex(m => m.group == e.FromGroup.Id && m.content == e.Message.Text && m.qq != e.FromQQ.Id && m.repeaters.Contains(e.FromQQ.Id + ";") == false);
            if(index != -1)
            {
                Msg m = mq[index];
                m.repeat += 1; m.repeaters += e.FromQQ.Id + ";"; mq[mq.Count - 1] = m;
                Console.WriteLine("heat the word:" + m.repeat);
                if (m.repeat == 3)
                {
                    Console.WriteLine("record!");
                    int qcount = (int)DT.re["q" + m.qq, "count"] + 1,
                        ccount = (int)DT.re["count"] + 1;
                    DT.re["q" + m.qq, "count"] = qcount;
                    DT.re["count"] = ccount;
                    DT.re["q" + m.qq, qcount.ToString()] = m.content;
                }
                mq.RemoveAt(index);
            }
            for(int i = 0;i < mq.Count; i++)
            {
                if (mq[i].group == e.FromGroup.Id) mq[i].Tick();
            }
            mq.RemoveAll(m => m.tick > 10);
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
            #region Command Recording
            DT.log("intallk-command", e.FromGroup.Id + "\\" + e.FromQQ.Id + "：" + re);
            #endregion

            #region Information Command
            if (p[0] == ".bark" && DT.pm(e.FromQQ.Id, 0)) e.FromGroup.SendGroupMessage("汪");
            if (p[0] == ".help" ) e.FromGroup.SendGroupMessage("https://buger404.gitee.io/web/blog.html?article=intallk-guidence");
            if (p[0] == ".lic") e.FromGroup.SendGroupMessage("https://buger404.gitee.io/web/blog.html?article=intallk-license");
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
            #endregion
            #region Repeat Record Command
            if (p[0] == ".ws" && DT.pm(e.FromQQ.Id, 0))
            {
                if (p.Length == 1) e.FromGroup.SendGroupMessage("共" + DT.re["count"] + "条语录");
                if (p.Length == 2) e.FromGroup.SendGroupMessage("对象共有" + DT.re["q" + p[1], "count"] + "条语录");
            }
            if (p[0] == ".w" && DT.pm(e.FromQQ.Id, 0))
            {
                if (p.Length == 1)
                {
                    List<DataCenter.DataItem> dl = DT.re.di.FindAll(m => m.name != "count");
                    DataCenter.DataItem d = dl[ran.Next(0, dl.Count)];
                    e.FromGroup.SendGroupMessage(
                        CQApi.CQCode_At(long.Parse(d.group.Remove(0,1))), "：", d.var.ToString()
                        );
                }
                if (p.Length == 2)
                {
                    int ind = 0,exc = int.Parse(p[1]);
                    for(int i = 0;i < DT.re.di.Count; i++)
                    {
                        if (DT.re.di[i].name != "count") ind++;
                        if (ind == exc)
                        {
                            e.FromGroup.SendGroupMessage(
                                CQApi.CQCode_At(long.Parse(DT.re.di[i].group.Remove(0, 1))), "：", DT.re.di[i].var.ToString()
                                );
                        }
                    }
                }
            }
            if (p[0] == ".wp" && DT.pm(e.FromQQ.Id, 0))
            {
                if (p.Length == 2)
                {
                    int index = ran.Next(1, (int)DT.re["q" + p[1],"count"] + 1);
                    e.FromGroup.SendGroupMessage(
                        CQApi.CQCode_At(long.Parse(p[1])), "：", DT.re["q" + p[1], index.ToString()].ToString()
                        );
                }
                if (p.Length == 3)
                {
                    e.FromGroup.SendGroupMessage(
                        CQApi.CQCode_At(long.Parse(p[1])), "：", DT.re["q" + p[1], p[2]].ToString()
                        );
                }
            }
            if (p[0] == ".wse" && DT.pm(e.FromQQ.Id, 0))
            {
                if (p.Length == 2)
                {
                    List<DataCenter.DataItem> dl = DT.re.di.FindAll(m => m.name != "count" && m.var.ToString().ToLower().Contains(p[1].ToLower()));
                    DataCenter.DataItem d = dl[ran.Next(0, dl.Count)];
                    e.FromGroup.SendGroupMessage(
                        CQApi.CQCode_At(long.Parse(d.group.Remove(0, 1))), "：", d.var.ToString()
                        );
                }
                if (p.Length == 3)
                {
                    List<DataCenter.DataItem> dl = DT.re.di.FindAll(m => m.name != "count" && m.group == "q" + p[2] && m.var.ToString().ToLower().Contains(p[1].ToLower()));
                    DataCenter.DataItem d = dl[ran.Next(0, dl.Count)];
                    e.FromGroup.SendGroupMessage(
                        CQApi.CQCode_At(long.Parse(d.group.Remove(0, 1))), "：", d.var.ToString()
                        );
                }
            }
            if (p[0] == ".t" && DT.pm(e.FromQQ.Id, 0))
            {
                string o = "";
                foreach(Msg m in mq.FindAll(m => m.group == e.FromGroup.Id))
                {
                    if(m.repeaters != "")
                    {
                        string n = "";
                        foreach(string qqs in m.repeaters.Split(';'))
                        {
                            if (qqs != "") n += GetGroupCard(long.Parse(qqs), e.FromGroup) + "，";
                        }
                        o += "（复读者：" + n + "）\n" + m.content + "\n";
                    }
                    else
                    {
                        o +=  "（" + GetGroupCard(m.qq, e.FromGroup) + "）\n" + m.content + "\n";
                    }
                }
                o = o.Replace("[CQ:at,", "[艾特,").Replace("[CQ:image,", "[图片,");
                o = o.Replace("[CQ:,", "[操作：");
                e.FromQQ.SendPrivateMessage(o);
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "私聊查收");
            }
            #endregion
            #region Debug Command
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
            if (p[0] == ".cmd")
            {
                string o = "param.length = " + p.Length + "\n";
                for(int i = 0;i < p.Length; i++)
                {
                    o += "p[" + i + "] = " + p[i] + "\n";
                }
                e.FromGroup.SendGroupMessage(o);
            }
            if (p[0] == ".elog" && DT.pm(e.FromQQ.Id, 0))
            {
                Exception err = errlog[int.Parse(p[1])];
                e.FromQQ.SendPrivateMessage(err.Message);
                e.FromQQ.SendPrivateMessage(err.StackTrace);
                e.FromQQ.SendPrivateMessage(err.Source);
                string o = "";
                if (err.Data.Count > 0)
                {
                    foreach (DictionaryEntry de in err.Data)
                    {
                        o += de.Key.ToString() + ":" + de.Value + "\n";
                    }
                }
                e.FromQQ.SendPrivateMessage(o);
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "私聊查收");
            }
            if (p[0] == ".save" && DT.pm(e.FromQQ.Id, 32767))
            {
                DT.pms.Write(); DT.sw.Write(); DT.bm.Write(); DT.re.Write();
                e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "成功");
            }
            #endregion
        }

        public void Switcher(CQGroupMessageEventArgs e)
        {
            if (!e.Message.Text.StartsWith(".")) return;
            string[] p = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (p[0] == ".sw" )
            {
                if (p.Length == 1)
                {
                    e.FromQQ.SendPrivateMessage("Command：黑嘴指令的使用\nRepeatRecord：复读语录记录");
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "私聊");
                }
                if (p.Length == 3)
                {
                    DT.log("intallk-switcher", e.FromGroup.Id + "\\" + e.FromQQ.Id + "：" + e.Message.Text);
                    int ma = int.Parse(p[2]);
                    if(p[1] != "RepeatRecord" && p[1] != "Command")
                    {
                        e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "不存在该功能");
                        return;
                    }
                    if(ma == 1)
                    {
                        if (p[1] == "RepeatRecord")
                        {
                            request(e, "记录复读发言", "黑嘴将会记住被复读的发言，形成语录集，留下回忆。");
                            return;
                        }
                        DT.sw["g" + e.FromGroup.Id.ToString(), p[1]] = 1;
                        DT.sw.Write();
                        e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "已开启功能：", p[1]);
                    }
                    else
                    {
                        DT.sw["g" + e.FromGroup.Id.ToString(), p[1]] = 0;
                        DT.sw.Write();
                        e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "已关闭功能：", p[1]);
                    }
                }
            }
        }

        public void Requester(CQGroupMessageEventArgs e)
        {
            if (!e.Message.Text.StartsWith(".")) return;
            string[] p = e.Message.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (p[0] == ".ac")
            {
                Request r = req[int.Parse(p[1])];
                if(r.groupid != e.FromGroup.Id || r.signs.Split(';').Length > 3)
                {
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "无效会话。");
                    return;
                }
                DT.log("intallk-permit", e.FromGroup.Id + "\\" + e.FromQQ.Id + "：（同意）" + r.mark);
                GroupMemberInfo gmi = e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id);
                if (r.signs.IndexOf(e.FromQQ.Id + ";") >= 0)
                {
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "请勿重复表决！");
                    return;
                }
                if (gmi.MemberType == QQGroupMemberType.Creator)
                {
                    r.sign(e.FromQQ.Id + ";" + e.FromQQ.Id + ";" + e.FromQQ.Id);
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "同意了黑嘴", r.mark, r.signs.Split(';').Length-1,"/3");
                    req[int.Parse(p[1])] = r;
                }
                if (gmi.MemberType == QQGroupMemberType.Manage)
                {
                    r.sign(e.FromQQ.Id.ToString());
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "同意了黑嘴", r.mark, r.signs.Split(';').Length-1, "/3");
                    req[int.Parse(p[1])] = r;
                }
                if (gmi.MemberType == QQGroupMemberType.Member)
                {
                    e.FromGroup.SendGroupMessage(e.FromQQ.CQCode_At(), "您无权表决");
                }
                if (r.signs.Split(';').Length > 3)
                {
                    e.FromGroup.SendGroupMessage("授权成功。");
                    requestOK(e, r.mark);
                }
            }
        }

        public string GetGroupCard(long qq,Group g)
        {
            GroupMemberInfo gmi = g.GetGroupMemberInfo(qq);
            string c = gmi.Card;
            if (c == "" || c == null) c = gmi.Nick;
            return c;
        }

    }
}
